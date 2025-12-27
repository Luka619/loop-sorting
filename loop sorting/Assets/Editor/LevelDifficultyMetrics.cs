using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LoopSorting.Editor
{
    public struct DsrMetrics
    {
        public float D;
        public float S;
        public float R;
        public int Colors;
        public int Boxes;
        public int Blocks;
        public int BeltCapacity;
    }

    public static class LevelDifficultyMetrics
    {
        private const int MaxColors = 6;
        private const int DefaultBeltCapacity = 50;
        private const int DefaultMaxBoxes = 8;
        private const float DefaultBlockSize = 0.6f;
        private const float DefaultBeltSlotSpacing = 0.6f;
        private const bool DefaultAutoResolveLayoutOverlap = true;
        private const float DefaultMinBoxToBeltGap = 0.08f;
        private const float DefaultPreferredBoxToBeltGap = 0.18f;
        private const float DefaultMinBoxToBoxGap = 0.05f;
        private const int DefaultOverlapResolveIterations = 3;
        private const float DefaultConveyorTickSeconds = 0.35f;
        private const float DefaultReleaseInterval = 0.12f;
        private const float StopFillRatioHigh = 0.9f;
        private const float StopFillRatioLow = 0.72f;
        private const float StrategyWeightE = 0.55f;
        private const float StrategyWeightG = 0.45f;
        private const float StrategyRandomJitter = 0.03f;
        private const int SimulationRuns = 80;
        private const int SimulationMaxTicks = 1600;
        private const double CacheCooldownSeconds = 0.25;

        private struct SimulationCacheEntry
        {
            public int Hash;
            public double Timestamp;
            public DsrMetrics Metrics;
        }

        private static readonly Dictionary<int, SimulationCacheEntry> CacheByLayout = new Dictionary<int, SimulationCacheEntry>();

        public static DsrMetrics Compute(LevelLayout layout, int maxBoxes = 0)
        {
            int layoutHash;
            var result = ComputeBaseMetrics(layout, maxBoxes, out layoutHash);
            if (layout == null)
            {
                return result;
            }

            if (TryGetCached(layout, layoutHash, out var cached))
            {
                return cached;
            }

            float r = Mathf.Clamp01(SimulateFailureRate(layout, result.BeltCapacity));
            result.R = r;
            result.D = Mathf.Clamp01(0.6f * result.S + 0.4f * result.R);

            StoreCachedMetrics(layout, result, layoutHash);
            return result;
        }

        public static DsrMetrics ComputeStatic(LevelLayout layout, int maxBoxes = 0, float overrideR = float.NaN)
        {
            int layoutHash;
            var result = ComputeBaseMetrics(layout, maxBoxes, out layoutHash);
            if (layout == null)
            {
                return result;
            }

            if (!float.IsNaN(overrideR))
            {
                result.R = Mathf.Clamp01(overrideR);
                result.D = Mathf.Clamp01(0.6f * result.S + 0.4f * result.R);
                return result;
            }

            if (TryGetCached(layout, layoutHash, out var cached))
            {
                return cached;
            }

            result.R = -1f;
            result.D = -1f;
            return result;
        }

        public static int GetLayoutHash(LevelLayout layout)
        {
            return layout == null ? 0 : ComputeLayoutHash(layout);
        }

        public static void StoreCachedMetrics(LevelLayout layout, DsrMetrics metrics)
        {
            if (layout == null) return;
            StoreCachedMetrics(layout, metrics, ComputeLayoutHash(layout));
        }

        public sealed class FailureRateSimulation
        {
            private readonly LevelLayout _sourceLayout;
            private readonly int _sourceHash;
            private readonly int _runsTotal;
            private int _runsCompleted;
            private int _runsStarted;
            private int _failures;
            private bool _done;
            private bool _cancelled;
            private bool _initialized;
            private LevelLayout _runtimeLayout;
            private bool _cleanupRuntime;
            private Dictionary<int, int> _portMapping;
            private int _beltLength;
            private int _seedBase;
            private SimulationRunState _currentRun;

            internal FailureRateSimulation(LevelLayout layout, int runsTotal)
            {
                _sourceLayout = layout;
                _sourceHash = layout == null ? 0 : ComputeLayoutHash(layout);
                _runsTotal = Mathf.Max(1, runsTotal);
            }

            public LevelLayout SourceLayout => _sourceLayout;
            public int LayoutHash => _sourceHash;
            public int RunsCompleted => _runsCompleted;
            public int RunsTotal => _runsTotal;
            public bool IsDone => _done;
            public bool Cancelled => _cancelled;
            public float FailureRate => _runsCompleted > 0 ? _failures / (float)_runsCompleted : 0f;
            public int RunsStarted => _runsStarted;

            public void Cancel()
            {
                if (_done) return;
                _cancelled = true;
                _done = true;
                Cleanup();
            }

            public bool TryGetDebugSnapshot(out SimulationDebugSnapshot snapshot)
            {
                snapshot = default;
                if (_currentRun == null || _currentRun.Game == null)
                {
                    return false;
                }

                var game = _currentRun.Game;
                int beltLength = game.Conveyor.Length;
                var beltSlots = new BlockColor?[beltLength];
                var slots = game.Conveyor.Slots;
                for (int i = 0; i < beltLength; i++)
                {
                    if (slots[i].HasValue)
                    {
                        beltSlots[i] = slots[i].Value.Color;
                    }
                }

                var containers = new SimulationContainerSnapshot[game.Containers.Count];
                for (int i = 0; i < game.Containers.Count; i++)
                {
                    var c = game.Containers[i];
                    BlockColor? front = c.Count > 0 ? c.Blocks[0].Color : (BlockColor?)null;
                    var blocks = new BlockColor?[c.Count];
                    for (int b = 0; b < c.Count; b++)
                    {
                        blocks[b] = c.Blocks[b].Color;
                    }
                    containers[i] = new SimulationContainerSnapshot(
                        i,
                        c.Count,
                        c.Capacity,
                        front,
                        blocks,
                        c.Locked,
                        c.Busy,
                        c.IsUniformAndFull());
                }

                snapshot = new SimulationDebugSnapshot(
                    _currentRun.RunIndex,
                    _currentRun.TickCount,
                    beltLength,
                    game.Conveyor.BlockCount,
                    _currentRun.NoInsertWhileFull,
                    _currentRun.IsReleasing,
                    _currentRun.ActiveReleaseIndex,
                    _currentRun.ActiveColor,
                    _currentRun.PendingRelease,
                    beltSlots,
                    containers);
                return true;
            }

            public bool Step(int maxTicks, double timeBudgetSeconds)
            {
                if (_done) return true;
                if (_cancelled)
                {
                    Cleanup();
                    _done = true;
                    return true;
                }

                if (!_initialized)
                {
                    if (!Initialize())
                    {
                        _done = true;
                        Cleanup();
                        return true;
                    }
                }

                int ticksThisStep = 0;
                double start = EditorApplication.timeSinceStartup;
                while (_runsCompleted < _runsTotal)
                {
                    if (maxTicks > 0 && ticksThisStep >= maxTicks) break;
                    if (timeBudgetSeconds > 0 &&
                        EditorApplication.timeSinceStartup - start >= timeBudgetSeconds)
                    {
                        break;
                    }

                    if (_currentRun == null)
                    {
                        _currentRun = CreateRunState(_runsStarted++);
                        if (_currentRun == null)
                        {
                            _done = true;
                            Cleanup();
                            return true;
                        }
                    }

                    var outcome = StepRunTick(_currentRun);
                    ticksThisStep++;
                    if (outcome == SimulationOutcome.None)
                    {
                        continue;
                    }

                    if (outcome == SimulationOutcome.Fail)
                    {
                        _failures++;
                    }

                    _runsCompleted++;
                    _currentRun = null;
                }

                if (_runsCompleted >= _runsTotal)
                {
                    _done = true;
                    Cleanup();
                }

                return _done;
            }

            private bool Initialize()
            {
                _initialized = true;
                if (_sourceLayout == null || _sourceLayout.boxes == null || _sourceLayout.boxes.Count == 0)
                {
                    return false;
                }

                _runtimeLayout = BuildRuntimeLayoutForSim(_sourceLayout);
                _cleanupRuntime = _runtimeLayout != null && _runtimeLayout != _sourceLayout;
                if (_runtimeLayout == null || _runtimeLayout.conveyors == null || _runtimeLayout.conveyors.Count == 0)
                {
                    return false;
                }

                var path = FindFirstConveyor(_runtimeLayout);
                if (path == null)
                {
                    return false;
                }

                _beltLength = _runtimeLayout.beltCapacity > 0 ? _runtimeLayout.beltCapacity : DefaultBeltCapacity;
                float spacing = _runtimeLayout.beltSlotSpacing > 0f ? _runtimeLayout.beltSlotSpacing : DefaultBeltSlotSpacing;

                var slots = LayoutUtils.BuildSlotsFromPath(
                    path,
                    spacing,
                    _beltLength,
                    out _,
                    smoothCorners: _runtimeLayout.smoothCorners,
                    smoothTension: _runtimeLayout.cornerSmoothTension,
                    smoothSubdivisions: _runtimeLayout.cornerSubdivisions,
                    out _);

                if (slots == null || slots.Count == 0)
                {
                    CleanupSlots(slots);
                    return false;
                }

                var slotPositions = new List<Vector3>(slots.Count);
                foreach (var t in slots)
                {
                    slotPositions.Add(t != null ? t.position : Vector3.zero);
                }
                CleanupSlots(slots);

                _portMapping = BuildContainerPorts(_runtimeLayout, slotPositions);
                _seedBase = ComputeLayoutHash(_runtimeLayout);
                return true;
            }

            private SimulationRunState CreateRunState(int runIndex)
            {
                if (_runtimeLayout == null || _runtimeLayout.boxes == null)
                {
                    return null;
                }

                var containers = new List<Container>(_runtimeLayout.boxes.Count);
                var boxLocked = new bool[_runtimeLayout.boxes.Count];
                var boxCompleted = new bool[_runtimeLayout.boxes.Count];

                for (int i = 0; i < _runtimeLayout.boxes.Count; i++)
                {
                    var spec = _runtimeLayout.boxes[i];
                    if (spec == null)
                    {
                        var dummy = new Container(1);
                        dummy.SetLocked(true);
                        containers.Add(dummy);
                        boxLocked[i] = true;
                        boxCompleted[i] = false;
                        continue;
                    }

                    int capacity = Mathf.Max(1, spec.columns * spec.rows);
                    var blocks = BuildBlocksForSpec(spec, capacity);
                    var container = new Container(capacity, blocks);
                    container.SetLocked(spec.locked);
                    containers.Add(container);
                    boxLocked[i] = spec.locked;
                    boxCompleted[i] = container.IsUniformAndFull();
                }

                var game = new LoopSortingGame(_beltLength, containers, _portMapping, _beltLength);
                int seed = _seedBase ^ (runIndex * 73856093);
                var rng = new System.Random(seed);
                var containerIndexByRef = new Dictionary<Container, int>(containers.Count);
                for (int i = 0; i < containers.Count; i++)
                {
                    if (containers[i] != null)
                    {
                        containerIndexByRef[containers[i]] = i;
                    }
                }

                return new SimulationRunState
                {
                    Game = game,
                    Layout = _runtimeLayout,
                    ContainerToBelt = _portMapping,
                    BoxLocked = boxLocked,
                    BoxCompleted = boxCompleted,
                    RunIndex = runIndex,
                    TickCount = 0,
                    BeltLength = _beltLength,
                    ReleaseAttemptsPerTick = Mathf.Max(1, Mathf.CeilToInt(DefaultConveyorTickSeconds / DefaultReleaseInterval)),
                    Rng = rng,
                    InboundStreamCounts = new int[containers.Count],
                    ContainerIndexByRef = containerIndexByRef
                };
            }

            private void Cleanup()
            {
                if (_cleanupRuntime && _runtimeLayout != null)
                {
                    UnityEngine.Object.DestroyImmediate(_runtimeLayout);
                }
                _runtimeLayout = null;
                _portMapping = null;
                _currentRun = null;
            }
        }

        public readonly struct SimulationDebugSnapshot
        {
            public readonly int RunIndex;
            public readonly int Tick;
            public readonly int BeltLength;
            public readonly int BeltCount;
            public readonly int NoInsertWhileFull;
            public readonly bool IsReleasing;
            public readonly int ActiveReleaseIndex;
            public readonly BlockColor ActiveReleaseColor;
            public readonly int PendingRelease;
            public readonly BlockColor?[] BeltSlots;
            public readonly SimulationContainerSnapshot[] Containers;

            public SimulationDebugSnapshot(
                int runIndex,
                int tick,
                int beltLength,
                int beltCount,
                int noInsertWhileFull,
                bool isReleasing,
                int activeReleaseIndex,
                BlockColor activeReleaseColor,
                int pendingRelease,
                BlockColor?[] beltSlots,
                SimulationContainerSnapshot[] containers)
            {
                RunIndex = runIndex;
                Tick = tick;
                BeltLength = beltLength;
                BeltCount = beltCount;
                NoInsertWhileFull = noInsertWhileFull;
                IsReleasing = isReleasing;
                ActiveReleaseIndex = activeReleaseIndex;
                ActiveReleaseColor = activeReleaseColor;
                PendingRelease = pendingRelease;
                BeltSlots = beltSlots;
                Containers = containers;
            }
        }

        public readonly struct SimulationContainerSnapshot
        {
            public readonly int Index;
            public readonly int Count;
            public readonly int Capacity;
            public readonly BlockColor? FrontColor;
            public readonly BlockColor?[] Blocks;
            public readonly bool Locked;
            public readonly bool Busy;
            public readonly bool Completed;

            public SimulationContainerSnapshot(
                int index,
                int count,
                int capacity,
                BlockColor? frontColor,
                BlockColor?[] blocks,
                bool locked,
                bool busy,
                bool completed)
            {
                Index = index;
                Count = count;
                Capacity = capacity;
                FrontColor = frontColor;
                Blocks = blocks;
                Locked = locked;
                Busy = busy;
                Completed = completed;
            }
        }

        private enum SimulationOutcome
        {
            None,
            Win,
            Fail
        }

        private sealed class SimulationRunState
        {
            public LoopSortingGame Game;
            public LevelLayout Layout;
            public Dictionary<int, int> ContainerToBelt;
            public bool[] BoxLocked;
            public bool[] BoxCompleted;
            public int RunIndex;
            public int TickCount;
            public bool IsReleasing;
            public int ActiveReleaseIndex;
            public BlockColor ActiveColor;
            public int PendingRelease;
            public bool CanClick = true;
            public int NoInsertWhileFull;
            public int NoProgressTicks;
            public int BeltLength;
            public System.Random Rng;
            public int ReleaseAttemptsPerTick;
            public int[] InboundStreamCounts;
            public Dictionary<Container, int> ContainerIndexByRef;
            public readonly List<ConveyorPortEvent> Events = new List<ConveyorPortEvent>(8);
        }

        private static SimulationOutcome StepRunTick(SimulationRunState state)
        {
            if (state == null || state.Game == null) return SimulationOutcome.Fail;
            state.TickCount++;

            RefreshInboundStreamCounts(state.Game, state.ContainerToBelt, state.InboundStreamCounts);
            RefreshContainerBusy(state.Game, state.IsReleasing, state.ActiveReleaseIndex);

            float fillRatio = state.Game.Conveyor.BlockCount / Mathf.Max(1f, state.BeltLength);
            if (state.CanClick && fillRatio >= StopFillRatioHigh) state.CanClick = false;
            if (!state.CanClick && fillRatio <= StopFillRatioLow) state.CanClick = true;
            if (!state.IsReleasing && !state.CanClick && state.NoProgressTicks >= state.BeltLength)
            {
                state.CanClick = true;
            }
            if (CountActiveColors(state.Game) <= 2)
            {
                state.CanClick = true;
            }

            if (!state.IsReleasing && state.CanClick)
            {
                int next = ChooseNextContainer(
                    state.Game,
                    state.ContainerToBelt,
                    state.BoxLocked,
                    state.BoxCompleted,
                    state.InboundStreamCounts,
                    state.BeltLength,
                    state.Rng);
                if (next >= 0)
                {
                    var container = state.Game.Containers[next];
                    if (container.TryPeek(out var first))
                    {
                        state.IsReleasing = true;
                        state.ActiveReleaseIndex = next;
                        state.ActiveColor = first.Color;
                        state.PendingRelease = CountFrontRun(container, state.ActiveColor);
                    }
                }
            }

            bool released = false;
            for (int s = 0; s < state.ReleaseAttemptsPerTick; s++)
            {
                if (!state.IsReleasing) break;
                var container = state.Game.Containers[state.ActiveReleaseIndex];
                if (state.PendingRelease <= 0 || !container.TryPeek(out var peek) || peek.Color != state.ActiveColor)
                {
                    EndRelease(container, ref state.IsReleasing, ref state.ActiveReleaseIndex, ref state.PendingRelease);
                    break;
                }

                var result = state.Game.TryReleaseFromContainer(state.ActiveReleaseIndex);
                if (result == ReleaseResult.Success)
                {
                    state.PendingRelease--;
                    released = true;
                    if (state.PendingRelease <= 0)
                    {
                        EndRelease(container, ref state.IsReleasing, ref state.ActiveReleaseIndex, ref state.PendingRelease);
                        break;
                    }
                }
                else if (result != ReleaseResult.BeltBlocked)
                {
                    EndRelease(container, ref state.IsReleasing, ref state.ActiveReleaseIndex, ref state.PendingRelease);
                    break;
                }
            }
            RefreshInboundStreamCounts(state.Game, state.ContainerToBelt, state.InboundStreamCounts);
            RefreshContainerBusy(state.Game, state.IsReleasing, state.ActiveReleaseIndex);

            state.Events.Clear();
            int? blockedPort = null;
            if (state.IsReleasing && state.ContainerToBelt != null &&
                state.ContainerToBelt.TryGetValue(state.ActiveReleaseIndex, out var blockedIdx))
            {
                blockedPort = blockedIdx;
            }
            state.Game.TickConveyor(blockedPort, state.Events, allowInsert: true);

            bool inserted = false;
            for (int i = 0; i < state.Events.Count; i++)
            {
                if (state.Events[i].Outcome != ConveyorPortOutcome.Inserted)
                {
                    continue;
                }

                inserted = true;
                if (state.ContainerIndexByRef != null &&
                    state.ContainerIndexByRef.TryGetValue(state.Events[i].Container, out var idx) &&
                    idx >= 0 && idx < state.Game.Containers.Count)
                {
                    UpdateInboundStreamCount(state.Game, state.ContainerToBelt, idx, state.Events[i].Block.Color, state.InboundStreamCounts);
                }
            }
            RefreshInboundStreamCounts(state.Game, state.ContainerToBelt, state.InboundStreamCounts);
            RefreshContainerBusy(state.Game, state.IsReleasing, state.ActiveReleaseIndex);

            UpdateLocks(state.Game, state.Layout, state.BoxLocked);
            UpdateCompletion(state.Game, state.BoxCompleted);

            if (IsSolved(state.Game))
            {
                return SimulationOutcome.Win;
            }

            if (inserted || released)
            {
                state.NoProgressTicks = 0;
            }
            else
            {
                state.NoProgressTicks++;
            }

            if (state.Game.Conveyor.BlockCount >= state.BeltLength && !inserted)
            {
                state.NoInsertWhileFull++;
            }
            else
            {
                state.NoInsertWhileFull = 0;
            }

            if (state.NoInsertWhileFull >= state.BeltLength)
            {
                return SimulationOutcome.Fail;
            }

            return SimulationOutcome.None;
        }

        public static FailureRateSimulation StartFailureRateSimulation(LevelLayout layout, int runs = SimulationRuns)
        {
            if (layout == null) return null;
            return new FailureRateSimulation(layout, runs);
        }

        private static bool TryGetCached(LevelLayout layout, int layoutHash, out DsrMetrics metrics)
        {
            metrics = default;
            int id = layout.GetInstanceID();
            if (!CacheByLayout.TryGetValue(id, out var cached))
            {
                return false;
            }

            if (cached.Hash != layoutHash)
            {
                return false;
            }

            if (EditorApplication.timeSinceStartup - cached.Timestamp > CacheCooldownSeconds)
            {
                return false;
            }

            metrics = cached.Metrics;
            return true;
        }

        private static void StoreCachedMetrics(LevelLayout layout, DsrMetrics metrics, int layoutHash)
        {
            CacheByLayout[layout.GetInstanceID()] = new SimulationCacheEntry
            {
                Hash = layoutHash,
                Timestamp = EditorApplication.timeSinceStartup,
                Metrics = metrics
            };
        }

        private static DsrMetrics ComputeBaseMetrics(LevelLayout layout, int maxBoxes, out int layoutHash)
        {
            var result = new DsrMetrics();
            layoutHash = layout == null ? 0 : ComputeLayoutHash(layout);
            if (layout == null)
            {
                return result;
            }

            var boxes = layout.boxes;
            int boxCount = boxes != null ? boxes.Count : 0;
            int[] colorTotals = new int[MaxColors];
            int totalBlocks = 0;
            float boxMixSum = 0f;
            int boxMixCount = 0;

            if (boxes != null)
            {
                foreach (var box in boxes)
                {
                    if (box == null) continue;
                    int capacity = Mathf.Max(0, box.columns * box.rows);
                    int boxTotal = 0;
                    int maxColor = 0;

                    if (box.colorCounts != null)
                    {
                        foreach (var cc in box.colorCounts)
                        {
                            if (cc == null) continue;
                            int count = Mathf.Max(0, cc.count);
                            if (capacity > 0 && boxTotal + count > capacity)
                            {
                                count = capacity - boxTotal;
                            }
                            if (count <= 0) continue;

                            int colorIndex = (int)cc.color;
                            if (colorIndex >= 0 && colorIndex < colorTotals.Length)
                            {
                                colorTotals[colorIndex] += count;
                            }

                            boxTotal += count;
                            if (count > maxColor) maxColor = count;
                        }
                    }

                    if (boxTotal > 0)
                    {
                        boxMixSum += 1f - (maxColor / (float)boxTotal);
                        boxMixCount++;
                        totalBlocks += boxTotal;
                    }
                }
            }

            int colorCount = 0;
            for (int i = 0; i < colorTotals.Length; i++)
            {
                if (colorTotals[i] > 0) colorCount++;
            }

            float sColor = colorCount <= 1 ? 0f : Mathf.Clamp01((colorCount - 1f) / (MaxColors - 1f));
            float mixRaw = 0f;
            if (totalBlocks > 0)
            {
                for (int i = 0; i < colorTotals.Length; i++)
                {
                    if (colorTotals[i] <= 0) continue;
                    float p = colorTotals[i] / (float)totalBlocks;
                    mixRaw += p * p;
                }
            }
            float sMix = colorCount <= 1 ? 0f : (1f - mixRaw) / (1f - 1f / Mathf.Max(1f, colorCount));
            sMix = Mathf.Clamp01(sMix);

            float sBoxMix = boxMixCount > 0 ? Mathf.Clamp01(boxMixSum / boxMixCount) : 0f;

            int maxBoxesUsed = maxBoxes > 1 ? maxBoxes : DefaultMaxBoxes;
            float sBoxes = boxCount <= 1 ? 0f : Mathf.Clamp01((boxCount - 1f) / (maxBoxesUsed - 1f));

            float s = Mathf.Clamp01(0.35f * sColor + 0.35f * sMix + 0.2f * sBoxMix + 0.1f * sBoxes);

            int beltCapacity = layout.beltCapacity > 0 ? layout.beltCapacity : DefaultBeltCapacity;

            result.S = s;
            result.R = 0f;
            result.D = 0f;
            result.Colors = colorCount;
            result.Boxes = boxCount;
            result.Blocks = totalBlocks;
            result.BeltCapacity = beltCapacity;
            return result;
        }

        private static int ComputeLayoutHash(LevelLayout layout)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (layout.beltCapacity);
                hash = hash * 31 + layout.beltSlotSpacing.GetHashCode();
                hash = hash * 31 + layout.smoothCorners.GetHashCode();
                hash = hash * 31 + layout.cornerSmoothTension.GetHashCode();
                hash = hash * 31 + layout.cornerSubdivisions.GetHashCode();
                hash = hash * 31 + layout.blockSize.GetHashCode();
                hash = hash * 31 + layout.overrideLayoutAutoSettings.GetHashCode();
                hash = hash * 31 + layout.autoResolveLayoutOverlap.GetHashCode();
                hash = hash * 31 + layout.minBoxToBeltGap.GetHashCode();
                hash = hash * 31 + layout.preferredBoxToBeltGap.GetHashCode();
                hash = hash * 31 + layout.minBoxToBoxGap.GetHashCode();
                hash = hash * 31 + layout.overlapResolveIterations.GetHashCode();
                hash = hash * 31 + layout.cameraMaxOrthoSize.GetHashCode();
                hash = hash * 31 + layout.minBlockPixelSize.GetHashCode();

                if (layout.conveyors != null)
                {
                    hash = hash * 31 + layout.conveyors.Count;
                    foreach (var conveyor in layout.conveyors)
                    {
                        if (conveyor == null)
                        {
                            hash = hash * 31 + 17;
                            continue;
                        }
                        hash = hash * 31 + conveyor.loop.GetHashCode();
                        hash = hash * 31 + conveyor.width.GetHashCode();
                        if (conveyor.points != null)
                        {
                            hash = hash * 31 + conveyor.points.Count;
                            foreach (var pt in conveyor.points)
                            {
                                hash = hash * 31 + Mathf.RoundToInt(pt.x * 1000f);
                                hash = hash * 31 + Mathf.RoundToInt(pt.y * 1000f);
                            }
                        }
                    }
                }

                if (layout.boxes != null)
                {
                    hash = hash * 31 + layout.boxes.Count;
                    foreach (var box in layout.boxes)
                    {
                        if (box == null)
                        {
                            hash = hash * 31 + 23;
                            continue;
                        }
                        hash = hash * 31 + Mathf.RoundToInt(box.position.x * 1000f);
                        hash = hash * 31 + Mathf.RoundToInt(box.position.y * 1000f);
                        hash = hash * 31 + box.columns;
                        hash = hash * 31 + box.rows;
                        hash = hash * 31 + (int)box.opening;
                        hash = hash * 31 + box.autoAlignSlot.GetHashCode();
                        hash = hash * 31 + box.beltSlotIndex;
                        hash = hash * 31 + box.locked.GetHashCode();
                        hash = hash * 31 + (int)box.unlockColor;
                        if (box.colorCounts != null)
                        {
                            hash = hash * 31 + box.colorCounts.Count;
                            foreach (var cc in box.colorCounts)
                            {
                                if (cc == null)
                                {
                                    hash = hash * 31 + 29;
                                    continue;
                                }
                                hash = hash * 31 + (int)cc.color;
                                hash = hash * 31 + cc.count;
                                hash = hash * 31 + cc.hidden.GetHashCode();
                            }
                        }
                    }
                }

                return hash;
            }
        }

        private static float SimulateFailureRate(LevelLayout sourceLayout, int beltCapacity)
        {
            if (sourceLayout == null || sourceLayout.boxes == null || sourceLayout.boxes.Count == 0)
            {
                return 0f;
            }

            var runtimeLayout = BuildRuntimeLayoutForSim(sourceLayout);
            bool cleanupRuntime = runtimeLayout != null && runtimeLayout != sourceLayout;
            try
            {
                if (runtimeLayout == null || runtimeLayout.conveyors == null || runtimeLayout.conveyors.Count == 0)
                {
                    return 0f;
                }

                var path = FindFirstConveyor(runtimeLayout);
                if (path == null)
                {
                    return 0f;
                }

                int beltLength = beltCapacity > 0 ? beltCapacity : DefaultBeltCapacity;
                float spacing = runtimeLayout.beltSlotSpacing > 0f ? runtimeLayout.beltSlotSpacing : DefaultBeltSlotSpacing;

                var slots = LayoutUtils.BuildSlotsFromPath(
                    path,
                    spacing,
                    beltLength,
                    out _,
                    smoothCorners: runtimeLayout.smoothCorners,
                    smoothTension: runtimeLayout.cornerSmoothTension,
                    smoothSubdivisions: runtimeLayout.cornerSubdivisions,
                    out _);

                if (slots == null || slots.Count == 0)
                {
                    CleanupSlots(slots);
                    return 0f;
                }

                var slotPositions = new List<Vector3>(slots.Count);
                foreach (var t in slots)
                {
                    slotPositions.Add(t != null ? t.position : Vector3.zero);
                }
                CleanupSlots(slots);

                var portMapping = BuildContainerPorts(runtimeLayout, slotPositions);

                int failures = 0;
                int runs = Mathf.Max(1, SimulationRuns);
                int seedBase = ComputeLayoutHash(runtimeLayout);

                for (int run = 0; run < runs; run++)
                {
                    int seed = seedBase ^ (run * 73856093);
                    var rng = new System.Random(seed);
                    if (SimulateRun(runtimeLayout, portMapping, beltLength, rng))
                    {
                        failures++;
                    }
                }

                return failures / (float)runs;
            }
            finally
            {
                if (cleanupRuntime)
                {
                    UnityEngine.Object.DestroyImmediate(runtimeLayout);
                }
            }
        }

        private static bool SimulateRun(LevelLayout layout, Dictionary<int, int> portMapping, int beltLength, System.Random rng)
        {
            if (layout == null || layout.boxes == null || layout.boxes.Count == 0 || beltLength <= 0)
            {
                return false;
            }

            var containers = new List<Container>(layout.boxes.Count);
            var containerToBelt = portMapping ?? new Dictionary<int, int>();

            for (int i = 0; i < layout.boxes.Count; i++)
            {
                var spec = layout.boxes[i];
                if (spec == null) continue;

                int capacity = Mathf.Max(1, spec.columns * spec.rows);

                var blocks = BuildBlocksForSpec(spec, capacity);
                var container = new Container(capacity, blocks);
                container.SetLocked(spec.locked);
                containers.Add(container);
            }

            var game = new LoopSortingGame(beltLength, containers, containerToBelt, beltLength);
            var containerIndexByRef = new Dictionary<Container, int>(containers.Count);
            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i] != null)
                {
                    containerIndexByRef[containers[i]] = i;
                }
            }
            var inboundStreamCounts = new int[containers.Count];
            var boxLocked = new bool[containers.Count];
            var boxCompleted = new bool[containers.Count];
            for (int i = 0; i < containers.Count; i++)
            {
                boxLocked[i] = layout.boxes[i].locked;
                boxCompleted[i] = containers[i].IsUniformAndFull();
            }

            bool isReleasing = false;
            int activeReleaseIndex = -1;
            BlockColor activeColor = BlockColor.Red;
            int pendingRelease = 0;
            bool canClick = true;
            int noInsertWhileFull = 0;
            int noProgressTicks = 0;
            int maxTicks = Mathf.Max(200, SimulationMaxTicks);
            int releaseAttemptsPerTick = Mathf.Max(1, Mathf.CeilToInt(DefaultConveyorTickSeconds / DefaultReleaseInterval));

            var events = new List<ConveyorPortEvent>(8);

            for (int tick = 0; tick < maxTicks; tick++)
            {
                RefreshInboundStreamCounts(game, containerToBelt, inboundStreamCounts);
                RefreshContainerBusy(game, isReleasing, activeReleaseIndex);

                float fillRatio = game.Conveyor.BlockCount / Mathf.Max(1f, beltLength);
                if (canClick && fillRatio >= StopFillRatioHigh) canClick = false;
                if (!canClick && fillRatio <= StopFillRatioLow) canClick = true;
                if (!isReleasing && !canClick && noProgressTicks >= beltLength)
                {
                    canClick = true;
                }
                if (CountActiveColors(game) <= 2)
                {
                    canClick = true;
                }

                if (!isReleasing && canClick)
                {
                    int next = ChooseNextContainer(
                        game,
                        containerToBelt,
                        boxLocked,
                        boxCompleted,
                        inboundStreamCounts,
                        beltLength,
                        rng);
                    if (next >= 0)
                    {
                        var container = game.Containers[next];
                        if (container.TryPeek(out var first))
                        {
                            isReleasing = true;
                            activeReleaseIndex = next;
                            activeColor = first.Color;
                            pendingRelease = CountFrontRun(container, activeColor);
                        }
                    }
                }

                bool released = false;
                for (int s = 0; s < releaseAttemptsPerTick; s++)
                {
                    if (!isReleasing) break;
                    var container = game.Containers[activeReleaseIndex];
                    if (pendingRelease <= 0 || !container.TryPeek(out var peek) || peek.Color != activeColor)
                    {
                        EndRelease(container, ref isReleasing, ref activeReleaseIndex, ref pendingRelease);
                        break;
                    }

                    var result = game.TryReleaseFromContainer(activeReleaseIndex);
                    if (result == ReleaseResult.Success)
                    {
                        pendingRelease--;
                        released = true;
                        if (pendingRelease <= 0)
                        {
                            EndRelease(container, ref isReleasing, ref activeReleaseIndex, ref pendingRelease);
                            break;
                        }
                    }
                    else if (result != ReleaseResult.BeltBlocked)
                    {
                        EndRelease(container, ref isReleasing, ref activeReleaseIndex, ref pendingRelease);
                        break;
                    }
                }
                RefreshInboundStreamCounts(game, containerToBelt, inboundStreamCounts);
                RefreshContainerBusy(game, isReleasing, activeReleaseIndex);

                events.Clear();
                int? blockedPort = null;
                if (isReleasing && containerToBelt.TryGetValue(activeReleaseIndex, out var blockedIdx))
                {
                    blockedPort = blockedIdx;
                }
                game.TickConveyor(blockedPort, events, allowInsert: true);
                bool inserted = false;
                for (int i = 0; i < events.Count; i++)
                {
                    if (events[i].Outcome != ConveyorPortOutcome.Inserted)
                    {
                        continue;
                    }

                    inserted = true;
                    if (containerIndexByRef.TryGetValue(events[i].Container, out var idx) &&
                        idx >= 0 && idx < game.Containers.Count)
                    {
                        UpdateInboundStreamCount(game, containerToBelt, idx, events[i].Block.Color, inboundStreamCounts);
                    }
                }
                RefreshInboundStreamCounts(game, containerToBelt, inboundStreamCounts);
                RefreshContainerBusy(game, isReleasing, activeReleaseIndex);

                UpdateLocks(game, layout, boxLocked);
                UpdateCompletion(game, boxCompleted);

                if (IsSolved(game))
                {
                    return false;
                }

                if (inserted || released)
                {
                    noProgressTicks = 0;
                }
                else
                {
                    noProgressTicks++;
                }

                if (game.Conveyor.BlockCount >= beltLength && !inserted)
                {
                    noInsertWhileFull++;
                }
                else
                {
                    noInsertWhileFull = 0;
                }

                if (noInsertWhileFull >= beltLength)
                {
                    return true;
                }
            }

            return false;
        }

        private static LevelLayout BuildRuntimeLayoutForSim(LevelLayout source)
        {
            if (source == null) return null;
            var runtimeLayout = source;

            bool overrideLayout = source.overrideLayoutAutoSettings;
            bool autoResolve = overrideLayout ? source.autoResolveLayoutOverlap : DefaultAutoResolveLayoutOverlap;
            float minGap = overrideLayout ? source.minBoxToBeltGap : DefaultMinBoxToBeltGap;
            float preferredGap = overrideLayout ? source.preferredBoxToBeltGap : DefaultPreferredBoxToBeltGap;
            float minBoxGap = overrideLayout ? source.minBoxToBoxGap : DefaultMinBoxToBoxGap;
            int iterations = overrideLayout ? source.overlapResolveIterations : DefaultOverlapResolveIterations;

            if (autoResolve && (minGap > 0f || preferredGap > 0f || minBoxGap > 0f))
            {
                runtimeLayout = LayoutUtils.CloneLayout(source);
                int fixIterations = Mathf.Clamp(iterations, 1, 8);
                for (int i = 0; i < fixIterations; i++)
                {
                    bool moved = false;
                    if (minGap > 0f || preferredGap > 0f)
                    {
                        moved |= LayoutUtils.ResolveBoxBeltOverlap(
                            runtimeLayout,
                            minGap,
                            preferredGap,
                            DefaultBeltSlotSpacing,
                            1) > 0;
                    }
                    if (minBoxGap > 0f)
                    {
                        moved |= LayoutUtils.ResolveBoxBoxOverlap(runtimeLayout, minBoxGap, 1) > 0;
                    }
                    if (!moved) break;
                }
            }

            return runtimeLayout;
        }

        private static ConveyorPath FindFirstConveyor(LevelLayout layout)
        {
            if (layout == null || layout.conveyors == null) return null;
            foreach (var c in layout.conveyors)
            {
                if (c != null && c.points != null && c.points.Count >= 2)
                {
                    return c;
                }
            }
            return null;
        }

        private static Dictionary<int, int> BuildContainerPorts(LevelLayout layout, List<Vector3> slotPositions)
        {
            var mapping = new Dictionary<int, int>();
            if (layout == null || layout.boxes == null || slotPositions == null || slotPositions.Count == 0)
            {
                return mapping;
            }

            float unit = layout.blockSize > 0f ? layout.blockSize : DefaultBlockSize;
            var reserved = new HashSet<int>();
            var autoAvoid = new HashSet<int>();
            if (slotPositions.Count > 1)
            {
                autoAvoid.Add(0);
            }

            var slotTransforms = BuildSlotTransforms(slotPositions);
            for (int i = 0; i < layout.boxes.Count; i++)
            {
                var spec = layout.boxes[i];
                if (spec == null) continue;

                int slotIndex;
                if (spec.autoAlignSlot)
                {
                    slotIndex = LayoutUtils.ResolveBeltSlotIndex(spec, slotTransforms, unit, reserved, autoAvoid);
                }
                else
                {
                    slotIndex = Mathf.Clamp(spec.beltSlotIndex, 0, Mathf.Max(0, slotPositions.Count - 1));
                    if (reserved.Contains(slotIndex) && slotPositions.Count > 1)
                    {
                        int bestAlt = slotIndex;
                        int maxScan = Mathf.Max(1, slotPositions.Count - 1);
                        for (int delta = 1; delta <= maxScan; delta++)
                        {
                            int a = (slotIndex + delta) % slotPositions.Count;
                            int b = (slotIndex - delta + slotPositions.Count) % slotPositions.Count;
                            if (!reserved.Contains(a)) { bestAlt = a; break; }
                            if (!reserved.Contains(b)) { bestAlt = b; break; }
                        }
                        slotIndex = bestAlt;
                    }
                }

                mapping[i] = slotIndex;
                reserved.Add(slotIndex);
            }

            CleanupSlots(slotTransforms);
            return mapping;
        }

        private static void CleanupSlots(List<Transform> slots)
        {
            if (slots == null) return;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(slots[i].gameObject);
                }
            }
        }

        private static IEnumerable<Block> BuildBlocksForSpec(BoxSpec spec, int capacity)
        {
            if (spec == null || spec.colorCounts == null) yield break;
            int sorted = 0;
            for (int idx = 0; idx < spec.colorCounts.Count && sorted < capacity; idx++)
            {
                var cc = spec.colorCounts[idx];
                if (cc == null) continue;
                int cnt = Mathf.Max(0, cc.count);
                for (int i = 0; i < cnt && sorted < capacity; i++)
                {
                    yield return new Block(cc.color, cc.hidden);
                    sorted++;
                }
            }
        }

        private static void RefreshContainerBusy(LoopSortingGame game, bool isReleasing, int activeReleaseIndex)
        {
            if (game == null) return;

            int count = game.Containers.Count;
            for (int i = 0; i < count; i++)
            {
                bool releaseBusy = isReleasing && activeReleaseIndex == i;
                game.Containers[i].SetBusy(releaseBusy);
            }
        }

        private static void UpdateInboundStreamCount(
            LoopSortingGame game,
            Dictionary<int, int> containerToBelt,
            int containerIndex,
            BlockColor color,
            int[] inboundStreamCounts)
        {
            if (game == null || inboundStreamCounts == null) return;
            if (containerIndex < 0 || containerIndex >= game.Containers.Count) return;
            if (containerIndex >= inboundStreamCounts.Length) return;
            if (containerToBelt == null || !containerToBelt.TryGetValue(containerIndex, out var beltIndex)) return;

            var container = game.Containers[containerIndex];
            if (container == null)
            {
                inboundStreamCounts[containerIndex] = 0;
                return;
            }

            int remaining = Mathf.Max(0, container.Capacity - container.Count);
            if (remaining <= 0)
            {
                inboundStreamCounts[containerIndex] = 0;
                return;
            }

            int queued = CountQueuedIncomingBlocks(game.Conveyor.Slots, beltIndex, color, remaining);
            inboundStreamCounts[containerIndex] = queued;
        }

        private static void RefreshInboundStreamCounts(
            LoopSortingGame game,
            Dictionary<int, int> containerToBelt,
            int[] inboundStreamCounts)
        {
            if (game == null || inboundStreamCounts == null) return;

            int max = Mathf.Min(inboundStreamCounts.Length, game.Containers.Count);
            for (int i = 0; i < max; i++)
            {
                if (inboundStreamCounts[i] <= 0) continue;
                var container = game.Containers[i];
                if (container == null || container.Count == 0)
                {
                    inboundStreamCounts[i] = 0;
                    continue;
                }

                UpdateInboundStreamCount(game, containerToBelt, i, container.Blocks[0].Color, inboundStreamCounts);
            }
        }

        private static int CountQueuedIncomingBlocks(
            IReadOnlyList<Block?> slots,
            int beltIndex,
            BlockColor color,
            int maxCount)
        {
            if (slots == null || slots.Count <= 1 || maxCount <= 0) return 0;

            int length = slots.Count;
            int count = 0;
            int idx = (beltIndex - 1 + length) % length;
            while (idx != beltIndex && count < maxCount)
            {
                var slot = slots[idx];
                if (!slot.HasValue)
                {
                    idx = (idx - 1 + length) % length;
                    continue;
                }
                if (slot.Value.Color != color) break;
                count++;
                idx = (idx - 1 + length) % length;
            }
            return count;
        }

        private static int CountFrontRun(Container container, BlockColor color)
        {
            int run = 0;
            for (int i = 0; i < container.Count; i++)
            {
                if (container.Blocks[i].Color == color) run++; else break;
            }
            return run;
        }

        private static void EndRelease(Container container, ref bool isReleasing, ref int activeReleaseIndex, ref int pendingRelease)
        {
            isReleasing = false;
            activeReleaseIndex = -1;
            pendingRelease = 0;
        }

        private static void UpdateLocks(LoopSortingGame game, LevelLayout layout, bool[] boxLocked)
        {
            var completedColors = new HashSet<BlockColor>();
            for (int i = 0; i < game.Containers.Count; i++)
            {
                if (i < boxLocked.Length && boxLocked[i]) continue;
                var c = game.Containers[i];
                if (c.IsUniformAndFull() && c.Blocks.Count > 0)
                {
                    completedColors.Add(c.Blocks[0].Color);
                }
            }

            for (int i = 0; i < game.Containers.Count; i++)
            {
                bool currentlyLocked = i < boxLocked.Length && boxLocked[i];
                bool shouldUnlock = false;
                if (currentlyLocked && layout != null && layout.boxes != null && i < layout.boxes.Count && layout.boxes[i] != null)
                {
                    shouldUnlock = completedColors.Contains(layout.boxes[i].unlockColor);
                }

                bool finalLocked = currentlyLocked && !shouldUnlock;
                if (i < boxLocked.Length) boxLocked[i] = finalLocked;
                game.Containers[i].SetLocked(finalLocked);
            }
        }

        private static void UpdateCompletion(LoopSortingGame game, bool[] boxCompleted)
        {
            for (int i = 0; i < game.Containers.Count; i++)
            {
                if (i < boxCompleted.Length)
                {
                    boxCompleted[i] = game.Containers[i].IsUniformAndFull();
                }
            }
        }

        private static bool IsSolved(LoopSortingGame game)
        {
            for (int i = 0; i < game.Containers.Count; i++)
            {
                var c = game.Containers[i];
                if (c.Count == 0) continue;
                if (!c.IsUniformAndFull()) return false;
            }
            return game.Conveyor.BlockCount == 0;
        }

        private static int CountActiveColors(LoopSortingGame game)
        {
            if (game == null) return 0;

            var present = new bool[MaxColors];
            int count = 0;

            var slots = game.Conveyor.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].HasValue) continue;
                int idx = (int)slots[i].Value.Color;
                if (idx < 0 || idx >= MaxColors || present[idx]) continue;
                present[idx] = true;
                count++;
                if (count >= MaxColors) return count;
            }

            for (int i = 0; i < game.Containers.Count; i++)
            {
                var c = game.Containers[i];
                if (c == null || c.Count == 0) continue;
                var blocks = c.Blocks;
                for (int b = 0; b < blocks.Count; b++)
                {
                    int idx = (int)blocks[b].Color;
                    if (idx < 0 || idx >= MaxColors || present[idx]) continue;
                    present[idx] = true;
                    count++;
                    if (count >= MaxColors) return count;
                }
            }

            return count;
        }

        private static int ChooseNextContainer(
            LoopSortingGame game,
            Dictionary<int, int> containerToBelt,
            bool[] boxLocked,
            bool[] boxCompleted,
            int[] inboundStreamCounts,
            int beltLength,
            System.Random rng)
        {
            float bestScore = 0f;
            int bestIndex = -1;
            for (int i = 0; i < game.Containers.Count; i++)
            {
                if (i < boxLocked.Length && boxLocked[i]) continue;
                if (i < boxCompleted.Length && boxCompleted[i]) continue;
                if (inboundStreamCounts != null && i < inboundStreamCounts.Length && inboundStreamCounts[i] > 0) continue;
                var container = game.Containers[i];
                if (container.Busy) continue;
                if (container.Count == 0) continue;
                if (!container.TryPeek(out var first)) continue;

                float score = ScoreCandidate(game, containerToBelt, i, first.Color, beltLength);
                if (score > 0f)
                {
                    score += (float)rng.NextDouble() * StrategyRandomJitter;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static float ScoreCandidate(
            LoopSortingGame game,
            Dictionary<int, int> containerToBelt,
            int sourceIndex,
            BlockColor color,
            int beltLength)
        {
            var source = game.Containers[sourceIndex];
            int run = CountFrontRun(source, color);
            float runRatio = run / Mathf.Max(1f, source.Capacity);
            bool isUniform = run >= source.Count;
            bool exposesDifferent = run < source.Count;

            bool hasPreferredTarget = false;
            bool hasEmptyTarget = false;
            float bestTargetFill = 0f;
            int bestDistance = int.MaxValue;

            if (!containerToBelt.TryGetValue(sourceIndex, out var sourcePort))
            {
                return 0f;
            }

            for (int i = 0; i < game.Containers.Count; i++)
            {
                if (i == sourceIndex) continue;
                var target = game.Containers[i];
                if (target.Locked || target.Busy) continue;
                if (target.Count >= target.Capacity) continue;
                if (target.Count == 0)
                {
                    hasEmptyTarget = true;
                    continue;
                }
                if (target.Blocks[0].Color != color) continue;
                hasPreferredTarget = true;
                float fill = target.Count / Mathf.Max(1f, target.Capacity);
                if (fill > bestTargetFill) bestTargetFill = fill;
                if (!containerToBelt.TryGetValue(i, out var targetPort)) continue;
                int dist = ComputeForwardDistance(sourcePort, targetPort, beltLength);
                if (dist < bestDistance) bestDistance = dist;
            }

            if (!hasPreferredTarget && hasEmptyTarget)
            {
                for (int i = 0; i < game.Containers.Count; i++)
                {
                    if (i == sourceIndex) continue;
                    var target = game.Containers[i];
                    if (target.Locked || target.Busy) continue;
                    if (target.Count > 0) continue;
                    if (!containerToBelt.TryGetValue(i, out var targetPort)) continue;
                    int dist = ComputeForwardDistance(sourcePort, targetPort, beltLength);
                    if (dist < bestDistance) bestDistance = dist;
                }
            }

            if (isUniform && !hasPreferredTarget)
            {
                return 0f;
            }

            float proximity = bestDistance == int.MaxValue ? 0f : 1f - (bestDistance / Mathf.Max(1f, beltLength));
            float eScore = 0f;
            if (hasPreferredTarget)
            {
                eScore = 0.6f + 0.4f * proximity;
            }
            else if (hasEmptyTarget)
            {
                eScore = 0.35f + 0.4f * proximity;
            }
            else
            {
                // No external target: allow recycling into the same box (endgame with few colors).
                eScore = 0.15f + 0.25f * runRatio;
            }

            float gScore = 0.6f * runRatio + 0.4f * bestTargetFill;
            float exposeBonus = exposesDifferent ? (0.1f + 0.2f * (1f - runRatio)) : 0f;
            return Mathf.Clamp01(StrategyWeightE * eScore + StrategyWeightG * gScore + exposeBonus);
        }

        private static int ComputeForwardDistance(int source, int target, int beltLength)
        {
            if (beltLength <= 0) return int.MaxValue;
            int diff = target - source;
            if (diff < 0) diff += beltLength;
            return diff;
        }

        private static List<Transform> BuildSlotTransforms(List<Vector3> positions)
        {
            var list = new List<Transform>(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                var go = new GameObject($"SimSlot_{i}");
                go.hideFlags = HideFlags.HideAndDontSave;
                go.transform.position = positions[i];
                list.Add(go.transform);
            }
            return list;
        }
    }
}
