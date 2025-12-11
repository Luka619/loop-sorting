using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        [Tooltip("Max blocks allowed on the conveyor (0 = no extra limit). If layout sets beltCapacity > 0, it overrides this.")]
        public int beltBlockLimit = 0;
        [Tooltip("Scale factor for belt block size relative to slot spacing.")]
        public float beltBlockSizeFactor = 0.65f;
        [Header("Animation")]
        [Tooltip("Seconds between consecutive block releases from the same container.")]
        public float releaseInterval = 0.12f;
        [Tooltip("Seconds to wait before retrying if belt slot is blocked.")]
        public float releaseBlockedRetry = 0.1f;
        [Tooltip("Z offset for belt blocks so they render above markers (negative brings closer to camera).")]
        public float beltBlockZOffset = -0.05f;
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

        public void Build(LevelLayout layout)
        {
            if (layout == null)
            {
                Debug.LogError("GameRuntimeController.Build: layout is null");
                return;
            }

            _beltCapacity = layout.beltCapacity > 0 ? layout.beltCapacity : beltBlockLimit;
            EnsureCounterUI();

            BuildConveyor(layout);
            BuildContainers(layout);
            _levelBounds = LayoutUtils.ComputeLayoutBounds(layout);
            FitCameraToLevel(layout);
            SyncContainersVisuals();
            SyncBeltVisuals();
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
        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            _tickTimer += Time.deltaTime;
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

            var path = layout.conveyors[0];
            float spacing = layout.beltSlotSpacing > 0f ? layout.beltSlotSpacing : beltSlotSpacing;
            _beltSlots = LayoutUtils.BuildSlotsFromPath(
                path,
                spacing,
                _beltCapacity,
                out _beltSpacingUsed,
                smoothCorners: layout.smoothCorners,
                smoothTension: layout.cornerSmoothTension,
                smoothSubdivisions: layout.cornerSubdivisions);

            BuildSlotMarkers();

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

            for (int i = 0; i < layout.boxes.Count; i++)
            {
                var spec = layout.boxes[i];

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
                    ? LayoutUtils.ResolveBeltSlotIndex(spec, _beltSlots)
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
            int maxOps = pending + _beltSlots.Count;

            while (safety < maxOps)
            {
                if (pending <= 0)
                {
                    break;
                }

                // Check front color still matches.
                if (!container.TryPeek(out var peek) || peek.Color != targetColor)
                {
                    break;
                }

                var result = _game.TryReleaseFromContainer(containerIndex);
                if (result == ReleaseResult.BeltBlocked)
                {
                    // Slot is occupied, wait and retry next frame/interval. Belt moves independently.
                    yield return new WaitForSeconds(releaseBlockedRetry);
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

                yield return new WaitForSeconds(releaseInterval);
                safety++;
            }

            _isReleasing = false;
            _activeReleasePort = null;
        }

        private void EnsureCounterUI()
        {
            if (beltCounterUI != null)
            {
                return;
            }

            beltCounterUI = FindObjectOfType<BeltCounterUI>();
            if (beltCounterUI != null)
            {
                return;
            }

            var canvasGO = new GameObject("BeltCounterCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO);

            var textGO = new GameObject("BeltCounter");
            textGO.transform.SetParent(canvasGO.transform, false);
            var text = textGO.AddComponent<Text>();
            text.alignment = TextAnchor.UpperLeft;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.text = "空余格: -/-";
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(10f, -10f);

            beltCounterUI = textGO.AddComponent<BeltCounterUI>();
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
