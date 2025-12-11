using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LoopSorting
{
    /// <summary>
    /// Drives gameplay using LevelLayout; keeps visuals aligned with editor preview.
    /// </summary>
    public class GameRuntimeController : MonoBehaviour
    {
        [Header("Config")]
        public float conveyorTickSeconds = 0.35f;
        public float beltSlotSpacing = 0.6f;
        [Tooltip("Camera padding around level bounds. Negative -> use preview-style auto padding.")]
        public float cameraPadding = -1f;
        public float cameraZOffset = -10f;
        [Tooltip("Visual size of each block in box grid.")]
        public Vector2 blockVisualSize = new Vector2(0.45f, 0.45f);
        [Tooltip("Max blocks / slots on the conveyor (default 50). If layout sets beltCapacity > 0, it overrides this.")]
        public int beltBlockLimit = 50;
        [Tooltip("Scale factor for belt block size relative to slot spacing.")]
        public float beltBlockSizeFactor = 0.65f;
        [Header("Animation")]
        [Tooltip("Seconds between consecutive block releases from the same container.")]
        public float releaseInterval = 0.12f;
        [Tooltip("Seconds to wait before retrying if belt slot is blocked.")]
        public float releaseBlockedRetry = 0.1f;
        [Tooltip("Z offset for belt blocks so they render above markers (negative brings closer to camera).")]
        public float beltBlockZOffset = -0.05f;
        [Header("Settings")]
        public bool vibrationEnabled = true;
        public bool soundEnabled = true;
        [Header("UI")]
        public BeltCounterUI beltCounterUI;
        [Header("Debug/Visuals")]
        public bool showSlotGizmos = true;
        public Color slotColor = new Color(1f, 0.8f, 0.2f, 0.9f);
        public float slotGizmoRadius = 0.1f;
        [Tooltip("在游戏中可见的槽位标记")]
        public bool showSlotMarkersRuntime = true;
        public float slotMarkerScale = 0.15f;
        public Color slotMarkerColor = new Color(0.6f, 0.6f, 0.6f, 0.3f);
        [Header("Speed")]
        public float[] speedSteps = new float[] { 1f, 1.5f, 2f };
        [Header("UI Theme")]
        public UITheme uiTheme;

        private LoopSortingGame _game;
        private List<Transform> _beltSlots = new List<Transform>();
        private Dictionary<int, GameObject> _beltBlockVisuals = new Dictionary<int, GameObject>();
        private List<BoxView> _boxViews = new List<BoxView>();
        private Dictionary<int, int> _containerToBelt = new Dictionary<int, int>();
        private List<GameObject> _slotMarkers = new List<GameObject>();
        private List<Vector3> _slotBasePositions = new List<Vector3>();
        private List<Vector3> _slotCurrentPositions = new List<Vector3>();
        private float _tickTimer;
        private Bounds _levelBounds;
        private int _beltCapacity;
        private float _beltSpacingUsed;
        private bool _isReleasing;
        private int? _activeReleasePort;
        private float _speedMultiplier = 1f;
        private int _speedIndex = 0;
        private Button _speedButton;
        private Text _speedButtonLabel;
        private Button _settingsButton;
        private GameObject _settingsPanel;
        private Toggle _vibrationToggle;
        private Toggle _soundToggle;
        private GameObject _boosterPanel;
        private Button _boosterFillButton;
        private Button _boosterShuffleButton;
        private System.Random _rng = new System.Random();
        private GameObject _backgroundQuad;
        private GameObject _eventSystem;
        private Canvas _uiCanvas;
        private LevelFlow _flow;
        private int _flowIndex;
        private LevelLayout _currentLayout;
        private GameObject _resultPanel;
        private Text _resultText;
        private Button _primaryButton;
        private Button _secondaryButton;
        private Text _primaryLabel;
        private Text _secondaryLabel;
        private bool _gameOver;

        private void ClearRuntime()
        {
            // Stop coroutines
            StopAllCoroutines();

            // Destroy child objects under controller (conveyors, containers, slot markers, background, UI spawned under this transform)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            // Clear collections/state
            _beltSlots.Clear();
            _slotMarkers.Clear();
            _slotBasePositions.Clear();
            _slotCurrentPositions.Clear();
            foreach (var kv in _beltBlockVisuals)
            {
                if (kv.Value != null) DestroyImmediate(kv.Value);
            }
            _beltBlockVisuals.Clear();
            _boxViews.Clear();
            _containerToBelt.Clear();
            _game = null;
            _isReleasing = false;
            _activeReleasePort = null;
            _tickTimer = 0f;
            _beltSpacingUsed = 0f;
            _gameOver = false;
        }

        public void Build(LevelLayout layout)
        {
            BuildInternal(layout, clearFlow: true);
        }

        private void BuildInternal(LevelLayout layout, bool clearFlow)
        {
            if (clearFlow)
            {
                _flow = null;
                _flowIndex = 0;
            }
            _currentLayout = layout;
            if (layout == null)
            {
                Debug.LogError("GameRuntimeController.Build: layout is null");
                return;
            }

            // Reset root transform to avoid inherited offsets/scale from scene.
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            // Full cleanup before building next level.
            ClearRuntime();

            _beltCapacity = layout.beltCapacity > 0 ? layout.beltCapacity : beltBlockLimit;
            EnsureEventSystem();

            BuildConveyor(layout);
            BuildContainers(layout);
            _currentLayout = layout;
            _levelBounds = LayoutUtils.ComputeLayoutBounds(layout);
            FitCameraToLevel(layout);
            EnsureBackground();
            EnsureCounterUI();
            EnsureSettingsUI();
            SyncContainersVisuals();
            SyncBeltVisuals();
        }

        public void Build(LevelFlow flow, int startIndex = 0)
        {
            _flow = flow;
            _flowIndex = Mathf.Clamp(startIndex, 0, flow != null ? Mathf.Max(0, flow.levels.Count - 1) : 0);
            var layout = flow != null && flow.levels.Count > 0 ? flow.levels[_flowIndex] : null;
            BuildInternal(layout, clearFlow: false);
        }

        private void CycleSpeed()
        {
            if (speedSteps == null || speedSteps.Length == 0)
            {
                _speedMultiplier = 1f;
                UpdateSpeedButtonLabel();
                return;
            }
            _speedIndex = (_speedIndex + 1) % speedSteps.Length;
            _speedMultiplier = Mathf.Max(0.0001f, speedSteps[_speedIndex]);
            UpdateSpeedButtonLabel();
        }

        private void UpdateSpeedButtonLabel()
        {
            if (_speedButtonLabel == null) return;
            float val = _speedMultiplier;
            _speedButtonLabel.text = $"{val:0.##}x";
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }

            _eventSystem = new GameObject("EventSystem");
            _eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            _eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(_eventSystem);
        }

        private void EnsureBackground()
        {
            // Rebuild each time to match camera framing and avoid偏移/残留。
            if (_backgroundQuad != null)
            {
                DestroyImmediate(_backgroundQuad);
                _backgroundQuad = null;
            }

            var cam = Camera.main;
            if (cam == null) return;

            _backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _backgroundQuad.name = "BackgroundQuad";
            _backgroundQuad.layer = cam.gameObject.layer;
            _backgroundQuad.transform.SetParent(cam.transform, false);

            // 锚定相机，推到视锥远端，确保始终在玩法后方且不偏移。
            float dist = Mathf.Max(5f, cam.farClipPlane * 0.5f);
            _backgroundQuad.transform.localPosition = Vector3.forward * dist;
            _backgroundQuad.transform.localRotation = Quaternion.identity;

            // 按相机视口尺寸加 padding 缩放。
            float viewHeight = cam.orthographic ? cam.orthographicSize * 2f : 30f;
            float aspect = Mathf.Max(0.01f, cam.aspect);
            float padding = 1.2f;
            _backgroundQuad.transform.localScale = new Vector3(viewHeight * aspect * padding, viewHeight * padding, 1f);

            // 渐变材质，关闭深度写入/测试，只做背景。
            var tex = new Texture2D(1, 2);
            tex.wrapMode = TextureWrapMode.Clamp;
            Color top = new Color(1f, 0.92f, 0.78f);   // soft warm
            Color bottom = new Color(1f, 0.87f, 0.65f); // slight red-ish yellow
            tex.SetPixels(new[] { bottom, top });
            tex.Apply();

            var mat = new Material(Shader.Find("Unlit/Texture"));
            mat.mainTexture = tex;
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Background;
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            var renderer = _backgroundQuad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            // Disable collider
            var col = _backgroundQuad.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private int? TryGetBlockedPort()
        {
            if (!_isReleasing || _game == null || !_activeReleasePort.HasValue)
            {
                return null;
            }

            int portIdx = _activeReleasePort.Value;
            var slot = _game.Conveyor.GetSlot(portIdx);
            if (slot.HasValue)
            {
                return portIdx;
            }

            return null;
        }

        private void ToggleSettingsPanel(bool show)
        {
            if (_settingsPanel == null) return;
            _settingsPanel.SetActive(show);
            if (show)
            {
                if (_vibrationToggle != null) _vibrationToggle.isOn = vibrationEnabled;
                if (_soundToggle != null) _soundToggle.isOn = soundEnabled;
            }
        }

        private void UseBoosterFillColor()
        {
            if (_game == null) return;

            // find completed colors
            var completedColors = new HashSet<BlockColor>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                var c = _game.Containers[i];
                if (c.IsUniformAndFull())
                {
                    if (c.Blocks.Count > 0) completedColors.Add(c.Blocks[0].Color);
                }
            }

            // count colors available and unfinished colors
            var colorCounts = new Dictionary<BlockColor, int>();
            foreach (var cont in _game.Containers)
            {
                foreach (var b in cont.Blocks)
                {
                    if (!colorCounts.ContainsKey(b.Color)) colorCounts[b.Color] = 0;
                    colorCounts[b.Color]++;
                }
            }
            foreach (var slot in _game.Conveyor.Slots)
            {
                if (slot.HasValue)
                {
                    if (!colorCounts.ContainsKey(slot.Value.Color)) colorCounts[slot.Value.Color] = 0;
                    colorCounts[slot.Value.Color]++;
                }
            }

            var candidates = new List<BlockColor>();
            foreach (var kv in colorCounts)
            {
                if (!completedColors.Contains(kv.Key))
                {
                    candidates.Add(kv.Key);
                }
            }
            if (candidates.Count == 0) return;

            var targetColor = candidates[_rng.Next(candidates.Count)];

            // pick target container with most targetColor and not already full uniform of another color
            int targetIdx = -1;
            int bestCount = -1;
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                var c = _game.Containers[i];
                if (c.IsUniformAndFull()) continue;
                int count = 0;
                foreach (var b in c.Blocks) if (b.Color == targetColor) count++;
                if (count > bestCount)
                {
                    bestCount = count;
                    targetIdx = i;
                }
            }
            if (targetIdx < 0) targetIdx = 0;

            // collect target color blocks from all containers and conveyor
            var sourceBlocks = new List<Block>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                var rem = _game.Containers[i].RemoveBlocksWhere(b => b.Color == targetColor);
                sourceBlocks.AddRange(rem);
            }
            var beltRem = _game.Conveyor.RemoveBlocksWhere(b => b.Color == targetColor);
            sourceBlocks.AddRange(beltRem);

            // collect displaced non-target from target container so它们不会消失
            var displaced = _game.Containers[targetIdx].RemoveBlocksWhere(b => b.Color != targetColor);

            // fill target container with targetColor up to capacity
            int cap = _game.Containers[targetIdx].Capacity;
            var fillList = new List<Block>();
            for (int i = 0; i < cap; i++)
            {
                if (sourceBlocks.Count > 0)
                {
                    fillList.Add(sourceBlocks[0]);
                    sourceBlocks.RemoveAt(0);
                }
                else
                {
                    fillList.Add(new Block(targetColor));
                }
            }
            _game.Containers[targetIdx].ClearAndAdd(fillList);

            // put displaced + leftover target blocks back into containers (fill other unfinished containers)
            var leftovers = new List<Block>();
            leftovers.AddRange(displaced);
            leftovers.AddRange(sourceBlocks);
            if (leftovers.Count > 0)
            {
                for (int i = 0; i < _game.Containers.Count && leftovers.Count > 0; i++)
                {
                    if (i == targetIdx) continue;
                    var cont = _game.Containers[i];
                    int room = cont.Capacity - cont.Count;
                    int take = Math.Min(room, leftovers.Count);
                    if (take > 0)
                    {
                        var extra = leftovers.GetRange(0, take);
                        cont.AddBlocks(extra);
                        leftovers.RemoveRange(0, take);
                    }
                }
            }

            SyncContainersVisuals();
            SyncBeltVisuals();
            CheckEndConditions();
        }

        private List<(BlockColor color, int count)> BuildColorRuns(IReadOnlyList<Block> blocks)
        {
            var runs = new List<(BlockColor color, int count)>();
            if (blocks == null || blocks.Count == 0) return runs;
            var currentColor = blocks[0].Color;
            int run = 1;
            for (int i = 1; i < blocks.Count; i++)
            {
                if (blocks[i].Color == currentColor)
                {
                    run++;
                }
                else
                {
                    runs.Add((currentColor, run));
                    currentColor = blocks[i].Color;
                    run = 1;
                }
            }
            runs.Add((currentColor, run));
            return runs;
        }

        private void UseBoosterShuffle()
        {
            if (_game == null) return;

            var completedColors = new HashSet<BlockColor>();
            var completedContainers = new HashSet<int>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                var c = _game.Containers[i];
                if (c.IsUniformAndFull())
                {
                    completedContainers.Add(i);
                    if (c.Blocks.Count > 0) completedColors.Add(c.Blocks[0].Color);
                }
            }

            // gather chunks (consecutive runs) from unfinished containers
            var chunks = new List<(BlockColor color, int count)>();
            var targetContainers = new List<int>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (completedContainers.Contains(i)) continue;
                targetContainers.Add(i);
                var runs = BuildColorRuns(_game.Containers[i].Blocks);
                chunks.AddRange(runs);
                _game.Containers[i].ClearAndAdd(Array.Empty<Block>());
            }

            // conveyor: remove non-completed colors and add as single-block chunks
            var beltPool = _game.Conveyor.RemoveBlocksWhere(b => !completedColors.Contains(b.Color));
            foreach (var b in beltPool) chunks.Add((b.Color, 1));

            if (chunks.Count == 0) return;

            // shuffle chunks
            for (int i = chunks.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (chunks[i], chunks[j]) = (chunks[j], chunks[i]);
            }

            var queue = new Queue<(BlockColor color, int count)>(chunks);
            // distribute chunks into containers, splitting if overflow (remaining re-enqueued)
            foreach (var idx in targetContainers)
            {
                var cont = _game.Containers[idx];
                int space = cont.Capacity;
                var newBlocks = new List<Block>();
                while (space > 0 && queue.Count > 0)
                {
                    var chunk = queue.Dequeue();
                    int take = Math.Min(space, chunk.count);
                    for (int t = 0; t < take; t++)
                    {
                        newBlocks.Add(new Block(chunk.color));
                    }
                    space -= take;
                    int leftover = chunk.count - take;
                    if (leftover > 0)
                    {
                        // put remainder back to queue
                        queue.Enqueue((chunk.color, leftover));
                    }
                }
                cont.ClearAndAdd(newBlocks);
            }

            // leftover chunks to belt
            var remaining = new List<Block>();
            while (queue.Count > 0)
            {
                var ch = queue.Dequeue();
                for (int i = 0; i < ch.count; i++) remaining.Add(new Block(ch.color));
            }
            FillConveyorFreeSlots(remaining);

            SyncContainersVisuals();
            SyncBeltVisuals();
            CheckEndConditions();
        }

        private void FillConveyorFreeSlots(List<Block> blocks)
        {
            if (blocks == null || blocks.Count == 0) return;
            for (int i = 0; i < _game.Conveyor.Length && blocks.Count > 0; i++)
            {
                if (!_game.Conveyor.GetSlot(i).HasValue)
                {
                    _game.Conveyor.TryPlaceAt(i, blocks[0]);
                    blocks.RemoveAt(0);
                }
            }
        }
        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            _tickTimer += Time.deltaTime * _speedMultiplier;
            float progress = Mathf.Clamp01(_tickTimer / Mathf.Max(0.0001f, conveyorTickSeconds));
            UpdateSlotMarkersVisuals(progress);
            UpdateBeltBlockVisuals(progress);

            if (_tickTimer >= conveyorTickSeconds)
            {
                _tickTimer = 0f;
                int? blocked = _isReleasing && TryGetBlockedPort() is int idx ? idx : (int?)null;
                _game.TickConveyor(blocked);
                SyncBeltVisuals();
                SyncContainersVisuals();
                UpdateBeltCounter();
                CheckEndConditions();
            }
        }

        public void HandleContainerClick(int containerIndex)
        {
            if (_game == null || _isReleasing)
            {
                return;
            }

            if (containerIndex < 0 || containerIndex >= _game.Containers.Count)
            {
                return;
            }

            var container = _game.Containers[containerIndex];
            if (!container.TryPeek(out var first))
            {
                return;
            }

            StartCoroutine(ReleaseRoutine(containerIndex, first.Color));
        }

        private void BuildConveyor(LevelLayout layout)
        {
            if (layout.conveyors == null || layout.conveyors.Count == 0)
            {
                Debug.LogError("No conveyor path defined in layout.");
                return;
            }

            // Pick the first conveyor that has at least 2 points; ignore empty placeholders.
            ConveyorPath path = null;
            for (int i = 0; i < layout.conveyors.Count; i++)
            {
                var candidate = layout.conveyors[i];
                if (candidate != null && candidate.points != null && candidate.points.Count >= 2)
                {
                    path = candidate;
                    break;
                }
            }
            if (path == null)
            {
                Debug.LogError("No valid conveyor path (needs at least 2 points).");
                return;
            }

            float spacing = layout.beltSlotSpacing > 0f ? layout.beltSlotSpacing : beltSlotSpacing;
            _beltSlots = LayoutUtils.BuildSlotsFromPath(
                path,
                spacing,
                _beltCapacity,
                out _beltSpacingUsed,
                smoothCorners: layout.smoothCorners,
                smoothTension: layout.cornerSmoothTension,
                smoothSubdivisions: layout.cornerSubdivisions);

            // Safety: ensure we always have at least one slot to avoid zero-length conveyor crashes.
            if (_beltSlots == null || _beltSlots.Count == 0)
            {
                _beltSlots = new List<Transform>();
                var t = new GameObject("Slot_0").transform;
                t.position = path.points != null && path.points.Count > 0
                    ? new Vector3(path.points[0].x, path.points[0].y, 0f)
                    : Vector3.zero;
                _beltSlots.Add(t);
                _beltSpacingUsed = spacing;
                Debug.LogWarning("BuildConveyor: slot generation failed, created a single fallback slot to keep the game running.");
            }

            BuildSlotMarkers();
            EnsureBackground();

            var trackParent = new GameObject("ConveyorSlots");
            trackParent.transform.SetParent(transform, false);
            foreach (var t in _beltSlots)
            {
                t.SetParent(trackParent.transform, true);
            }
        }

        private void BuildContainers(LevelLayout layout)
        {
            var containers = new List<Container>();
            var containerToBelt = new Dictionary<int, int>();

            var parent = new GameObject("Containers");
            parent.transform.SetParent(transform, false);

            float unit = layout.blockSize > 0 ? layout.blockSize : blockVisualSize.x;
            blockVisualSize = new Vector2(unit, unit);

            for (int i = 0; i < layout.boxes.Count; i++)
            {
                var spec = layout.boxes[i];

                // derive size from rows/cols and block size
                spec.size = LayoutUtils.ComputeBoxSize(spec, unit);

                var go = new GameObject(string.IsNullOrEmpty(spec.name) ? $"Box_{i}" : spec.name);
                go.transform.SetParent(parent.transform, false);
                go.transform.position = new Vector3(spec.position.x, spec.position.y, 0f);
                go.transform.localScale = Vector3.one;

                var boxView = go.AddComponent<BoxView>();
                int columns = Mathf.Max(1, spec.columns);
                int rows = Mathf.Max(1, spec.rows);
                boxView.Init(i, this, spec.size, columns, rows, blockVisualSize, spec.opening);
                _boxViews.Add(boxView);

                int capacity = Mathf.Max(1, columns * rows);
                var boxBlocks = BuildBlocksForSpec(spec, capacity);
                var container = new Container(capacity, boxBlocks);
                containers.Add(container);

                int slotIndex = spec.autoAlignSlot
                    ? LayoutUtils.ResolveBeltSlotIndex(spec, _beltSlots, unit)
                    : Mathf.Clamp(spec.beltSlotIndex, 0, Mathf.Max(0, _beltSlots.Count - 1));
                _containerToBelt[i] = slotIndex;
                containerToBelt[i] = slotIndex;
            }

            _game = new LoopSortingGame(_beltSlots.Count, containers, containerToBelt, _beltCapacity);
            UpdateBeltCounter();
        }

        private void SyncContainersVisuals()
        {
            if (_game == null)
            {
                return;
            }

            for (int i = 0; i < _game.Containers.Count && i < _boxViews.Count; i++)
            {
                _boxViews[i].SyncBlocks(_game.Containers[i].Blocks);
            }
        }

        private void SyncBeltVisuals()
        {
            if (_game == null)
            {
                return;
            }

            // Remove visuals no longer needed.
            var toRemove = new List<int>();
            foreach (var kv in _beltBlockVisuals)
            {
                int idx = kv.Key;
                var slot = _game.Conveyor.GetSlot(idx);
                if (!slot.HasValue)
                {
                    Destroy(kv.Value);
                    toRemove.Add(idx);
                }
            }
            foreach (var idx in toRemove)
            {
                _beltBlockVisuals.Remove(idx);
            }

            // Add/update visuals.
            for (int i = 0; i < _beltSlots.Count; i++)
            {
                var slot = _game.Conveyor.GetSlot(i);
                if (!slot.HasValue)
                {
                    continue;
                }

                EnsureBlockVisual(i, slot.Value);
            }
        }

        private void OnDrawGizmos()
        {
            if (!showSlotGizmos || _beltSlots == null) return;
            Gizmos.color = slotColor;
            foreach (var t in _beltSlots)
            {
                if (t == null) continue;
                Gizmos.DrawSphere(t.position, slotGizmoRadius);
            }
        }

        private void UpdateBeltCounter()
        {
            if (beltCounterUI == null || _game == null)
            {
                return;
            }

            int total = _game.Conveyor.Length;
            int occupied = _game.Conveyor.BlockCount;
            int empty = Mathf.Max(0, total - occupied);
            beltCounterUI.SetValue(empty, total);
        }

        private void CheckEndConditions()
        {
            if (_gameOver || _game == null) return;

            bool win = _game.IsSolved(true);
            if (win)
            {
                ShowResult(true);
                return;
            }

            bool beltFull = _game.Conveyor.BlockCount >= _game.Conveyor.Length;
            if (beltFull && !CanAnyContainerAcceptAnyBeltBlock())
            {
                ShowResult(false);
            }
        }

        private bool CanAnyContainerAcceptAnyBeltBlock()
        {
            for (int i = 0; i < _game.Conveyor.Length; i++)
            {
                var slot = _game.Conveyor.GetSlot(i);
                if (!slot.HasValue) continue;
                var block = slot.Value;
                for (int c = 0; c < _game.Containers.Count; c++)
                {
                    if (_game.Containers[c].CanAccept(block))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ShowResult(bool win)
        {
            _gameOver = true;
            EnsureResultPanel();
            _resultPanel.SetActive(true);
            _resultText.text = win ? "Level Complete" : "Level Failed";
            _primaryLabel.text = win ? "Next" : "Retry";
            _secondaryLabel.text = win ? "Retry" : "Close";
        }

        private void OnPrimaryClicked()
        {
            if (_resultPanel != null) _resultPanel.SetActive(false);
            if (_flow != null && _flow.levels.Count > 0 && _primaryLabel != null && _primaryLabel.text == "Next")
            {
                int next = _flowIndex + 1;
                if (next < _flow.levels.Count)
                {
                    _flowIndex = next;
                    _gameOver = false;
                    Build(_flow, _flowIndex);
                    return;
                }
            }
            RestartCurrent();
        }

        private void OnSecondaryClicked()
        {
            if (_resultPanel != null) _resultPanel.SetActive(false);
            RestartCurrent();
        }

        private void RestartCurrent()
        {
            _gameOver = false;
            if (_flow != null && _flow.levels.Count > 0)
            {
                Build(_flow, _flowIndex);
            }
            else
            {
                Build(_currentLayout);
            }
        }

        private void FitCameraToLevel(LevelLayout layout)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("GameRuntimeController: no main camera found to frame level.");
                return;
            }

            var bounds = LayoutUtils.ComputeLayoutBounds(layout);
            if (bounds.size == Vector3.zero)
            {
                return;
            }

            float paddingToUse = cameraPadding;
            if (paddingToUse < 0f)
            {
                var size = bounds.size;
                paddingToUse = Mathf.Max(size.x, size.y) * 0.1f + 0.5f;
            }
            bounds.Expand(paddingToUse * 2f);

            cam.orthographic = true;
            float width = bounds.size.x;
            float height = bounds.size.y;
            float orthoSize = Mathf.Max(height * 0.5f, width * 0.5f / Mathf.Max(0.0001f, cam.aspect));
            cam.orthographicSize = orthoSize;
            cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, cameraZOffset);
        }

        private static IEnumerable<Block> BuildBlocksForSpec(BoxSpec spec, int capacity)
        {
            if (spec.colorCounts != null)
            {
                int filled = 0;
                foreach (var cc in spec.colorCounts)
                {
                    int cnt = Mathf.Max(0, cc.count);
                    for (int i = 0; i < cnt && filled < capacity; i++)
                    {
                        yield return new Block(cc.color);
                        filled++;
                    }

                    if (filled >= capacity)
                    {
                        yield break;
                    }
                }
            }
        }

        private IEnumerator ReleaseRoutine(int containerIndex, BlockColor targetColor)
        {
            _isReleasing = true;
            _activeReleasePort = _containerToBelt.TryGetValue(containerIndex, out var portIdx) ? portIdx : (int?)null;

            var container = _game.Containers[containerIndex];
            // Determine how many consecutive blocks of the same color are at the front.
            int pending = 0;
            for (int i = 0; i < container.Count; i++)
            {
                if (container.Blocks[i].Color == targetColor)
                {
                    pending++;
                }
                else
                {
                    break;
                }
            }

            int safety = 0;
            int maxOps = Mathf.Max(128, pending * 4); // generous safety to avoid early stop when belt temporarily blocked

            while (pending > 0 && safety < maxOps)
            {
                // Check front color still matches.
                if (!container.TryPeek(out var peek) || peek.Color != targetColor)
                {
                    break;
                }

                var result = _game.TryReleaseFromContainer(containerIndex);
                if (result == ReleaseResult.BeltBlocked)
                {
                    // Slot is occupied, wait and retry next frame/interval. Belt moves independently.
                    yield return new WaitForSeconds(releaseBlockedRetry / Mathf.Max(0.0001f, _speedMultiplier));
                    safety++;
                    continue;
                }
                if (result != ReleaseResult.Success)
                {
                    break;
                }

                pending--;

                SyncBeltVisuals();
                SyncContainersVisuals();

                yield return new WaitForSeconds(releaseInterval / Mathf.Max(0.0001f, _speedMultiplier));
                safety++;
            }

            _isReleasing = false;
            _activeReleasePort = null;
            CheckEndConditions();
        }

        private void EnsureCounterUI()
        {
            if (_uiCanvas != null && beltCounterUI != null && _speedButton != null && _resultPanel != null && _settingsButton != null && _boosterPanel != null)
            {
                return;
            }

            var canvasGO = new GameObject("HUDCanvas");
            _uiCanvas = canvasGO.AddComponent<Canvas>();
            _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _uiCanvas.overrideSorting = true;
            _uiCanvas.sortingOrder = 0;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO);

            var textGO = new GameObject("BeltCounter");
            textGO.transform.SetParent(canvasGO.transform, false);
            var text = textGO.AddComponent<Text>();
            text.alignment = TextAnchor.UpperLeft;
            text.font = uiTheme != null && uiTheme.font != null
                ? uiTheme.font
                : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = uiTheme != null ? uiTheme.counterFontSize : 24;
            text.color = uiTheme != null ? uiTheme.counterColor : Color.white;
            text.text = "Belt: -/-";
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(10f, -10f);

            beltCounterUI = textGO.AddComponent<BeltCounterUI>();
            var buttonGO = new GameObject("SpeedButton");
            buttonGO.transform.SetParent(canvasGO.transform, false);
            _speedButton = buttonGO.AddComponent<Button>();
            var img = buttonGO.AddComponent<Image>();
            img.color = uiTheme != null ? uiTheme.buttonColor : new Color(0.2f, 0.2f, 0.2f, 0.85f);
            if (uiTheme != null && uiTheme.buttonSprite != null) img.sprite = uiTheme.buttonSprite;
            var btnRect = buttonGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f, 1f);
            btnRect.anchorMax = new Vector2(1f, 1f);
            btnRect.pivot = new Vector2(1f, 1f);
            btnRect.sizeDelta = new Vector2(80f, 36f);
            btnRect.anchoredPosition = new Vector2(-10f, -10f);

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(buttonGO.transform, false);
            _speedButtonLabel = labelGO.AddComponent<Text>();
            _speedButtonLabel.font = uiTheme != null && uiTheme.font != null
                ? uiTheme.font
                : Resources.GetBuiltinResource<Font>("Arial.ttf");
            _speedButtonLabel.alignment = TextAnchor.MiddleCenter;
            _speedButtonLabel.color = uiTheme != null ? uiTheme.buttonTextColor : Color.white;
            var labelRect = _speedButtonLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            _speedButton.onClick.AddListener(CycleSpeed);
            UpdateSpeedButtonLabel();

            // Settings button
            var settingsGO = new GameObject("SettingsButton");
            settingsGO.transform.SetParent(canvasGO.transform, false);
            _settingsButton = settingsGO.AddComponent<Button>();
            var sImg = settingsGO.AddComponent<Image>();
            sImg.color = uiTheme != null ? uiTheme.buttonColor : new Color(0.2f, 0.2f, 0.2f, 0.85f);
            if (uiTheme != null && uiTheme.buttonSprite != null) sImg.sprite = uiTheme.buttonSprite;
            var sRect = settingsGO.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(1f, 1f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.pivot = new Vector2(1f, 1f);
            sRect.sizeDelta = new Vector2(36f, 36f);
            sRect.anchoredPosition = new Vector2(-100f, -10f);
            _settingsButton.onClick.AddListener(() => ToggleSettingsPanel(true));

            // Booster panel (bottom center)
            _boosterPanel = new GameObject("BoosterPanel");
            _boosterPanel.transform.SetParent(canvasGO.transform, false);
            var bRect = _boosterPanel.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0f);
            bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.sizeDelta = new Vector2(320f, 80f);
            bRect.anchoredPosition = new Vector2(0f, 12f);

            _boosterFillButton = CreateButton(_boosterPanel.transform, "BoosterFill", new Vector2(0.25f, 0.5f));
            _boosterFillButton.GetComponent<RectTransform>().sizeDelta = new Vector2(140f, 46f);
            _boosterFillButton.GetComponentInChildren<Text>().text = "完成颜色";
            _boosterFillButton.onClick.AddListener(UseBoosterFillColor);

            _boosterShuffleButton = CreateButton(_boosterPanel.transform, "BoosterShuffle", new Vector2(0.75f, 0.5f));
            _boosterShuffleButton.GetComponent<RectTransform>().sizeDelta = new Vector2(140f, 46f);
            _boosterShuffleButton.GetComponentInChildren<Text>().text = "打乱顺序";
            _boosterShuffleButton.onClick.AddListener(UseBoosterShuffle);

            EnsureResultPanel();
        }

        private void EnsureResultPanel()
        {
            if (_resultPanel != null) return;
            var panelGO = new GameObject("ResultPanel");
            panelGO.transform.SetParent(_uiCanvas.transform, false);
            _resultPanel = panelGO;
            var image = panelGO.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.6f);
            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var boxGO = new GameObject("Box");
            boxGO.transform.SetParent(panelGO.transform, false);
            var boxRect = boxGO.AddComponent<RectTransform>();
            boxRect.sizeDelta = new Vector2(320f, 180f);
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            var boxImg = boxGO.AddComponent<Image>();
            boxImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(boxGO.transform, false);
            _resultText = titleGO.AddComponent<Text>();
            _resultText.font = uiTheme != null && uiTheme.font != null
                ? uiTheme.font
                : Resources.GetBuiltinResource<Font>("Arial.ttf");
            _resultText.alignment = TextAnchor.MiddleCenter;
            _resultText.fontSize = 24;
            _resultText.color = Color.white;
            var titleRect = _resultText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.55f);
            titleRect.anchorMax = new Vector2(1f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Buttons
            _primaryButton = CreateButton(boxGO.transform, "PrimaryButton", new Vector2(0.25f, 0.15f));
            _secondaryButton = CreateButton(boxGO.transform, "SecondaryButton", new Vector2(0.75f, 0.15f));
            _primaryLabel = _primaryButton.GetComponentInChildren<Text>();
            _secondaryLabel = _secondaryButton.GetComponentInChildren<Text>();
            _primaryButton.onClick.AddListener(OnPrimaryClicked);
            _secondaryButton.onClick.AddListener(OnSecondaryClicked);

            _resultPanel.SetActive(false);
        }

        private void EnsureSettingsUI()
        {
            if (_uiCanvas == null) return;
            if (_settingsPanel != null && _vibrationToggle != null && _soundToggle != null) return;

            // panel
            _settingsPanel = new GameObject("SettingsPanel");
            _settingsPanel.transform.SetParent(_uiCanvas.transform, false);
            var overlayImg = _settingsPanel.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.4f);
            var overlayRect = _settingsPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var boxGO = new GameObject("Box");
            boxGO.transform.SetParent(_settingsPanel.transform, false);
            var boxRect = boxGO.AddComponent<RectTransform>();
            boxRect.sizeDelta = new Vector2(260f, 180f);
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            var boxImg = boxGO.AddComponent<Image>();
            boxImg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(boxGO.transform, false);
            var titleText = titleGO.AddComponent<Text>();
            titleText.font = uiTheme != null && uiTheme.font != null
                ? uiTheme.font
                : Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 20;
            titleText.color = Color.white;
            titleText.text = "设置";
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.75f);
            titleRect.anchorMax = new Vector2(1f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            _vibrationToggle = CreateToggle(boxGO.transform, "振动", vibrationEnabled, new Vector2(0.2f, 0.55f));
            _vibrationToggle.onValueChanged.AddListener(val => vibrationEnabled = val);

            _soundToggle = CreateToggle(boxGO.transform, "声音", soundEnabled, new Vector2(0.2f, 0.35f));
            _soundToggle.onValueChanged.AddListener(val => soundEnabled = val);

            var closeBtn = CreateButton(boxGO.transform, "CloseSettings", new Vector2(0.5f, 0.1f));
            closeBtn.onClick.AddListener(() => ToggleSettingsPanel(false));
            closeBtn.GetComponentInChildren<Text>().text = "关闭";

            _settingsPanel.SetActive(false);
        }

        private Button CreateButton(Transform parent, string name, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(120f, 40f);
            rect.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = uiTheme != null ? uiTheme.buttonColor : new Color(0.2f, 0.2f, 0.2f, 0.85f);
            if (uiTheme != null && uiTheme.buttonSprite != null) img.sprite = uiTheme.buttonSprite;
            var btn = go.AddComponent<Button>();
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<Text>();
            text.font = uiTheme != null && uiTheme.font != null
                ? uiTheme.font
                : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = uiTheme != null ? uiTheme.buttonTextColor : Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var tRect = text.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            return btn;
        }

        private Toggle CreateToggle(Transform parent, string label, bool initial, Vector2 anchor)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(180f, 30f);
            rect.anchoredPosition = Vector2.zero;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);

            var toggle = go.AddComponent<Toggle>();
            var check = new GameObject("Check").AddComponent<Image>();
            check.transform.SetParent(go.transform, false);
            var cRect = check.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0f, 0.5f);
            cRect.anchorMax = new Vector2(0f, 0.5f);
            cRect.pivot = new Vector2(0f, 0.5f);
            cRect.sizeDelta = new Vector2(20f, 20f);
            cRect.anchoredPosition = new Vector2(10f, 0f);
            check.color = new Color(0.4f, 0.8f, 0.4f, 0.9f);
            toggle.graphic = check;
            toggle.isOn = initial;

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(go.transform, false);
            var text = lblGO.AddComponent<Text>();
            text.font = uiTheme != null && uiTheme.font != null
                ? uiTheme.font
                : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = label;
            var tRect = text.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.offsetMin = new Vector2(40f, 0f);
            tRect.offsetMax = new Vector2(0f, 0f);

            return toggle;
        }

        private void BuildSlotMarkers()
        {
            if (!showSlotMarkersRuntime)
            {
                return;
            }

            // Clean previous markers
            foreach (var m in _slotMarkers)
            {
                if (m != null) Destroy(m);
            }
            _slotMarkers.Clear();
            _slotBasePositions.Clear();

            var parent = new GameObject("SlotMarkers");
            parent.transform.SetParent(transform, false);

            foreach (var t in _beltSlots)
            {
                _slotBasePositions.Add(t.position);
                _slotCurrentPositions.Add(t.position);
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "SlotMarker";
                marker.transform.SetParent(parent.transform, false);
                marker.transform.position = t.position;
                marker.transform.localScale = Vector3.one * slotMarkerScale;
                var renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Unlit/Color"));
                    mat.color = slotMarkerColor;
                    renderer.sharedMaterial = mat;
                }
                _slotMarkers.Add(marker);
            }
        }

        private void UpdateSlotMarkersVisuals(float progress)
        {
            if (!showSlotMarkersRuntime || _slotMarkers.Count == 0 || _slotBasePositions.Count == 0)
            {
                return;
            }

            int count = _slotBasePositions.Count;

            for (int i = 0; i < _slotMarkers.Count && i < count; i++)
            {
                var marker = _slotMarkers[i];
                if (marker == null) continue;

                var from = _slotBasePositions[i];
                // 不对最后一个做环形插值，避免入口出口之间的飞跃
                if (i == count - 1)
                {
                    marker.transform.position = from;
                    SetSlotCurrent(i, from);
                }
                else
                {
                    var to = _slotBasePositions[i + 1];
                    var pos = Vector3.Lerp(from, to, progress);
                    marker.transform.position = pos;
                    SetSlotCurrent(i, pos);
                }
            }
            // last slot current position fallback
            if (count > 0 && _slotCurrentPositions.Count >= count)
            {
                _slotCurrentPositions[count - 1] = _slotBasePositions[count - 1];
            }
        }

        private void UpdateBeltBlockVisuals(float progress)
        {
            if (_beltBlockVisuals.Count == 0 || _slotCurrentPositions.Count == 0)
            {
                return;
            }

            foreach (var kv in _beltBlockVisuals)
            {
                int idx = kv.Key;
                var go = kv.Value;
                if (go == null) continue;
                if (idx < 0 || idx >= _slotCurrentPositions.Count) continue;

                var pos = _slotCurrentPositions[idx] + new Vector3(0f, 0f, beltBlockZOffset);
                go.transform.position = pos;
            }
        }

        private void EnsureBlockVisual(int index, Block block)
        {
            if (!_beltBlockVisuals.TryGetValue(index, out var go) || go == null)
            {
                go = BlockVisual.CreateBlock(block.Color);
                _beltBlockVisuals[index] = go;
                go.transform.SetParent(transform, true);
            }

            float spacing = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;
            float baseSize = Mathf.Max(0.05f, spacing * beltBlockSizeFactor);
            go.transform.localScale = new Vector3(baseSize, baseSize, baseSize * 0.6f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = BlockVisual.ToUnityColor(block.Color);
            }
        }

        private void SetSlotCurrent(int index, Vector3 pos)
        {
            while (_slotCurrentPositions.Count <= index)
            {
                _slotCurrentPositions.Add(Vector3.zero);
            }
            _slotCurrentPositions[index] = pos;
        }
    }
}
