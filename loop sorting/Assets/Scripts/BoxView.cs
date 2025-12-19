using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace LoopSorting
{
    [RequireComponent(typeof(BoxCollider))]
    public class BoxView : MonoBehaviour
    {
        private static Texture2D _mouthFlashTexture;

        public int ContainerIndex { get; private set; }
        public GameRuntimeController Controller { get; private set; }

        private readonly List<GameObject> _blockVisuals = new List<GameObject>();
        private readonly List<BlockColor?> _slotColors = new List<BlockColor?>();
        // "Authored" hidden state coming from gameplay data (Block.Hidden). Never modified by reveal rules.
        private readonly List<bool> _slotAuthoredHidden = new List<bool>();
        // Final hidden state after applying run-based reveal rules. Used for visuals.
        private readonly List<bool> _slotHidden = new List<bool>();
        private int _columns = 1;
        private int _rows = 1;
        private int _capacity = 1;
        private Vector2 _blockSize = new Vector2(0.45f, 0.45f);
        private Vector2 _boxSize = Vector2.one;
        private OpeningSide _opening = OpeningSide.Top;
        private List<Vector2Int> _cellOrder = new List<Vector2Int>();
        private List<int> _tmpIndices = new List<int>();
        private GameObject _lockOverlay;
        private GameObject _lockBadge;
        private GameObject _lockMarkerPlate;
        private GameObject _lockMarkerColorHalo;
        private GameObject _lockMarkerDisc;
        private GameObject _lockMarkerIcon;
        private bool _locked;
        private bool _completed;
        private GameObject _completedOverlay;
        private GameObject _completedFrameGlow;
        private GameObject _completedGlass;
        private GameObject _completedBadge;
        private GameObject _completedBurst;
        private GameObject _completedConfetti;
        private Coroutine _completedFxRoutine;

        private const float CompletedNineSliceBorderFrac = 0.32f;
        private const float CompletedCapAlpha = 0.92f;
        private const float CompletedGlassAlpha = 0.22f;
        private const float CompletedBadgeAlpha = 0.95f;
        private const float CompletedCapScale = 1.04f;
        private const float CompletedGlassScale = 1.00f;
        private const float CompletedOverlayPopSeconds = 0.18f;
        private const float LockNineSliceBorderFrac = 0.09f;
        private const float LockOverlayScale = 1.0f;
        private const float LockOverlayBaseAlpha = 0.50f;
        private LineRenderer _frontOutline;
        private readonly List<LineRenderer> _boxOutlineSegments = new List<LineRenderer>();
        private GameObject _boxRim;
        private GameObject _boxCavity;
        private Renderer _boxCavityRenderer;
        private readonly GameObject[] _boxRimEdges = new GameObject[4]; // Top, Right, Bottom, Left
        private readonly Renderer[] _boxRimEdgeRenderers = new Renderer[4];
        private readonly GameObject[] _boxRimCorners = new GameObject[4]; // TL, TR, BR, BL
        private readonly Renderer[] _boxRimCornerRenderers = new Renderer[4];
        private Material _boxRimEdgeMaterial;
        private Material _boxRimCornerMaterial;
        private Material _boxCavityMaterial;
        private bool _hasBoxFrame;
        private readonly Dictionary<int, Coroutine> _incomingCoroutines = new Dictionary<int, Coroutine>();
        private readonly HashSet<int> _incomingAnimatingSlots = new HashSet<int>();
        private readonly Dictionary<int, Vector3> _slotFinalLocalPos = new Dictionary<int, Vector3>();
        private Vector3 _mouthLocalPos;
        private Vector3 _mouthLocalNormal;
        private Vector3 _baseLocalPos;
        private Vector3 _baseLocalScale;
        private Coroutine _tapRoutine;
        private Coroutine _denyRoutine;
        private Coroutine _shuffleRoutine;
        private Coroutine _lockAnimRoutine;
        private Coroutine _mouthFlashRoutine;
        private GameObject _mouthFlash;
        private GameObject _mouthIndicator;
        private Coroutine _mouthRippleRoutine;
        private GameObject _mouthRipple;
        private Coroutine _mouthSquashRoutine;
        private Vector3 _mouthIndicatorBaseScale = Vector3.one;

        // Render / layering guideline (smaller Z = closer to camera):
        // Blocks:            z = 0
        // Completed overlay: z = -0.18, queue 3000
        // Lock overlay:      z = -0.24, queue 3100
        // Lock badge:        z = -0.28, queue 3200
        private const float CompletedOverlayZ = -0.18f;
        private const int CompletedQueue = 3000;
        private const float LockOverlayZ = -0.24f;
        private const int LockOverlayQueue = 3100;
        private const float LockBadgeZ = -0.28f;
        private const int LockBadgeQueue = 3200;
        // Depth offset for the black "front outline". With a tilted orthographic camera this also affects screen-Y,
        // so keep it small to align visually with the box face.
        private const float OutlineZ = 0.08f;
        private const int FrontOutlineQueue = 2990;
        // Box rim/cavity should stay behind the black outline, but still in front of the background.
        private const float BoxOutlineZ = 0.10f;
        private const float BoxVisualScale = 1.00f;
        private const float BoxWallFracOfCell = 0.22f;
        private const float BoxWallMinFracOfMinDim = 0.06f;
        private const float BoxWallMaxFracOfMinDim = 0.14f;
        private const float BoxPadClosedMul = 1.05f;
        private const float BoxPadOpenMul = 0.25f;
        // Keep box rim/cavity behind mouth FX and overlays, but still in the transparent pipeline so "Order in Layer" works.
        private const int BoxCavitySortingOrder = 2800;
        private const int BoxCavityRenderQueue = 2800;
        private const int BoxRimSortingOrder = 2810;
        private const int BoxRimRenderQueue = 2810;
        private const float BoxCavityZ = 0.11f;
        private const float IncomingMinSeconds = 0.06f;
        private const float IncomingMaxSeconds = 0.22f;

        public void Init(int containerIndex, GameRuntimeController controller, Vector2 size, int columns, int rows, Vector2 blockSize, OpeningSide opening)
        {
            ContainerIndex = containerIndex;
            Controller = controller;
            _columns = Mathf.Max(1, columns);
            _rows = Mathf.Max(1, rows);
            _capacity = _columns * _rows;
            _blockSize = blockSize;
            _boxSize = size;
            _opening = opening;
            _cellOrder = BuildCellOrder(_columns, _rows, _opening);
            _slotColors.Clear();
            _slotAuthoredHidden.Clear();
            _slotHidden.Clear();
            _incomingCoroutines.Clear();
            _incomingAnimatingSlots.Clear();
            _slotFinalLocalPos.Clear();
            CacheMouth();
            _baseLocalPos = transform.localPosition;
            _baseLocalScale = transform.localScale;
            EnsureMouthIndicator();

            var collider = GetComponent<BoxCollider>();
            collider.size = new Vector3(size.x, size.y, 0.3f);
            collider.center = Vector3.zero;

            EnsureSlotCapacity();
            BuildBoxOutline();
            TryBuildBoxCavity();
        }

        public void SyncBlocks(IReadOnlyList<Block> blocks)
        {
            EnsureSlotCapacity();
            if (blocks == null) blocks = new List<Block>();

            _tmpIndices.Clear();
            for (int i = 0; i < _slotColors.Count; i++)
            {
                if (_slotColors[i].HasValue) _tmpIndices.Add(i);
            }

            int oldCount = _tmpIndices.Count;
            int newCount = Mathf.Min(blocks.Count, _capacity);

            bool animateInsert = false;
            int animateSlot = -1;

            if (newCount == 0)
            {
                if (oldCount > 0)
                {
                    for (int i = 0; i < _tmpIndices.Count; i++)
                    {
                        int idx = _tmpIndices[i];
                        CancelIncoming(idx);
                        _slotColors[idx] = null;
                        _slotAuthoredHidden[idx] = false;
                        _slotHidden[idx] = false;
                    }
                }
            }
            else if (oldCount == 0)
            {
                // Initial fill: pack to the inner side (end) so the empty space stays near the opening.
                RebuildFromBlocks(blocks, newCount);
            }
            else
            {
                int expectedStartOld = _capacity - oldCount;
                int expectedStartNew = _capacity - newCount;
                int currentStart = _tmpIndices[0];

                // If the occupied range is not a contiguous tail, do a full rebuild.
                bool contiguousTail = currentStart == expectedStartOld;
                if (contiguousTail)
                {
                    for (int i = 0; i < oldCount; i++)
                    {
                        if (expectedStartOld + i >= _slotColors.Count) { contiguousTail = false; break; }
                        if (!_slotColors[expectedStartOld + i].HasValue) { contiguousTail = false; break; }
                    }
                }

                int delta = newCount - oldCount;
                if (!contiguousTail || delta > 1 || delta < -1)
                {
                    RebuildFromBlocks(blocks, newCount);
                }
                else if (delta == 1)
                {
                    // One block inserted at the front (mouth side). Place it in the next outer slot of the packed tail.
                    int slot = expectedStartNew;
                    if (slot >= 0 && slot < _slotColors.Count && !_slotColors[slot].HasValue)
                    {
                        _slotColors[slot] = blocks[0].Color;
                        _slotAuthoredHidden[slot] = blocks[0].Hidden;
                        _slotHidden[slot] = blocks[0].Hidden;
                        animateInsert = true;
                        animateSlot = slot;
                    }
                    else
                    {
                        RebuildFromBlocks(blocks, newCount);
                    }
                }
                else if (delta == -1)
                {
                    // One block removed from the front. Clear the current outermost occupied slot.
                    int slot = expectedStartOld;
                    if (slot >= 0 && slot < _slotColors.Count && _slotColors[slot].HasValue)
                    {
                        CancelIncoming(slot);
                        _slotColors[slot] = null;
                        _slotAuthoredHidden[slot] = false;
                        _slotHidden[slot] = false;
                    }
                    else
                    {
                        RebuildFromBlocks(blocks, newCount);
                    }
                }
                else
                {
                    // Same count: verify the packed tail still matches; otherwise rebuild (e.g., boosters).
                    bool matches = true;
                    for (int i = 0; i < newCount; i++)
                    {
                        int slot = expectedStartNew + i;
                        if (slot < 0 || slot >= _slotColors.Count) { matches = false; break; }
                        if (!_slotColors[slot].HasValue) { matches = false; break; }
                        if (_slotColors[slot].Value != blocks[i].Color) { matches = false; break; }
                        if (_slotAuthoredHidden[slot] != blocks[i].Hidden) { matches = false; break; }
                    }
                    if (!matches)
                    {
                        RebuildFromBlocks(blocks, newCount);
                    }
                }
            }

            // Refresh occupied list for hidden-run enforcement.
            _tmpIndices.Clear();
            for (int i = 0; i < _slotColors.Count; i++)
            {
                if (_slotColors[i].HasValue) _tmpIndices.Add(i);
            }

            bool revealTriggered = false;
            BlockColor revealColor = default;

            // Enforce run-based hidden logic: same-color consecutive blocks share hidden state;
            // if the run touches the outermost position, reveal the whole run.
            for (int i = 0; i < _tmpIndices.Count;)
            {
                int idx = _tmpIndices[i];
                var color = _slotColors[idx].Value;
                bool runHidden = _slotAuthoredHidden[idx];
                if (i == 0)
                {
                    runHidden = false; // front run always revealed
                }

                int j = i;
                while (j < _tmpIndices.Count)
                {
                    int idx2 = _tmpIndices[j];
                    if (_slotColors[idx2].Value != color) break;
                    if (!_locked && !_completed && i == 0 && _slotHidden[idx2] && !runHidden)
                    {
                        revealTriggered = true;
                        revealColor = color;
                    }
                    _slotHidden[idx2] = runHidden;
                    j++;
                }
                i = j;
            }

            RefreshVisuals();

            if (revealTriggered)
            {
                Controller?.OnHiddenReveal(ContainerIndex, revealColor);
                PlayMouthFlash(BlockVisual.ToUnityColor(revealColor), sizeFactor: 1.25f, seconds: 0.22f);
            }

            if (animateInsert && animateSlot >= 0)
            {
                StartIncomingAnimation(animateSlot);
            }
        }

        private void RebuildFromBlocks(IReadOnlyList<Block> blocks, int newCount)
        {
            for (int i = 0; i < _slotColors.Count; i++)
            {
                if (_slotColors[i].HasValue) CancelIncoming(i);
                _slotColors[i] = null;
                _slotAuthoredHidden[i] = false;
                _slotHidden[i] = false;
            }

            int count = Mathf.Min(newCount, _capacity);
            int start = Mathf.Clamp(_capacity - count, 0, _capacity);
            for (int i = 0; i < count; i++)
            {
                int slot = start + i;
                if (slot < 0 || slot >= _slotColors.Count) break;
                _slotColors[slot] = blocks[i].Color;
                _slotAuthoredHidden[slot] = blocks[i].Hidden;
                _slotHidden[slot] = blocks[i].Hidden;
            }
        }

        private void OnMouseUpAsButton()
        {
            if (_locked || _completed) return;
            Controller?.HandleContainerClick(ContainerIndex);
        }

        private void EnsureSlotCapacity()
        {
            while (_blockVisuals.Count < _capacity)
            {
                _blockVisuals.Add(null);
            }
            while (_blockVisuals.Count > _capacity)
            {
                var last = _blockVisuals[_blockVisuals.Count - 1];
                if (last != null)
                {
                    Destroy(last);
                }
                _blockVisuals.RemoveAt(_blockVisuals.Count - 1);
            }

            while (_slotColors.Count < _capacity)
            {
                _slotColors.Add(null);
                _slotAuthoredHidden.Add(false);
                _slotHidden.Add(false);
            }
            while (_slotColors.Count > _capacity)
            {
                int lastIdx = _slotColors.Count - 1;
                CancelIncoming(lastIdx);
                _slotColors.RemoveAt(_slotColors.Count - 1);
                _slotAuthoredHidden.RemoveAt(_slotAuthoredHidden.Count - 1);
                _slotHidden.RemoveAt(_slotHidden.Count - 1);
            }
        }

        private void RefreshVisuals()
        {
            for (int i = 0; i < _slotColors.Count; i++)
            {
                var colorOpt = _slotColors[i];
                if (colorOpt.HasValue)
                {
                    if (_blockVisuals[i] == null)
                    {
                        var go = BlockVisual.CreateBlock(colorOpt.Value);
                        go.transform.SetParent(transform, false);
                        _blockVisuals[i] = go;
                    }
                    ApplyBlockVisual(i, colorOpt.Value);
                }
                else
                {
                    if (_blockVisuals[i] != null)
                    {
                        Destroy(_blockVisuals[i]);
                        _blockVisuals[i] = null;
                    }
                }
            }
        }

        private void ApplyBlockVisual(int slotIndex, BlockColor color)
        {
            int col = slotIndex % _columns;
            int row = slotIndex / _columns;

            if (_cellOrder != null && _cellOrder.Count == _columns * _rows && slotIndex < _cellOrder.Count)
            {
                col = _cellOrder[slotIndex].x;
                row = _cellOrder[slotIndex].y;
            }

            var rect = GetBlocksLocalRect();
            var cellSize = new Vector2(
                rect.width / _columns,
                rect.height / _rows
            );

            var origin = new Vector2(rect.xMin + cellSize.x * 0.5f, rect.yMax - cellSize.y * 0.5f);
            var pos = origin + new Vector2(col * cellSize.x, -row * cellSize.y);

            var finalPos = new Vector3(pos.x, pos.y, 0f);
            _slotFinalLocalPos[slotIndex] = finalPos;
            if (!_incomingAnimatingSlots.Contains(slotIndex))
            {
                _blockVisuals[slotIndex].transform.localPosition = finalPos;
            }
            var fitScale = new Vector3(
                Mathf.Min(_blockSize.x, cellSize.x * 0.9f),
                Mathf.Min(_blockSize.y, cellSize.y * 0.9f),
                Mathf.Min(_blockSize.y, cellSize.y * 0.9f)
            );
            _blockVisuals[slotIndex].transform.localScale = fitScale;
            bool hidden = _slotHidden[slotIndex] && slotIndex > 0;
            var matColor = hidden ? new Color(0.3f, 0.3f, 0.3f, 1f) : BlockVisual.ToUnityColor(color);
            BlockVisual.ApplyColor(_blockVisuals[slotIndex], matColor);
        }

        private struct BoxMetrics
        {
            public Vector2 outerSize;
            public float wall;
            public Rect contentRect;
        }

        private BoxMetrics ComputeBoxMetrics()
        {
            var m = new BoxMetrics();
            m.outerSize = _boxSize * BoxVisualScale;

            float minDim = Mathf.Max(0.0001f, Mathf.Min(m.outerSize.x, m.outerSize.y));
            float cell = Mathf.Min(m.outerSize.x / Mathf.Max(1, _columns), m.outerSize.y / Mathf.Max(1, _rows));

            m.wall = Mathf.Clamp(
                cell * BoxWallFracOfCell,
                minDim * BoxWallMinFracOfMinDim,
                minDim * BoxWallMaxFracOfMinDim);

            float padClosed = Mathf.Clamp(m.wall * BoxPadClosedMul, 0.02f, minDim * 0.25f);
            float padOpen = Mathf.Clamp(m.wall * BoxPadOpenMul, 0f, padClosed);

            float leftPad = _opening == OpeningSide.Left ? padOpen : padClosed;
            float rightPad = _opening == OpeningSide.Right ? padOpen : padClosed;
            float topPad = _opening == OpeningSide.Top ? padOpen : padClosed;
            float bottomPad = _opening == OpeningSide.Bottom ? padOpen : padClosed;

            float w = Mathf.Max(0.001f, m.outerSize.x - leftPad - rightPad);
            float h = Mathf.Max(0.001f, m.outerSize.y - topPad - bottomPad);
            m.contentRect = new Rect(-m.outerSize.x * 0.5f + leftPad, -m.outerSize.y * 0.5f + bottomPad, w, h);
            return m;
        }

        private Rect GetBlocksLocalRect()
        {
            // Decouple visuals from layout:
            // - Blocks always align to a computed "content rect"
            // - Box rim/cavity visuals use the same metrics, without relying on 9-slice borders.
            if (!_hasBoxFrame)
            {
                return new Rect(-_boxSize.x * 0.5f, -_boxSize.y * 0.5f, _boxSize.x, _boxSize.y);
            }

            return ComputeBoxMetrics().contentRect;
        }

        private void CacheMouth()
        {
            Vector2 normal = Vector2.down;
            switch (_opening)
            {
                case OpeningSide.Top: normal = Vector2.up; break;
                case OpeningSide.Bottom: normal = Vector2.down; break;
                case OpeningSide.Left: normal = Vector2.left; break;
                case OpeningSide.Right: normal = Vector2.right; break;
            }

            var half = _boxSize * 0.5f;
            float dist = (normal == Vector2.left || normal == Vector2.right) ? half.x : half.y;
            _mouthLocalNormal = new Vector3(normal.x, normal.y, 0f);
            _mouthLocalPos = _mouthLocalNormal * dist;
        }

        private void CancelIncoming(int slotIndex)
        {
            _incomingAnimatingSlots.Remove(slotIndex);
            if (_incomingCoroutines.TryGetValue(slotIndex, out var co) && co != null)
            {
                StopCoroutine(co);
            }
            _incomingCoroutines.Remove(slotIndex);
        }

        private void StartIncomingAnimation(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _blockVisuals.Count) return;
            var go = _blockVisuals[slotIndex];
            if (go == null) return;
            if (!_slotFinalLocalPos.TryGetValue(slotIndex, out var end)) return;

            CancelIncoming(slotIndex);
            _incomingAnimatingSlots.Add(slotIndex);

            float cell = Mathf.Min(_boxSize.x / Mathf.Max(1, _columns), _boxSize.y / Mathf.Max(1, _rows));
            float pad = Mathf.Max(0.05f, cell * 0.7f);
            var start = _mouthLocalPos + _mouthLocalNormal * pad;

            go.transform.localPosition = start;
            var co = StartCoroutine(AnimateIncoming(slotIndex, start, end));
            _incomingCoroutines[slotIndex] = co;
        }

        private global::System.Collections.IEnumerator AnimateIncoming(int slotIndex, Vector3 start, Vector3 end)
        {
            float duration = 0.12f;
            if (Controller != null)
            {
                duration = Mathf.Clamp(Controller.conveyorTickSeconds * 0.55f, IncomingMinSeconds, IncomingMaxSeconds);
            }
            duration = Mathf.Max(0.0001f, duration);

            float t = 0f;
            while (t < duration)
            {
                if (slotIndex < 0 || slotIndex >= _blockVisuals.Count) break;
                var go = _blockVisuals[slotIndex];
                if (go == null) break;

                float speed = 1f;
                if (Controller != null) speed = Mathf.Max(0.0001f, Controller.EffectiveSpeedMultiplier);

                t += Time.deltaTime * speed;
                float u = Mathf.Clamp01(t / duration);
                go.transform.localPosition = Vector3.Lerp(start, end, u);
                yield return null;
            }

            if (slotIndex >= 0 && slotIndex < _blockVisuals.Count && _blockVisuals[slotIndex] != null)
            {
                _blockVisuals[slotIndex].transform.localPosition = end;
            }

            _incomingAnimatingSlots.Remove(slotIndex);
            _incomingCoroutines.Remove(slotIndex);
        }

        public void ShowFrontOutline(int runCount, bool show)
        {
            if (_slotColors == null || _slotColors.Count == 0 || runCount <= 0)
            {
                show = false;
            }

            if (!show)
            {
                if (_frontOutline != null) _frontOutline.gameObject.SetActive(false);
                return;
            }

            // gather first runCount occupied slots in order (skip empty gaps)
            var indices = new List<int>();
            for (int i = 0; i < _slotColors.Count && indices.Count < runCount; i++)
            {
                if (_slotColors[i].HasValue)
                {
                    indices.Add(i);
                }
            }

            if (indices.Count == 0)
            {
                if (_frontOutline != null) _frontOutline.gameObject.SetActive(false);
                return;
            }

            var rect = GetBlocksLocalRect();
            var cellSize = new Vector2(rect.width / _columns, rect.height / _rows);

            // Build exact outline around the occupied cells (instead of a bounding rectangle),
            // so the outline matches the block count even when the last row is partial.
            var filled = new bool[_columns, _rows];
            for (int i = 0; i < indices.Count; i++)
            {
                int slotIndex = indices[i];
                int col = slotIndex % _columns;
                int row = slotIndex / _columns;
                if (_cellOrder != null && _cellOrder.Count == _columns * _rows && slotIndex < _cellOrder.Count)
                {
                    col = _cellOrder[slotIndex].x;
                    row = _cellOrder[slotIndex].y;
                }
                if (col < 0 || col >= _columns || row < 0 || row >= _rows) continue;
                filled[col, row] = true;
            }

            // Directed boundary edges (clockwise). Encode vertex (x,y) into int key.
            static int Key(int x, int y) => (x & 0xFFFF) | (y << 16);
            static int KeyX(int key) => key & 0xFFFF;
            static int KeyY(int key) => (key >> 16) & 0xFFFF;

            var next = new Dictionary<int, int>(256);
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    if (!filled[c, r]) continue;

                    // top
                    if (r == 0 || !filled[c, r - 1])
                    {
                        next[Key(c, r)] = Key(c + 1, r);
                    }
                    // right
                    if (c == _columns - 1 || !filled[c + 1, r])
                    {
                        next[Key(c + 1, r)] = Key(c + 1, r + 1);
                    }
                    // bottom
                    if (r == _rows - 1 || !filled[c, r + 1])
                    {
                        next[Key(c + 1, r + 1)] = Key(c, r + 1);
                    }
                    // left
                    if (c == 0 || !filled[c - 1, r])
                    {
                        next[Key(c, r + 1)] = Key(c, r);
                    }
                }
            }

            if (next.Count == 0)
            {
                if (_frontOutline != null) _frontOutline.gameObject.SetActive(false);
                return;
            }

            // Choose a stable start vertex (top-most, then left-most).
            int startKey = 0;
            bool hasStart = false;
            foreach (var k in next.Keys)
            {
                if (!hasStart)
                {
                    startKey = k;
                    hasStart = true;
                    continue;
                }
                int y0 = KeyY(startKey), x0 = KeyX(startKey);
                int y1 = KeyY(k), x1 = KeyX(k);
                if (y1 < y0 || (y1 == y0 && x1 < x0))
                {
                    startKey = k;
                }
            }

            var path = new List<Vector3>(next.Count + 1);
            int cur = startKey;
            int guard = 0;
            while (guard++ < next.Count + 2)
            {
                int vx = KeyX(cur);
                int vy = KeyY(cur);
                float px = rect.xMin + vx * cellSize.x;
                float py = rect.yMax - vy * cellSize.y;
                path.Add(new Vector3(px, py, OutlineZ));

                if (!next.TryGetValue(cur, out var nxt))
                {
                    break;
                }
                cur = nxt;
                if (cur == startKey)
                {
                    break;
                }
            }

            if (_frontOutline == null)
            {
                var go = new GameObject("FrontOutline");
                go.transform.SetParent(transform, false);
                _frontOutline = go.AddComponent<LineRenderer>();
                _frontOutline.useWorldSpace = false;
                _frontOutline.loop = true;
                _frontOutline.sortingLayerID = 0;
                _frontOutline.sortingOrder = FrontOutlineQueue;
                EnsureUnlitColorMaterial(go, Color.black, FrontOutlineQueue);
                _frontOutline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _frontOutline.receiveShadows = false;
                _frontOutline.numCapVertices = 2;
                _frontOutline.numCornerVertices = 2;
            }

            if (path.Count < 3)
            {
                _frontOutline.gameObject.SetActive(false);
                return;
            }

            _frontOutline.positionCount = path.Count;
            float width = Mathf.Clamp(Mathf.Min(cellSize.x, cellSize.y) * 0.12f, 0.04f, 0.09f);
            _frontOutline.startWidth = width;
            _frontOutline.endWidth = width;
            for (int i = 0; i < path.Count; i++)
            {
                _frontOutline.SetPosition(i, path[i]);
            }
            _frontOutline.gameObject.SetActive(true);
        }

        public void HideFrontOutline()
        {
            if (_frontOutline != null)
            {
                _frontOutline.gameObject.SetActive(false);
            }
        }

        private void BuildBoxOutline()
        {
            if (TryBuildBoxRim())
            {
                // Clear legacy dashed segments if present.
                for (int i = 0; i < _boxOutlineSegments.Count; i++)
                {
                    var seg = _boxOutlineSegments[i];
                    if (seg != null) Destroy(seg.gameObject);
                }
                _boxOutlineSegments.Clear();
                return;
            }

            // Fallback: legacy dashed outline (keeps game usable if the new box frame textures are missing).
            _hasBoxFrame = false;
            if (_boxRim != null) _boxRim.SetActive(false);

            // clear previous
            foreach (var seg in _boxOutlineSegments)
            {
                if (seg != null) Destroy(seg.gameObject);
            }
            _boxOutlineSegments.Clear();

            float halfX = _boxSize.x * 0.5f;
            float halfY = _boxSize.y * 0.5f;
            // White dashed outline stays outside black outline
            float pad = Mathf.Max(_boxSize.x / _columns, _boxSize.y / _rows) * 0.7f;
            Vector3 bl = new Vector3(-halfX - pad, -halfY - pad, BoxOutlineZ);
            Vector3 tl = new Vector3(-halfX - pad, halfY + pad, BoxOutlineZ);
            Vector3 tr = new Vector3(halfX + pad, halfY + pad, BoxOutlineZ);
            Vector3 br = new Vector3(halfX + pad, -halfY - pad, BoxOutlineZ);

            var edges = new List<(Vector3, Vector3)>();
            if (_opening != OpeningSide.Left) edges.Add((bl, tl));
            if (_opening != OpeningSide.Top) edges.Add((tl, tr));
            if (_opening != OpeningSide.Right) edges.Add((tr, br));
            if (_opening != OpeningSide.Bottom) edges.Add((br, bl));

            foreach (var edge in edges)
            {
                BuildDashedEdge(edge.Item1, edge.Item2);
            }
        }

        private const string BoxRimEdgeTexturePath = "World_Sprites/box_rim_edge_tile.png";
        private const string BoxRimCornerTexturePath = "World_Sprites/box_rim_corner.png";
        private const string BoxCavityTexturePath = "World_Sprites/box_cavity_fill.png";

        private static Material CreateBoxTextureMaterial(Texture2D texture, int renderQueue)
        {
            if (texture == null) return null;

            var shader =
                Shader.Find("LoopSorting/UnlitTexture") ??
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Unlit/Texture") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");

            if (shader == null) return null;

            var mat = new Material(shader);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            else mat.mainTexture = texture;

            TrySetMaterialColor(mat, Color.white);
            mat.renderQueue = renderQueue;

            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
            if (mat.HasProperty("_ZTest")) mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", 0);
            if (mat.HasProperty("_CullMode")) mat.SetInt("_CullMode", 0);

            return mat;
        }

        private static GameObject EnsureQuadChild(GameObject parent, string name, out Renderer renderer)
        {
            renderer = null;
            if (parent == null) return null;

            Transform existing = parent.transform.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = RuntimePrimitives.CreateQuad(name);
                go.name = name;
                go.transform.SetParent(parent.transform, false);
                RemoveCollider(go);
            }

            renderer = go.GetComponent<Renderer>();
            return go;
        }

        private bool TryBuildBoxCavity()
        {
            if (!LoopSortingUIKit.IsAvailable())
            {
                return false;
            }

            var tex = LoopSortingUIKit.LoadTexture(BoxCavityTexturePath);
            if (tex == null)
            {
                if (_boxCavity != null) _boxCavity.SetActive(false);
                return false;
            }

            if (_boxCavity == null)
            {
                _boxCavity = RuntimePrimitives.CreateQuad("BoxCavity");
                _boxCavity.transform.SetParent(transform, false);
                RemoveCollider(_boxCavity);
            }

            _boxCavityRenderer = _boxCavity.GetComponent<Renderer>();
            if (_boxCavityRenderer != null)
            {
                if (_boxCavityMaterial == null || _boxCavityMaterial.mainTexture != tex || _boxCavityMaterial.renderQueue != BoxCavityRenderQueue)
                {
                    _boxCavityMaterial = CreateBoxTextureMaterial(tex, BoxCavityRenderQueue);
                }
                if (_boxCavityMaterial != null)
                {
                    _boxCavityRenderer.sharedMaterial = _boxCavityMaterial;
                }
            }

            SetWorldOverlaySorting(_boxCavity, BoxCavitySortingOrder);

            var metrics = ComputeBoxMetrics();
            var c = metrics.contentRect.center;
            _boxCavity.transform.localPosition = new Vector3(c.x, c.y, BoxCavityZ); // behind blocks (bigger Z)
            _boxCavity.transform.localRotation = Quaternion.identity;
            _boxCavity.transform.localScale = new Vector3(metrics.contentRect.width, metrics.contentRect.height, 1f);
            _boxCavity.SetActive(true);
            return true;
        }

        private bool TryBuildBoxRim()
        {
            if (!LoopSortingUIKit.IsAvailable())
            {
                _hasBoxFrame = false;
                return false;
            }

            var edgeTex = LoopSortingUIKit.LoadTexture(BoxRimEdgeTexturePath);
            var cornerTex = LoopSortingUIKit.LoadTexture(BoxRimCornerTexturePath);
            if (edgeTex == null || cornerTex == null)
            {
                _hasBoxFrame = false;
                if (_boxRim != null) _boxRim.SetActive(false);
                return false;
            }

            if (_boxRim == null)
            {
                _boxRim = new GameObject("BoxRim");
                _boxRim.transform.SetParent(transform, false);
                _boxRim.transform.localScale = Vector3.one;
            }
            _boxRim.transform.localPosition = new Vector3(0f, 0f, BoxOutlineZ);
            _boxRim.transform.localRotation = Quaternion.identity;

            if (_boxRimEdgeMaterial == null || _boxRimEdgeMaterial.mainTexture != edgeTex || _boxRimEdgeMaterial.renderQueue != BoxRimRenderQueue)
            {
                _boxRimEdgeMaterial = CreateBoxTextureMaterial(edgeTex, BoxRimRenderQueue);
            }
            if (_boxRimCornerMaterial == null || _boxRimCornerMaterial.mainTexture != cornerTex || _boxRimCornerMaterial.renderQueue != (BoxRimRenderQueue + 1))
            {
                _boxRimCornerMaterial = CreateBoxTextureMaterial(cornerTex, BoxRimRenderQueue + 1);
            }
            if (_boxRimEdgeMaterial == null || _boxRimCornerMaterial == null)
            {
                _hasBoxFrame = false;
                return false;
            }

            // Edge/corner quad setup.
            _boxRimEdges[0] = EnsureQuadChild(_boxRim, "RimEdge_Top", out _boxRimEdgeRenderers[0]);
            _boxRimEdges[1] = EnsureQuadChild(_boxRim, "RimEdge_Right", out _boxRimEdgeRenderers[1]);
            _boxRimEdges[2] = EnsureQuadChild(_boxRim, "RimEdge_Bottom", out _boxRimEdgeRenderers[2]);
            _boxRimEdges[3] = EnsureQuadChild(_boxRim, "RimEdge_Left", out _boxRimEdgeRenderers[3]);

            _boxRimCorners[0] = EnsureQuadChild(_boxRim, "RimCorner_TL", out _boxRimCornerRenderers[0]);
            _boxRimCorners[1] = EnsureQuadChild(_boxRim, "RimCorner_TR", out _boxRimCornerRenderers[1]);
            _boxRimCorners[2] = EnsureQuadChild(_boxRim, "RimCorner_BR", out _boxRimCornerRenderers[2]);
            _boxRimCorners[3] = EnsureQuadChild(_boxRim, "RimCorner_BL", out _boxRimCornerRenderers[3]);

            for (int i = 0; i < _boxRimEdges.Length; i++)
            {
                if (_boxRimEdgeRenderers[i] != null) _boxRimEdgeRenderers[i].sharedMaterial = _boxRimEdgeMaterial;
                if (_boxRimEdges[i] != null) SetWorldOverlaySorting(_boxRimEdges[i], BoxRimSortingOrder);
            }
            for (int i = 0; i < _boxRimCorners.Length; i++)
            {
                if (_boxRimCornerRenderers[i] != null) _boxRimCornerRenderers[i].sharedMaterial = _boxRimCornerMaterial;
                if (_boxRimCorners[i] != null) SetWorldOverlaySorting(_boxRimCorners[i], BoxRimSortingOrder + 1);
            }

            var metrics = ComputeBoxMetrics();
            float halfW = metrics.outerSize.x * 0.5f;
            float halfH = metrics.outerSize.y * 0.5f;
            float wall = Mathf.Max(0.001f, metrics.wall);
            float corner = wall;

            bool edgeTop = _opening != OpeningSide.Top;
            bool edgeRight = _opening != OpeningSide.Right;
            bool edgeBottom = _opening != OpeningSide.Bottom;
            bool edgeLeft = _opening != OpeningSide.Left;

            bool cornerTL = edgeTop && edgeLeft;
            bool cornerTR = edgeTop && edgeRight;
            bool cornerBR = edgeBottom && edgeRight;
            bool cornerBL = edgeBottom && edgeLeft;

            // Corners (square patches). Keep orientation consistent (no per-corner rotation) so highlights stay stable.
            if (_boxRimCorners[0] != null)
            {
                _boxRimCorners[0].transform.localRotation = Quaternion.identity;
                _boxRimCorners[0].transform.localPosition = new Vector3(-halfW + corner * 0.5f, halfH - corner * 0.5f, 0f);
                _boxRimCorners[0].transform.localScale = new Vector3(corner, corner, 1f);
                _boxRimCorners[0].SetActive(cornerTL);
            }
            if (_boxRimCorners[1] != null)
            {
                _boxRimCorners[1].transform.localRotation = Quaternion.identity;
                _boxRimCorners[1].transform.localPosition = new Vector3(halfW - corner * 0.5f, halfH - corner * 0.5f, 0f);
                _boxRimCorners[1].transform.localScale = new Vector3(corner, corner, 1f);
                _boxRimCorners[1].SetActive(cornerTR);
            }
            if (_boxRimCorners[2] != null)
            {
                _boxRimCorners[2].transform.localRotation = Quaternion.identity;
                _boxRimCorners[2].transform.localPosition = new Vector3(halfW - corner * 0.5f, -halfH + corner * 0.5f, 0f);
                _boxRimCorners[2].transform.localScale = new Vector3(corner, corner, 1f);
                _boxRimCorners[2].SetActive(cornerBR);
            }
            if (_boxRimCorners[3] != null)
            {
                _boxRimCorners[3].transform.localRotation = Quaternion.identity;
                _boxRimCorners[3].transform.localPosition = new Vector3(-halfW + corner * 0.5f, -halfH + corner * 0.5f, 0f);
                _boxRimCorners[3].transform.localScale = new Vector3(corner, corner, 1f);
                _boxRimCorners[3].SetActive(cornerBL);
            }

            // Edges: build as quads with a constant wall thickness; missing edge indicates opening direction.
            void SetEdge(int idx, bool visible, Vector3 pos, float length, float rotDeg)
            {
                var go = _boxRimEdges[idx];
                if (go == null)
                {
                    return;
                }

                if (!visible || length <= 0.001f)
                {
                    go.SetActive(false);
                    return;
                }

                go.transform.localPosition = pos;
                go.transform.localRotation = Quaternion.Euler(0f, 0f, rotDeg);
                go.transform.localScale = new Vector3(length, wall, 1f);
                go.SetActive(true);
            }

            // Horizontal edges (rotation 0, length along X).
            float topLeftInset = cornerTL ? corner : 0f;
            float topRightInset = cornerTR ? corner : 0f;
            float topLen = Mathf.Max(0f, metrics.outerSize.x - topLeftInset - topRightInset);
            SetEdge(0, edgeTop, new Vector3((-halfW + topLeftInset) + topLen * 0.5f, halfH - wall * 0.5f, 0f), topLen, 0f);

            float bottomLeftInset = cornerBL ? corner : 0f;
            float bottomRightInset = cornerBR ? corner : 0f;
            float bottomLen = Mathf.Max(0f, metrics.outerSize.x - bottomLeftInset - bottomRightInset);
            SetEdge(2, edgeBottom, new Vector3((-halfW + bottomLeftInset) + bottomLen * 0.5f, -halfH + wall * 0.5f, 0f), bottomLen, 0f);

            // Vertical edges (rotation 90, length along local X -> world Y).
            float rightTopInset = cornerTR ? corner : 0f;
            float rightBottomInset = cornerBR ? corner : 0f;
            float rightLen = Mathf.Max(0f, metrics.outerSize.y - rightTopInset - rightBottomInset);
            SetEdge(1, edgeRight, new Vector3(halfW - wall * 0.5f, (-halfH + rightBottomInset) + rightLen * 0.5f, 0f), rightLen, 90f);

            float leftTopInset = cornerTL ? corner : 0f;
            float leftBottomInset = cornerBL ? corner : 0f;
            float leftLen = Mathf.Max(0f, metrics.outerSize.y - leftTopInset - leftBottomInset);
            SetEdge(3, edgeLeft, new Vector3(-halfW + wall * 0.5f, (-halfH + leftBottomInset) + leftLen * 0.5f, 0f), leftLen, 90f);

            _boxRim.SetActive(true);
            _hasBoxFrame = true;
            return true;
        }

        private void BuildDashedEdge(Vector3 start, Vector3 end)
        {
            float len = Vector3.Distance(start, end);
            if (len < 0.0001f) return;
            float dash = Mathf.Max(0.05f, Mathf.Min(_boxSize.x, _boxSize.y) * 0.18f);
            float gap = dash * 0.45f;

            var dir = (end - start).normalized;
            float traveled = 0f;
            while (traveled < len)
            {
                float segLen = Mathf.Min(dash, len - traveled);
                var segStart = start + dir * traveled;
                var segEnd = segStart + dir * segLen;
                var segGO = new GameObject("BoxOutlineSeg");
                segGO.transform.SetParent(transform, false);
                var lr = segGO.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.loop = false;
                lr.startWidth = 0.06f;
                lr.endWidth = 0.06f;
                lr.positionCount = 2;
                lr.SetPosition(0, segStart);
                lr.SetPosition(1, segEnd);
                var shader =
                    Shader.Find("Unlit/Color") ??
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("UI/Default") ??
                    Shader.Find("Standard");
                if (shader != null)
                {
                    var mat = new Material(shader)
                    {
                        color = Color.white,
                    };
                    mat.renderQueue = 3300;
                    lr.sharedMaterial = mat;
                }
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.numCapVertices = 0;
                lr.numCornerVertices = 0;
                _boxOutlineSegments.Add(lr);

                traveled += segLen + gap;
            }
        }

        public void SetLocked(bool val, BlockColor unlockColor = BlockColor.Red)
        {
            bool wasLocked = _locked;
            _locked = val;
            if (_lockOverlay == null)
            {
                // Prefer Unity's native 9-slice (SpriteRenderer + Sliced) for lock overlay.
                var overlayGO = new GameObject("LockOverlay");
                overlayGO.transform.SetParent(transform, false);
                overlayGO.transform.localPosition = new Vector3(0f, 0f, LockOverlayZ); // in front of blocks
                overlayGO.transform.localRotation = Quaternion.identity;
                overlayGO.transform.localScale = Vector3.one;
                _lockOverlay = overlayGO;

                var sr = overlayGO.AddComponent<SpriteRenderer>();
                sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sr.receiveShadows = false;
                sr.color = Color.white;
                sr.sortingOrder = LockOverlayQueue;

                if (LoopSortingUIKit.IsAvailable())
                {
                    // Use a higher PPU so the 9-slice borders stay small in world units (prevents corner stretching on small boxes).
                    var sprite = LoopSortingUIKit.LoadSprite("World_Sprites/lock_overlay.png", pixelsPerUnit: 500f, applyNineSlice: true);
                    if (sprite != null)
                    {
                        sr.sprite = sprite;
                        sr.drawMode = sprite.border.sqrMagnitude > 0.0001f ? SpriteDrawMode.Sliced : SpriteDrawMode.Simple;
                    }
                }

                // Fallback if sprite isn't available for some reason.
                if (sr.sprite == null)
                {
                    var tex = LoopSortingUIKit.IsAvailable() ? LoopSortingUIKit.LoadTextureByKey("world.lock_overlay") : null;
                    if (tex != null)
                    {
                        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                        sr.drawMode = SpriteDrawMode.Simple;
                    }
                }

                // Material: keep it unlit-ish and force queue ordering to match the rest of our world overlays.
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    mat.renderQueue = LockOverlayQueue;
                    sr.sharedMaterial = mat;
                }
                // Marker root sits above overlay (do not parent under overlay to avoid double Z offsets).
                _lockBadge = new GameObject("LockMarker");
                _lockBadge.transform.SetParent(transform, false);
                _lockBadge.transform.localPosition = new Vector3(0f, 0f, LockBadgeZ);
                _lockBadge.transform.localRotation = Quaternion.identity;
                _lockBadge.transform.localScale = Vector3.one;

                _lockMarkerPlate = RuntimePrimitives.CreateQuad("MarkerPlate");
                _lockMarkerPlate.transform.SetParent(_lockBadge.transform, false);
                _lockMarkerPlate.transform.localPosition = Vector3.zero;
                var plateCol = _lockMarkerPlate.GetComponent<Collider>();
                if (plateCol != null) GameObject.Destroy(plateCol);

                _lockMarkerDisc = RuntimePrimitives.CreateQuad("MarkerColorDisc");
                _lockMarkerDisc.transform.SetParent(_lockBadge.transform, false);
                _lockMarkerDisc.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                var discCol = _lockMarkerDisc.GetComponent<Collider>();
                if (discCol != null) GameObject.Destroy(discCol);

                _lockMarkerIcon = RuntimePrimitives.CreateQuad("MarkerLockIcon");
                _lockMarkerIcon.transform.SetParent(_lockBadge.transform, false);
                _lockMarkerIcon.transform.localPosition = new Vector3(0f, 0f, -0.02f);
                var iconCol = _lockMarkerIcon.GetComponent<Collider>();
                if (iconCol != null) GameObject.Destroy(iconCol);

                if (LoopSortingUIKit.IsAvailable())
                {
                    var plateTex = LoopSortingUIKit.LoadTextureByKey("world.lock_marker_plate");
                    var discTex = TryLoadBlockColorTexture(unlockColor) ?? LoopSortingUIKit.LoadTextureByKey("world.lock_marker_color_disc");
                    var iconTex = LoopSortingUIKit.LoadTextureByKey("world.lock_marker_lock_icon");

                    if (plateTex != null)
                    {
                        var r = _lockMarkerPlate.GetComponent<Renderer>();
                        if (r != null)
                        {
                            var mat = LoopSortingUIKit.CreateUnlitTextureMaterial(plateTex, Color.white, LockBadgeQueue);
                            if (mat != null) r.sharedMaterial = mat;
                        }
                    }
                    if (discTex != null)
                    {
                        var r = _lockMarkerDisc.GetComponent<Renderer>();
                        if (r != null)
                        {
                            var mat = LoopSortingUIKit.CreateUnlitTextureMaterial(discTex, Color.white, LockBadgeQueue + 1);
                            if (mat != null) r.sharedMaterial = mat;
                        }
                    }
                    if (iconTex != null)
                    {
                        var r = _lockMarkerIcon.GetComponent<Renderer>();
                        if (r != null)
                        {
                            var mat = LoopSortingUIKit.CreateUnlitTextureMaterial(iconTex, Color.white, LockBadgeQueue + 2);
                            if (mat != null) r.sharedMaterial = mat;
                        }
                    }
                }

                // Fallback materials if textures aren't available (avoid pink default).
                EnsureUnlitColorMaterial(_lockMarkerPlate, Color.white, LockBadgeQueue);
                EnsureUnlitColorMaterial(_lockMarkerDisc, Color.white, LockBadgeQueue + 1);
                EnsureUnlitColorMaterial(_lockMarkerIcon, Color.white, LockBadgeQueue + 2);
            }

            // Sorting: SpriteRenderer uses sortingOrder, while MeshRenderer defaults to 0.
            // Without explicitly setting this, the lock overlay (sortingOrder=LockOverlayQueue) can render above the badge.
            SetWorldOverlaySorting(_lockMarkerPlate, LockBadgeQueue);
            SetWorldOverlaySorting(_lockMarkerDisc, LockBadgeQueue + 1);
            SetWorldOverlaySorting(_lockMarkerIcon, LockBadgeQueue + 2);

            float badgeWidth = _boxSize.x * 0.96f;
            float badgeHeightMax = _boxSize.y * 0.92f;
            float plateWidth = Mathf.Max(0.05f, badgeWidth);
            float plateHeight = plateWidth;
            if (LoopSortingUIKit.IsAvailable())
            {
                var plateTex = LoopSortingUIKit.LoadTextureByKey("world.lock_marker_plate");
                if (plateTex != null && plateTex.width > 0)
                {
                    plateHeight = plateWidth * ((float)plateTex.height / plateTex.width);
                }
            }
            if (plateHeight > badgeHeightMax && badgeHeightMax > 0.05f)
            {
                float s = badgeHeightMax / plateHeight;
                plateWidth *= s;
                plateHeight *= s;
            }

            if (_lockMarkerPlate != null)
            {
                _lockMarkerPlate.transform.localScale = new Vector3(plateWidth, plateHeight, 1f);
            }
            if (_lockMarkerDisc != null)
            {
                // Make the tinted disc slightly larger than the lock icon so a colored edge is visible.
                float baseSize = Mathf.Min(plateWidth, plateHeight);
                float disc = baseSize * 0.92f;
                _lockMarkerDisc.transform.localScale = new Vector3(disc, disc, 1f);
                var discR = _lockMarkerDisc.GetComponent<Renderer>();
                if (discR != null && discR.sharedMaterial != null)
                {
                    var blockTex = TryLoadBlockColorTexture(unlockColor);
                    if (blockTex != null)
                    {
                        if (discR.sharedMaterial.HasProperty("_MainTex")) discR.sharedMaterial.SetTexture("_MainTex", blockTex);
                        else discR.sharedMaterial.mainTexture = blockTex;
                        TrySetMaterialColor(discR.sharedMaterial, Color.white);
                    }
                    else
                    {
                        var c = BlockVisual.ToUnityColor(unlockColor);
                        c.a = 1f;
                        TrySetMaterialColor(discR.sharedMaterial, c);
                    }
                }
            }
            if (_lockMarkerIcon != null)
            {
                float baseSize = Mathf.Min(plateWidth, plateHeight);
                float iconW = baseSize * 0.70f;
                float iconH = iconW;
                if (LoopSortingUIKit.IsAvailable())
                {
                    var iconTex = LoopSortingUIKit.LoadTextureByKey("world.lock_marker_lock_icon");
                    if (iconTex != null && iconTex.width > 0)
                    {
                        iconH = iconW * ((float)iconTex.height / iconTex.width);
                    }
                }
                _lockMarkerIcon.transform.localScale = new Vector3(iconW, iconH, 1f);
            }

            if (_lockAnimRoutine != null) StopCoroutine(_lockAnimRoutine);
            _lockAnimRoutine = null;
            UpdateMouthIndicatorVisibility();

            if (_lockOverlay != null)
            {
                float w = _boxSize.x * LockOverlayScale;
                float h = _boxSize.y * LockOverlayScale;

                var sr = _lockOverlay.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null && sr.drawMode == SpriteDrawMode.Sliced)
                {
                    sr.size = new Vector2(w, h);
                    _lockOverlay.transform.localScale = Vector3.one;
                }
                else if (sr != null && sr.sprite != null)
                {
                    // Simple sprite fallback: scale to target size.
                    var b = sr.sprite.bounds;
                    float sx = b.size.x > 0.0001f ? w / b.size.x : 1f;
                    float sy = b.size.y > 0.0001f ? h / b.size.y : 1f;
                    _lockOverlay.transform.localScale = new Vector3(sx, sy, 1f);
                }
                else
                {
                    // Legacy mesh fallback.
                    UpdateNineSliceMesh(_lockOverlay, w, h, LockNineSliceBorderFrac);
                }
            }

            if (val)
            {
                if (_lockOverlay != null) _lockOverlay.SetActive(true);
                if (_lockBadge != null) _lockBadge.SetActive(true);
                SetLockVisualAlpha(wasLocked ? 1f : 0f);
                if (!wasLocked)
                {
                    _lockAnimRoutine = StartCoroutine(AnimateLockAlpha(from: 0f, to: 1f, seconds: 0.16f));
                }
            }
            else
            {
                if (wasLocked)
                {
                    if (_lockOverlay != null) _lockOverlay.SetActive(true);
                    if (_lockBadge != null) _lockBadge.SetActive(true);
                    SetLockVisualAlpha(1f);
                    _lockAnimRoutine = StartCoroutine(AnimateUnlock(from: 1f, to: 0f, seconds: 0.18f));
                }
                else
                {
                    if (_lockOverlay != null) _lockOverlay.SetActive(false);
                    if (_lockBadge != null) _lockBadge.SetActive(false);
                }
            }
        }

        public void PlayTapFeedback()
        {
            if (_tapRoutine != null) StopCoroutine(_tapRoutine);
            _tapRoutine = StartCoroutine(MotionUtil.ScalePunch(transform, _baseLocalScale, punchScale: 0.06f, seconds: 0.14f));
        }

        public void PlayDeniedFeedback()
        {
            if (_denyRoutine != null) StopCoroutine(_denyRoutine);
            _denyRoutine = StartCoroutine(MotionUtil.ShakeLocalPosition(transform, _baseLocalPos, amplitude: Mathf.Max(0.04f, _boxSize.x * 0.04f), seconds: 0.18f, shakes: 8));
            PlayMouthFlash(new Color(1f, 0.3f, 0.3f, 1f), sizeFactor: 1.0f, seconds: 0.18f);
        }

        public void PlayInfoHint(Color color, float sizeFactor = 1.1f, float seconds = 0.18f)
        {
            PlayMouthFlash(color, sizeFactor, seconds);
        }

        public void PlayMouthRipple(Color color, float seconds = 0.14f)
        {
            EnsureMouthRipple();
            if (_mouthRipple == null) return;
            if (_mouthRippleRoutine != null) StopCoroutine(_mouthRippleRoutine);
            _mouthRippleRoutine = StartCoroutine(AnimateMouthRipple(color, seconds));
        }

        public void PlayMouthSquash(Color color, float seconds = 0.14f)
        {
            EnsureMouthIndicator();
            if (_mouthIndicator == null) return;
            if (_mouthSquashRoutine != null) StopCoroutine(_mouthSquashRoutine);
            _mouthSquashRoutine = StartCoroutine(AnimateMouthSquash(color, seconds));
        }

        public void PlayShuffleJiggle(float seconds = 0.85f)
        {
            if (_shuffleRoutine != null) StopCoroutine(_shuffleRoutine);
            _shuffleRoutine = StartCoroutine(AnimateShuffleJiggle(seconds));
        }

        public void PlayPortRejectFeedback(ConveyorPortOutcome outcome)
        {
            if (outcome == ConveyorPortOutcome.SkippedEmptyBoxPreferredTarget) return;

            Color c = new Color(1f, 0.35f, 0.35f, 1f);
            switch (outcome)
            {
                case ConveyorPortOutcome.RejectedLocked: c = new Color(0.8f, 0.8f, 0.9f, 1f); break;
                case ConveyorPortOutcome.RejectedBusy: c = new Color(0.75f, 0.75f, 0.75f, 1f); break;
                case ConveyorPortOutcome.RejectedFull: c = new Color(1f, 0.72f, 0.25f, 1f); break;
                case ConveyorPortOutcome.RejectedMismatch: c = new Color(1f, 0.35f, 0.35f, 1f); break;
            }
            PlayMouthFlash(c, sizeFactor: 1.05f, seconds: 0.16f);
        }

        private IEnumerator AnimateShuffleJiggle(float seconds)
        {
            seconds = Mathf.Clamp(seconds, 0.55f, 1.15f);
            var basePos = _baseLocalPos;

            float lift = Mathf.Clamp(Mathf.Min(_boxSize.x, _boxSize.y) * 0.06f, 0.06f, 0.14f);
            float wiggle = Mathf.Clamp(_boxSize.x * 0.03f, 0.03f, 0.10f);

            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);

                // Ease up then down.
                float up = u < 0.35f
                    ? MotionUtil.EaseOutCubic(u / 0.35f)
                    : 1f - MotionUtil.EaseOutCubic((u - 0.35f) / 0.65f);
                float y = lift * up;

                // Small horizontal wiggle while lifted.
                float s = Mathf.Sin(u * Mathf.PI * 10f) * wiggle * up;
                transform.localPosition = basePos + new Vector3(s, y, 0f);
                yield return null;
            }

            transform.localPosition = basePos;
        }

        private static void EnsureMouthFlashTexture()
        {
            if (_mouthFlashTexture != null) return;

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false, linear: true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float cx = (size - 1) * 0.5f;
            float cy = (size - 1) * 0.5f;
            float inv = 1f / Mathf.Max(1f, cx);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) * inv;
                    float dy = (y - cy) * inv;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    // Soft radial falloff: center strong, edges fade out smoothly.
                    float a = Mathf.Clamp01(1f - r);
                    a = a * a * a; // cubic
                    // Add a subtle hot core so it reads as a glow rather than a flat blob.
                    float core = Mathf.Clamp01(1f - r * 2.2f);
                    core = core * core;
                    float alpha = Mathf.Clamp01(a * 0.85f + core * 0.25f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            _mouthFlashTexture = tex;
        }

        private void EnsureMouthFlash()
        {
            if (_mouthFlash != null) return;

            EnsureMouthFlashTexture();

            _mouthFlash = RuntimePrimitives.CreateQuad("MouthFlash");
            _mouthFlash.name = "MouthFlash";
            _mouthFlash.transform.SetParent(transform, false);
            RemoveCollider(_mouthFlash);

            var shader =
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Particles/Additive") ??
                Shader.Find("Legacy Shaders/Particles/Additive") ??
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Unlit/Texture") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");

            if (shader != null)
            {
                var mat = new Material(shader);
                if (_mouthFlashTexture != null)
                {
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", _mouthFlashTexture);
                    else mat.mainTexture = _mouthFlashTexture;
                }
                if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 2950;

                var r = _mouthFlash.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = mat;
            }

            _mouthFlash.SetActive(false);
        }

        private void EnsureMouthIndicator()
        {
            EnsureMouthFlashTexture();

            if (_mouthIndicator == null)
            {
                _mouthIndicator = RuntimePrimitives.CreateQuad("MouthIndicator");
                _mouthIndicator.name = "MouthIndicator";
                _mouthIndicator.transform.SetParent(transform, false);
                RemoveCollider(_mouthIndicator);

                var shader =
                    Shader.Find("Unlit/Transparent") ??
                    Shader.Find("Unlit/Texture") ??
                    Shader.Find("Particles/Additive") ??
                    Shader.Find("Legacy Shaders/Particles/Additive") ??
                    Shader.Find("Unlit/Color") ??
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("UI/Default") ??
                    Shader.Find("Standard");

                if (shader != null)
                {
                    var mat = new Material(shader);
                    if (_mouthFlashTexture != null)
                    {
                        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", _mouthFlashTexture);
                        else mat.mainTexture = _mouthFlashTexture;
                    }
                    if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = 2915; // above blocks, below most overlays

                    var r = _mouthIndicator.GetComponent<Renderer>();
                    if (r != null) r.sharedMaterial = mat;
                }
            }

            // Placement/scale: a subtle "portal" at the box opening so players know where the entry check happens.
            if (_mouthIndicator != null)
            {
                _mouthIndicator.transform.localPosition = _mouthLocalPos + _mouthLocalNormal * 0.06f + new Vector3(0f, 0f, -0.06f);
                _mouthIndicator.transform.localRotation = Quaternion.identity;

                float edge = (_opening == OpeningSide.Left || _opening == OpeningSide.Right) ? _boxSize.y : _boxSize.x;
                float width = Mathf.Clamp(edge * 0.52f, 0.18f, 1.35f);
                float thickness = Mathf.Clamp(Mathf.Min(_boxSize.x, _boxSize.y) * 0.10f, 0.06f, 0.18f);

                float sx = (_opening == OpeningSide.Left || _opening == OpeningSide.Right) ? thickness : width;
                float sy = (_opening == OpeningSide.Left || _opening == OpeningSide.Right) ? width : thickness;
                _mouthIndicator.transform.localScale = new Vector3(sx, sy, 1f);
                _mouthIndicatorBaseScale = _mouthIndicator.transform.localScale;

                var r = _mouthIndicator.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    var c = Color.white;
                    c.a = 0.14f;
                    TrySetMaterialColor(r.sharedMaterial, c);
                }
            }

            UpdateMouthIndicatorVisibility();
        }

        private void UpdateMouthIndicatorVisibility()
        {
            if (_mouthIndicator == null) return;
            _mouthIndicator.SetActive(!_locked && !_completed);
        }

        public Vector3 GetMouthWorldPosition()
        {
            return transform.TransformPoint(_mouthLocalPos);
        }

        public Vector3 GetMouthWorldNormal()
        {
            return transform.TransformDirection(_mouthLocalNormal).normalized;
        }

        private void EnsureMouthRipple()
        {
            if (_mouthRipple != null) return;

            EnsureMouthFlashTexture();

            _mouthRipple = RuntimePrimitives.CreateQuad("MouthRipple");
            _mouthRipple.name = "MouthRipple";
            _mouthRipple.transform.SetParent(transform, false);
            RemoveCollider(_mouthRipple);

            var shader =
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Unlit/Texture") ??
                Shader.Find("Particles/Additive") ??
                Shader.Find("Legacy Shaders/Particles/Additive") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");

            if (shader != null)
            {
                var mat = new Material(shader);
                if (_mouthFlashTexture != null)
                {
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", _mouthFlashTexture);
                    else mat.mainTexture = _mouthFlashTexture;
                }
                if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 2925;

                var r = _mouthRipple.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = mat;
            }

            _mouthRipple.SetActive(false);
        }

        private IEnumerator AnimateMouthRipple(Color color, float seconds)
        {
            if (_mouthRipple == null) yield break;
            seconds = Mathf.Clamp(seconds, 0.10f, 0.22f);

            float edge = (_opening == OpeningSide.Left || _opening == OpeningSide.Right) ? _boxSize.y : _boxSize.x;
            float width = Mathf.Clamp(edge * 0.58f, 0.20f, 1.45f);
            float thickness = Mathf.Clamp(Mathf.Min(_boxSize.x, _boxSize.y) * 0.16f, 0.08f, 0.26f);
            float sx = (_opening == OpeningSide.Left || _opening == OpeningSide.Right) ? thickness : width;
            float sy = (_opening == OpeningSide.Left || _opening == OpeningSide.Right) ? width : thickness;

            var r = _mouthRipple.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != null)
            {
                var c = color;
                c.a = 0f;
                TrySetMaterialColor(r.sharedMaterial, c);
            }

            _mouthRipple.transform.localPosition = _mouthLocalPos + _mouthLocalNormal * 0.06f + new Vector3(0f, 0f, -0.07f);
            _mouthRipple.transform.localRotation = Quaternion.identity;
            _mouthRipple.transform.localScale = new Vector3(sx * 0.75f, sy * 0.75f, 1f);
            _mouthRipple.SetActive(true);

            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float e = MotionUtil.EaseOutCubic(u);
                float fade = 1f - MotionUtil.EaseOutCubic(u);

                float s = Mathf.Lerp(0.75f, 1.25f, e);
                _mouthRipple.transform.localScale = new Vector3(sx * s, sy * s, 1f);

                if (r != null && r.sharedMaterial != null)
                {
                    var c = color;
                    c.a = 0.55f * fade * Mathf.Clamp01(color.a);
                    TrySetMaterialColor(r.sharedMaterial, c);
                }

                yield return null;
            }

            _mouthRipple.SetActive(false);
        }

        private IEnumerator AnimateMouthSquash(Color color, float seconds)
        {
            if (_mouthIndicator == null) yield break;
            seconds = Mathf.Clamp(seconds, 0.10f, 0.22f);

            var r = _mouthIndicator.GetComponent<Renderer>();
            Color baseColor = Color.white;
            if (r != null && r.sharedMaterial != null) baseColor = GetMaterialColor(r.sharedMaterial, Color.white);

            // Thickness axis depends on opening orientation (indicator is a strip across the mouth).
            bool leftRight = _opening == OpeningSide.Left || _opening == OpeningSide.Right;
            float t0 = leftRight ? _mouthIndicatorBaseScale.x : _mouthIndicatorBaseScale.y;
            float w0 = leftRight ? _mouthIndicatorBaseScale.y : _mouthIndicatorBaseScale.x;

            float tMin = t0 * 0.55f;
            float tMax = t0 * 1.18f;

            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);

                // 0..0.35: squash; 0.35..0.7: overshoot; 0.7..1: settle
                float thickness;
                if (u < 0.35f)
                {
                    float a = MotionUtil.EaseOutCubic(u / 0.35f);
                    thickness = Mathf.Lerp(t0, tMin, a);
                }
                else if (u < 0.7f)
                {
                    float a = MotionUtil.EaseOutCubic((u - 0.35f) / 0.35f);
                    thickness = Mathf.Lerp(tMin, tMax, a);
                }
                else
                {
                    float a = MotionUtil.EaseOutCubic((u - 0.7f) / 0.3f);
                    thickness = Mathf.Lerp(tMax, t0, a);
                }

                if (leftRight)
                {
                    _mouthIndicator.transform.localScale = new Vector3(thickness, w0, 1f);
                }
                else
                {
                    _mouthIndicator.transform.localScale = new Vector3(w0, thickness, 1f);
                }

                if (r != null && r.sharedMaterial != null)
                {
                    var c = baseColor;
                    // Slight brighten during impact; use input alpha as intensity gate.
                    float impact = 1f - Mathf.Abs(u - 0.35f) / 0.35f;
                    impact = Mathf.Clamp01(impact);
                    c.a = Mathf.Lerp(baseColor.a, 0.26f, impact) * Mathf.Clamp01(color.a);
                    TrySetMaterialColor(r.sharedMaterial, c);
                }

                yield return null;
            }

            // Restore.
            _mouthIndicator.transform.localScale = _mouthIndicatorBaseScale;
            if (r != null && r.sharedMaterial != null) TrySetMaterialColor(r.sharedMaterial, baseColor);
        }

        private void PlayMouthFlash(Color color, float sizeFactor, float seconds)
        {
            EnsureMouthFlash();
            if (_mouthFlash == null) return;
            if (_mouthFlashRoutine != null) StopCoroutine(_mouthFlashRoutine);
            _mouthFlashRoutine = StartCoroutine(AnimateMouthFlash(color, sizeFactor, seconds));
        }

        private IEnumerator AnimateMouthFlash(Color color, float sizeFactor, float seconds)
        {
            if (_mouthFlash == null) yield break;
            seconds = Mathf.Max(0.05f, seconds);

            _mouthFlash.SetActive(true);

            float baseSize = Mathf.Min(_boxSize.x, _boxSize.y) * 0.38f;
            float s0 = Mathf.Max(0.08f, baseSize * 0.35f);
            float s1 = Mathf.Max(0.10f, baseSize * sizeFactor);

            _mouthFlash.transform.localPosition = _mouthLocalPos + _mouthLocalNormal * 0.03f + new Vector3(0f, 0f, -0.10f);
            _mouthFlash.transform.localRotation = Quaternion.identity;

            var r = _mouthFlash.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != null)
            {
                var c = color;
                c.a = 0f;
                TrySetMaterialColor(r.sharedMaterial, c);
            }

            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float pop = MotionUtil.EaseOutCubic(u);
                float fade = 1f - MotionUtil.EaseOutCubic(u);
                float s = Mathf.Lerp(s0, s1, pop);
                _mouthFlash.transform.localScale = new Vector3(s, s, 1f);
                if (r != null && r.sharedMaterial != null)
                {
                    var c = color;
                    c.a = (0.7f * fade) * Mathf.Clamp01(color.a);
                    TrySetMaterialColor(r.sharedMaterial, c);
                }
                yield return null;
            }

            _mouthFlash.SetActive(false);
        }

        private void SetLockVisualAlpha(float a)
        {
            SetQuadAlpha(_lockOverlay, a * LockOverlayBaseAlpha);
            SetQuadAlpha(_lockMarkerPlate, a);
            SetQuadAlpha(_lockMarkerColorHalo, a);
            SetQuadAlpha(_lockMarkerDisc, a);
            SetQuadAlpha(_lockMarkerIcon, a);
        }

        private static void SetQuadAlpha(GameObject go, float a)
        {
            if (go == null) return;
            a = Mathf.Clamp01(a);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var spriteColor = sr.color;
                spriteColor.a = a;
                sr.color = spriteColor;
                return;
            }
            var r = go.GetComponent<Renderer>();
            if (r == null || r.sharedMaterial == null) return;
            if (TrySetMaterialAlpha(r.sharedMaterial, a))
            {
                if (!r.enabled) r.enabled = true;
                return;
            }

            // Some built-in shaders (e.g. Unlit/Transparent) don't expose a color property.
            // Fall back to toggling renderer visibility (no smooth fade, but avoids errors).
            r.enabled = a > 0.001f;
        }

        private static bool TrySetMaterialAlpha(Material mat, float a)
        {
            if (mat == null) return false;

            // Common property names across pipelines/shaders.
            if (mat.HasProperty("_Color"))
            {
                var c = mat.GetColor("_Color");
                c.a = a;
                mat.SetColor("_Color", c);
                return true;
            }
            if (mat.HasProperty("_BaseColor"))
            {
                var c = mat.GetColor("_BaseColor");
                c.a = a;
                mat.SetColor("_BaseColor", c);
                return true;
            }
            if (mat.HasProperty("_TintColor"))
            {
                var c = mat.GetColor("_TintColor");
                c.a = a;
                mat.SetColor("_TintColor", c);
                return true;
            }

            return false;
        }

        private static bool TrySetMaterialColor(Material mat, Color color)
        {
            if (mat == null) return false;

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
                return true;
            }
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
                return true;
            }
            if (mat.HasProperty("_TintColor"))
            {
                mat.SetColor("_TintColor", color);
                return true;
            }

            return false;
        }

        private static Color GetMaterialColor(Material mat, Color fallback)
        {
            if (mat == null) return fallback;

            if (mat.HasProperty("_Color"))
            {
                return mat.GetColor("_Color");
            }
            if (mat.HasProperty("_BaseColor"))
            {
                return mat.GetColor("_BaseColor");
            }
            if (mat.HasProperty("_TintColor"))
            {
                return mat.GetColor("_TintColor");
            }

            return fallback;
        }

        private IEnumerator AnimateLockAlpha(float from, float to, float seconds)
        {
            if (_lockBadge != null) _lockBadge.transform.localScale = Vector3.one * 0.92f;
            float t = 0f;
            seconds = Mathf.Max(0.05f, seconds);
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float e = MotionUtil.EaseOutCubic(u);
                float a = Mathf.Lerp(from, to, e);
                SetLockVisualAlpha(a);
                if (_lockBadge != null)
                {
                    float s = Mathf.Lerp(0.92f, 1f, MotionUtil.EaseOutBack(u));
                    _lockBadge.transform.localScale = Vector3.one * s;
                }
                yield return null;
            }
            SetLockVisualAlpha(to);
            if (_lockBadge != null) _lockBadge.transform.localScale = Vector3.one;
        }

        private IEnumerator AnimateUnlock(float from, float to, float seconds)
        {
            float t = 0f;
            seconds = Mathf.Max(0.05f, seconds);
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float e = MotionUtil.EaseOutCubic(u);
                float a = Mathf.Lerp(from, to, e);
                SetLockVisualAlpha(a);
                if (_lockBadge != null)
                {
                    float s = Mathf.Lerp(1f, 0.9f, e);
                    _lockBadge.transform.localScale = Vector3.one * s;
                }
                yield return null;
            }

            SetLockVisualAlpha(to);
            if (_lockOverlay != null) _lockOverlay.SetActive(false);
            if (_lockBadge != null) _lockBadge.SetActive(false);
            if (_lockBadge != null) _lockBadge.transform.localScale = Vector3.one;
        }

        private static void EnsureUnlitColorMaterial(GameObject quad, Color color, int renderQueue)
        {
            if (quad == null) return;
            var r = quad.GetComponent<Renderer>();
            if (r == null) return;

            // If a textured material is already assigned, keep it (this function is meant as a fallback to avoid magenta).
            if (r.sharedMaterial != null && r.sharedMaterial.mainTexture != null)
            {
                var tex = r.sharedMaterial.mainTexture;
                if (TrySetMaterialColor(r.sharedMaterial, color))
                {
                    r.sharedMaterial.renderQueue = renderQueue;
                    if (r.sharedMaterial.HasProperty("_ZWrite")) r.sharedMaterial.SetInt("_ZWrite", 0);
                    if (r.sharedMaterial.HasProperty("_ZTest")) r.sharedMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                    if (r.sharedMaterial.HasProperty("_Cull")) r.sharedMaterial.SetInt("_Cull", 0);
                    if (r.sharedMaterial.HasProperty("_CullMode")) r.sharedMaterial.SetInt("_CullMode", 0);
                    return;
                }

                // Material is textured but not tintable (e.g. Unlit/Transparent). Replace with a tintable shader.
                var tintShader =
                    Shader.Find("LoopSorting/UnlitTexture") ??
                    Shader.Find("Unlit/Transparent Colored") ??
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("Unlit/Texture") ??
                    Shader.Find("UI/Default") ??
                    Shader.Find("Standard");
                if (tintShader == null) return;

                var tintMat = new Material(tintShader);
                if (tintMat.HasProperty("_MainTex")) tintMat.SetTexture("_MainTex", tex);
                else tintMat.mainTexture = tex;
                TrySetMaterialColor(tintMat, color);
                tintMat.renderQueue = renderQueue;
                if (tintMat.HasProperty("_ZWrite")) tintMat.SetInt("_ZWrite", 0);
                if (tintMat.HasProperty("_ZTest")) tintMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                if (tintMat.HasProperty("_Cull")) tintMat.SetInt("_Cull", 0);
                if (tintMat.HasProperty("_CullMode")) tintMat.SetInt("_CullMode", 0);
                r.sharedMaterial = tintMat;
                return;
            }

            // Primitives come with Unity's shared default material; always replace it so per-box tinting doesn't leak across instances.
            if (r.sharedMaterial != null &&
                r.sharedMaterial.shader != null &&
                r.sharedMaterial.shader.name == "Unlit/Color" &&
                r.sharedMaterial.renderQueue == renderQueue)
            {
                TrySetMaterialColor(r.sharedMaterial, color);
                if (r.sharedMaterial.HasProperty("_ZWrite")) r.sharedMaterial.SetInt("_ZWrite", 0);
                if (r.sharedMaterial.HasProperty("_ZTest")) r.sharedMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                if (r.sharedMaterial.HasProperty("_Cull")) r.sharedMaterial.SetInt("_Cull", 0);
                if (r.sharedMaterial.HasProperty("_CullMode")) r.sharedMaterial.SetInt("_CullMode", 0);
                return;
            }

            var shader =
                Shader.Find("LoopSorting/UnlitTexture") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");
            if (shader == null) return;

            var mat = new Material(shader);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", Texture2D.whiteTexture);
            else mat.mainTexture = Texture2D.whiteTexture;
            TrySetMaterialColor(mat, color);
            mat.renderQueue = renderQueue;
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
            if (mat.HasProperty("_ZTest")) mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", 0);
            if (mat.HasProperty("_CullMode")) mat.SetInt("_CullMode", 0);
            r.sharedMaterial = mat;
        }

        private static void SetWorldOverlaySorting(GameObject go, int sortingOrder)
        {
            if (go == null) return;
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            r.sortingLayerID = 0;
            r.sortingOrder = sortingOrder;
        }

        private static Texture2D TryLoadBlockColorTexture(BlockColor color)
        {
            string file;
            switch (color)
            {
                case BlockColor.Red:
                    file = "block_red.png";
                    break;
                case BlockColor.Blue:
                    file = "block_blue.png";
                    break;
                case BlockColor.Yellow:
                    file = "block_yellow.png";
                    break;
                case BlockColor.Green:
                    file = "block_green.png";
                    break;
                case BlockColor.Purple:
                    file = "block_purple.png";
                    break;
                case BlockColor.Orange:
                    file = "block_orange.png";
                    break;
                default:
                    file = "block_red.png";
                    break;
            }

            return LoopSortingUIKit.LoadTexture("Blocks/" + file);
        }

        public void SetCompleted(bool val)
        {
            bool wasCompleted = _completed;
            _completed = val;

            // Once a box is completed, the dashed operable outline is no longer needed.
            SetBoxOutlineVisible(!val);
            if (val) HideFrontOutline();
            UpdateMouthIndicatorVisibility();

            EnsureCompletedOverlayBuilt();
            if (_completedOverlay == null) return;

            if (!val)
            {
                if (wasCompleted && _completedFxRoutine != null) StopCoroutine(_completedFxRoutine);
                _completedFxRoutine = null;
                if (_completedBurst != null)
                {
                    Destroy(_completedBurst);
                    _completedBurst = null;
                }
                if (_completedConfetti != null)
                {
                    Destroy(_completedConfetti);
                    _completedConfetti = null;
                }
                _completedOverlay.SetActive(false);
                return;
            }

            _completedOverlay.SetActive(true);
            UpdateCompletedOverlayVisuals();

            if (!wasCompleted)
            {
                // Start from invisible + slightly squashed so the overlay feels like a "cap" settling on the box.
                bool useCap = _completedFrameGlow != null && _completedFrameGlow.GetComponent<SpriteRenderer>() != null;
                if (useCap)
                {
                    SetCompletedOverlayFade(0f);
                    _completedOverlay.transform.localScale = Vector3.one * 0.92f;
                }

                if (_completedFxRoutine != null) StopCoroutine(_completedFxRoutine);
                _completedFxRoutine = StartCoroutine(PlayCompletedFx());
            }
        }

        private void SetBoxOutlineVisible(bool visible)
        {
            if (_boxOutlineSegments == null) return;
            for (int i = 0; i < _boxOutlineSegments.Count; i++)
            {
                var seg = _boxOutlineSegments[i];
                if (seg == null) continue;
                if (seg.gameObject != null) seg.gameObject.SetActive(visible);
            }
        }

        private void EnsureCompletedOverlayBuilt()
        {
            if (_completedOverlay != null) return;

            _completedOverlay = new GameObject("CompletedOverlay");
            _completedOverlay.transform.SetParent(transform, false);
            _completedOverlay.transform.localPosition = new Vector3(0f, 0f, CompletedOverlayZ); // above blocks, below lock
            _completedOverlay.transform.localRotation = Quaternion.identity;
            _completedOverlay.transform.localScale = Vector3.one;

            bool hasKit = LoopSortingUIKit.IsAvailable();
            if (hasKit)
            {
                _completedFrameGlow = CreateSlicedSpriteLayer(
                    parent: _completedOverlay.transform,
                    name: "Cap",
                    z: 0f,
                    spritePath: "World_Sprites/completed_overlay.png",
                    pixelsPerUnit: 500f,
                    sortingOrder: CompletedQueue,
                    renderQueue: CompletedQueue);

                _completedGlass = CreateSlicedSpriteLayer(
                    parent: _completedOverlay.transform,
                    name: "Glass",
                    z: 0.01f,
                    spritePath: "World_Sprites/box_completed_glass_overlay_512.png",
                    pixelsPerUnit: 500f,
                    sortingOrder: CompletedQueue + 1,
                    renderQueue: CompletedQueue + 1);
            }
            else
            {
                _completedFrameGlow = CreateNineSliceLayer(_completedOverlay.transform, "FrameGlow", z: 0f);
                _completedGlass = CreateNineSliceLayer(_completedOverlay.transform, "Glass", z: 0.01f);
            }

            _completedBadge = RuntimePrimitives.CreateQuad("CompletedBadge");
            _completedBadge.name = "Badge";
            _completedBadge.transform.SetParent(_completedOverlay.transform, false);
            _completedBadge.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            RemoveCollider(_completedBadge);

            // Textures live under the active UI kit resources root.
            var badgeTex = LoopSortingUIKit.LoadTexture("World_Sprites/box_completed_badge_check_256.png");
            if (badgeTex != null)
            {
                var r = _completedBadge.GetComponent<Renderer>();
                if (r != null)
                {
                    var mat = LoopSortingUIKit.CreateUnlitTextureMaterial(badgeTex, Color.white, CompletedQueue + 2);
                    if (mat != null) r.sharedMaterial = mat;
                }
            }

            // Fallback so we never show magenta (mesh path).
            if (!hasKit)
            {
                var frameTex = LoopSortingUIKit.LoadTexture("World_Sprites/box_completed_frame_glow_512.png");
                var glassTex = LoopSortingUIKit.LoadTexture("World_Sprites/box_completed_glass_overlay_512.png");

                if (frameTex != null)
                {
                    var r = _completedFrameGlow.GetComponent<Renderer>();
                    if (r != null)
                    {
                        var mat = LoopSortingUIKit.CreateUnlitTextureMaterial(frameTex, Color.white, CompletedQueue);
                        if (mat != null) r.sharedMaterial = mat;
                    }
                }
                if (glassTex != null)
                {
                    var r = _completedGlass.GetComponent<Renderer>();
                    if (r != null)
                    {
                        var mat = LoopSortingUIKit.CreateUnlitTextureMaterial(glassTex, Color.white, CompletedQueue + 1);
                        if (mat != null) r.sharedMaterial = mat;
                    }
                }

                EnsureUnlitColorMaterial(_completedFrameGlow, new Color(1f, 1f, 1f, 0.35f), CompletedQueue);
                EnsureUnlitColorMaterial(_completedGlass, new Color(1f, 1f, 1f, 0.25f), CompletedQueue + 1);
            }

            EnsureUnlitColorMaterial(_completedBadge, new Color(1f, 1f, 1f, 1f), CompletedQueue + 2);

            _completedOverlay.SetActive(false);
        }

        private void SetCompletedOverlayFade(float fade01)
        {
            fade01 = Mathf.Clamp01(fade01);
            SetQuadAlpha(_completedFrameGlow, CompletedCapAlpha * fade01);
            SetQuadAlpha(_completedGlass, CompletedGlassAlpha * fade01);
        }

        private void UpdateCompletedOverlayVisuals()
        {
            var boxTint = GetCompletedTintColor();

            void FitLayer(GameObject go, float w, float h, float borderFrac)
            {
                if (go == null) return;
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    if (sr.drawMode == SpriteDrawMode.Sliced)
                    {
                        sr.size = new Vector2(w, h);
                        go.transform.localScale = Vector3.one;
                        return;
                    }

                    var b = sr.sprite.bounds;
                    float sx = b.size.x > 0.0001f ? w / b.size.x : 1f;
                    float sy = b.size.y > 0.0001f ? h / b.size.y : 1f;
                    go.transform.localScale = new Vector3(sx, sy, 1f);
                    return;
                }

                UpdateNineSliceMesh(go, w, h, borderFrac);
            }

            // Keep the overlay tightly matched to the box size (avoid "stretched mask" look).
            float capW = _boxSize.x * CompletedCapScale;
            float capH = _boxSize.y * CompletedCapScale;
            FitLayer(_completedFrameGlow, capW, capH, CompletedNineSliceBorderFrac);

            float glassW = _boxSize.x * CompletedGlassScale;
            float glassH = _boxSize.y * CompletedGlassScale;
            FitLayer(_completedGlass, glassW, glassH, CompletedNineSliceBorderFrac);

            // For very wide/tall boxes, use the 1024 textures to keep edge quality when stretched.
            bool useHiRes = ShouldUseHiResCompletedTextures();
            if (_completedFrameGlow != null)
            {
                var sr = _completedFrameGlow.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    // Cap uses a single authored sprite (no hi-res variant).
                }
                else
                {
                    var r = _completedFrameGlow.GetComponent<Renderer>();
                    if (r != null && r.sharedMaterial != null)
                    {
                        var tex = LoopSortingUIKit.LoadTexture(useHiRes
                            ? "World_Sprites/box_completed_frame_glow_1024.png"
                            : "World_Sprites/box_completed_frame_glow_512.png");
                        if (tex != null) r.sharedMaterial.mainTexture = tex;
                    }
                }
            }

            if (_completedGlass != null)
            {
                var sr = _completedGlass.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    // Sprite renderer path: optionally swap to hi-res sprite if present.
                    var sprite = LoopSortingUIKit.LoadSprite(useHiRes
                        ? "World_Sprites/box_completed_glass_overlay_1024.png"
                        : "World_Sprites/box_completed_glass_overlay_512.png", pixelsPerUnit: 500f, applyNineSlice: true);
                    if (sprite != null) sr.sprite = sprite;
                }
                else
                {
                    var r = _completedGlass.GetComponent<Renderer>();
                    if (r != null && r.sharedMaterial != null)
                    {
                        var tex = LoopSortingUIKit.LoadTexture(useHiRes
                            ? "World_Sprites/box_completed_glass_overlay_1024.png"
                            : "World_Sprites/box_completed_glass_overlay_512.png");
                        if (tex != null) r.sharedMaterial.mainTexture = tex;
                    }
                }
            }

            float badgeBase = Mathf.Min(_boxSize.x, _boxSize.y);
            float badgeSize = badgeBase * 0.28f;
            if (_completedBadge != null)
            {
                _completedBadge.transform.localScale = new Vector3(badgeSize, badgeSize, 1f);
                _completedBadge.transform.localPosition = new Vector3(_boxSize.x * 0.54f, _boxSize.y * 0.54f, 0.02f);
            }

            // Tint: cap is subtly tinted, glass is semi-transparent, badge stays mostly white.
            if (_completedFrameGlow != null)
            {
                var sr = _completedFrameGlow.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    var c = Color.Lerp(Color.white, boxTint, 0.22f);
                    c.a = CompletedCapAlpha;
                    sr.color = c;
                }
                else
                {
                    var r = _completedFrameGlow.GetComponent<Renderer>();
                    if (r != null && r.sharedMaterial != null)
                    {
                        var c = boxTint * 0.78f;
                        c.a = 0.5f;
                        TrySetMaterialColor(r.sharedMaterial, c);
                    }
                }
            }
            if (_completedGlass != null)
            {
                var sr = _completedGlass.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    var c = Color.white;
                    c.a = CompletedGlassAlpha;
                    sr.color = c;
                }
                else
                {
                    var r = _completedGlass.GetComponent<Renderer>();
                    if (r != null && r.sharedMaterial != null)
                    {
                        var c = Color.white;
                        c.a = 0.22f;
                        TrySetMaterialColor(r.sharedMaterial, c);
                    }
                }
            }
            if (_completedBadge != null)
            {
                var r = _completedBadge.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    var c = Color.white;
                    c.a = CompletedBadgeAlpha;
                    TrySetMaterialColor(r.sharedMaterial, c);
                }
            }
        }

        private static GameObject CreateSlicedSpriteLayer(
            Transform parent,
            string name,
            float z,
            string spritePath,
            float pixelsPerUnit,
            int sortingOrder,
            int renderQueue)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, z);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sr.receiveShadows = false;
            sr.color = Color.white;
            sr.sortingOrder = sortingOrder;

            var sprite = LoopSortingUIKit.LoadSprite(spritePath, pixelsPerUnit: pixelsPerUnit, applyNineSlice: true);
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.drawMode = sprite.border.sqrMagnitude > 0.0001f ? SpriteDrawMode.Sliced : SpriteDrawMode.Simple;
            }

            // Ensure deterministic ordering against other world overlays.
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.renderQueue = renderQueue;
                sr.sharedMaterial = mat;
            }

            return go;
        }

        private bool ShouldUseHiResCompletedTextures()
        {
            float maxDim = Mathf.Max(_boxSize.x, _boxSize.y);
            float minDim = Mathf.Max(0.0001f, Mathf.Min(_boxSize.x, _boxSize.y));
            float aspect = maxDim / minDim;

            // Prefer hi-res when the texture will be stretched significantly.
            return aspect >= 1.8f || maxDim >= 3.0f;
        }

        private static GameObject CreateNineSliceLayer(Transform parent, string name, float z)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, z);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            return go;
        }

        private static void UpdateNineSliceMesh(GameObject go, float width, float height, float borderFrac)
        {
            if (go == null) return;
            var mf = go.GetComponent<MeshFilter>();
            if (mf == null) return;

            width = Mathf.Max(0.001f, width);
            height = Mathf.Max(0.001f, height);

            // Keep corners square by basing corner size on the smaller dimension.
            float corner = Mathf.Min(width, height) * Mathf.Clamp01(borderFrac);
            float hw = width * 0.5f;
            float hh = height * 0.5f;
            corner = Mathf.Min(corner, hw, hh);

            float x0 = -hw;
            float x1 = -hw + corner;
            float x2 = hw - corner;
            float x3 = hw;

            float y0 = -hh;
            float y1 = -hh + corner;
            float y2 = hh - corner;
            float y3 = hh;

            float u0 = 0f;
            float u1 = borderFrac;
            float u2 = 1f - borderFrac;
            float u3 = 1f;

            float v0 = 0f;
            float v1 = borderFrac;
            float v2 = 1f - borderFrac;
            float v3 = 1f;

            var mesh = mf.sharedMesh;
            if (mesh == null)
            {
                mesh = new Mesh { name = $"{go.name}_NineSlice" };
                mf.sharedMesh = mesh;
            }
            else
            {
                mesh.Clear();
            }

            // 4x4 grid => 16 verts.
            var verts = new Vector3[16];
            var uvs = new Vector2[16];
            float[] xs = { x0, x1, x2, x3 };
            float[] ys = { y0, y1, y2, y3 };
            float[] us = { u0, u1, u2, u3 };
            float[] vs = { v0, v1, v2, v3 };

            int vi = 0;
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    verts[vi] = new Vector3(xs[x], ys[y], 0f);
                    uvs[vi] = new Vector2(us[x], vs[y]);
                    vi++;
                }
            }

            // 3x3 cells => 9 quads => 18 tris => 54 indices.
            var indices = new int[54];
            int ti = 0;
            for (int cy = 0; cy < 3; cy++)
            {
                for (int cx = 0; cx < 3; cx++)
                {
                    int i00 = cy * 4 + cx;
                    int i10 = cy * 4 + (cx + 1);
                    int i01 = (cy + 1) * 4 + cx;
                    int i11 = (cy + 1) * 4 + (cx + 1);

                    indices[ti++] = i00;
                    indices[ti++] = i01;
                    indices[ti++] = i11;

                    indices[ti++] = i00;
                    indices[ti++] = i11;
                    indices[ti++] = i10;
                }
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
        }

        private Color GetCompletedTintColor()
        {
            for (int i = 0; i < _slotColors.Count; i++)
            {
                if (_slotColors[i].HasValue)
                {
                    var c = BlockVisual.ToUnityColor(_slotColors[i].Value);
                    c.a = 1f;
                    return c;
                }
            }
            return Color.white;
        }

        private IEnumerator PlayCompletedFx()
        {
            bool useCap = _completedFrameGlow != null && _completedFrameGlow.GetComponent<SpriteRenderer>() != null;

            // Badge pop: 0 -> 1.08 -> 1.0 in ~0.22s.
            float t = 0f;
            float dur = 0.22f;
            if (_completedBadge != null)
            {
                _completedBadge.transform.localScale = Vector3.zero;
            }

            // Cap/glass pop-in first (gives a more 3D "cap settling" feel).
            if (useCap && _completedOverlay != null)
            {
                float popT = 0f;
                float popDur = Mathf.Max(0.05f, CompletedOverlayPopSeconds);
                while (popT < popDur)
                {
                    popT += Time.deltaTime;
                    float u = Mathf.Clamp01(popT / popDur);
                    float e = MotionUtil.EaseOutCubic(u);
                    SetCompletedOverlayFade(e);
                    float s = Mathf.Lerp(0.92f, 1f, MotionUtil.EaseOutBack(u));
                    _completedOverlay.transform.localScale = Vector3.one * s;
                    yield return null;
                }

                SetCompletedOverlayFade(1f);
                _completedOverlay.transform.localScale = Vector3.one;
            }

            // Confetti-only (more natural, avoids sprite-sheet square edge artifacts).
            CreateCompletedConfettiFx();

            while (t < dur)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / dur);
                float s = u < 0.7f ? Mathf.Lerp(0f, 1.08f, u / 0.7f) : Mathf.Lerp(1.08f, 1f, (u - 0.7f) / 0.3f);
                if (_completedBadge != null)
                {
                    float badgeBase = Mathf.Min(_boxSize.x, _boxSize.y);
                    float badgeSize = badgeBase * 0.34f;
                    _completedBadge.transform.localScale = new Vector3(badgeSize * s, badgeSize * s, 1f);
                }
                yield return null;
            }

            // Let confetti fall for a bit before cleanup.
            yield return new WaitForSeconds(1.65f);
            if (_completedBurst != null)
            {
                Destroy(_completedBurst);
                _completedBurst = null;
            }
            if (_completedConfetti != null)
            {
                Destroy(_completedConfetti);
                _completedConfetti = null;
            }
        }

        private void CreateCompletedConfettiFx()
        {
            if (_completedOverlay == null) return;

            if (_completedBurst != null) Destroy(_completedBurst);
            if (_completedConfetti != null) Destroy(_completedConfetti);
            _completedBurst = null;
            _completedConfetti = null;

            if (!LoopSortingUIKit.IsAvailable())
            {
                return;
            }

            CreateConfettiBurst();
        }

        private void CreateConfettiBurst()
        {
            if (_completedOverlay == null) return;

            var texRect = LoopSortingUIKit.LoadTexture("World_Sprites/vfx_confetti_rect_256.png");
            var texTri = LoopSortingUIKit.LoadTexture("World_Sprites/vfx_confetti_tri_256.png");
            var texStream = LoopSortingUIKit.LoadTexture("World_Sprites/vfx_confetti_stream_256.png");
            var texStar = LoopSortingUIKit.LoadTexture("World_Sprites/vfx_confetti_star_256.png");

            var textures = new List<Texture2D>(4);
            if (texRect != null) textures.Add(texRect);
            if (texTri != null) textures.Add(texTri);
            if (texStream != null) textures.Add(texStream);
            if (texStar != null) textures.Add(texStar);
            if (textures.Count == 0) return;

            _completedConfetti = new GameObject("Confetti");
            _completedConfetti.transform.SetParent(_completedOverlay.transform, false);
            _completedConfetti.transform.localPosition = new Vector3(0f, 0f, -0.04f);
            _completedConfetti.transform.localRotation = Quaternion.identity;
            _completedConfetti.transform.localScale = Vector3.one;

            var palette = BuildConfettiPalette(GetCompletedTintColor());

            int totalCount = 28;
            int perSystem = Mathf.Max(6, Mathf.RoundToInt((float)totalCount / textures.Count));

            float baseSize = Mathf.Min(_boxSize.x, _boxSize.y);
            float startSize = Mathf.Clamp(baseSize * 0.10f, 0.06f, 0.22f) * 3.0f;
            float startSpeed = Mathf.Clamp(baseSize * 4.8f, 3.0f, 10.5f);

            for (int i = 0; i < textures.Count; i++)
            {
                var c0 = palette[i % palette.Length];
                var c1 = palette[(i + 1) % palette.Length];
                CreateOneConfettiSystem(_completedConfetti.transform, $"Confetti_{i}", textures[i], c0, c1, perSystem, startSize, startSpeed);
            }
        }

        private static void CreateOneConfettiSystem(
            Transform parent,
            string name,
            Texture2D texture,
            Color colorA,
            Color colorB,
            int burstCount,
            float startSize,
            float startSpeed)
        {
            if (parent == null || texture == null) return;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed * 0.55f, startSpeed * 1.05f);
            main.startSize = new ParticleSystem.MinMaxCurve(startSize * 0.7f, startSize * 1.25f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = new ParticleSystem.MinMaxCurve(1.0f, 1.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
            main.maxParticles = 128;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            // ParticleSystem cone emits along local +Z; rotate the shape so it fires upward (+Y) in our XY gameplay plane.
            shape.rotation = new Vector3(-90f, 0f, 0f);
            shape.angle = 18f;
            shape.radius = 0.10f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = BuildAlphaFadeGradient();

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-3.5f, 3.5f);

            var sizeOver = ps.sizeOverLifetime;
            sizeOver.enabled = true;
            sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.7f));

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.material = CreateAdditiveTextureMaterial(texture, Color.white, CompletedQueue + 5);

            ps.Play();
        }

        private static ParticleSystem.MinMaxGradient BuildAlphaFadeGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            return new ParticleSystem.MinMaxGradient(g);
        }

        private static Color[] BuildConfettiPalette(Color baseColor)
        {
            baseColor.a = 1f;

            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            s = Mathf.Clamp01(Mathf.Max(0.45f, s));
            v = Mathf.Clamp01(Mathf.Max(0.65f, v));

            var a = Color.HSVToRGB(h, s, Mathf.Clamp01(v * 1.15f));
            var b = Color.HSVToRGB(Mathf.Repeat(h + 0.08f, 1f), s * 0.9f, Mathf.Clamp01(v * 0.95f));
            var c = Color.HSVToRGB(Mathf.Repeat(h - 0.10f, 1f), s * 0.8f, Mathf.Clamp01(v * 1.05f));
            var d = Color.HSVToRGB(Mathf.Repeat(h + 0.18f, 1f), s * 0.75f, Mathf.Clamp01(v * 0.85f));
            a.a = b.a = c.a = d.a = 1f;
            return new[] { a, b, c, d };
        }

        private static Material CreateAdditiveTextureMaterial(Texture2D texture, Color color, int renderQueue)
        {
            var shader =
                Shader.Find("Particles/Additive") ??
                Shader.Find("Legacy Shaders/Particles/Additive") ??
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Unlit/Texture") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");

            if (shader == null) return null;

            var mat = new Material(shader);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            else mat.mainTexture = texture;
            TrySetMaterialColor(mat, color);
            mat.renderQueue = renderQueue;
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
            return mat;
        }

        private static void RemoveCollider(GameObject quad)
        {
            if (quad == null) return;
            var col = quad.GetComponent<Collider>();
            if (col != null) GameObject.Destroy(col);
        }

        private static List<Vector2Int> BuildCellOrder(int cols, int rows, OpeningSide opening)
        {
            var order = new List<Vector2Int>(cols * rows);

            switch (opening)
            {
                case OpeningSide.Top:
                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                            order.Add(new Vector2Int(c, r));
                    break;
                case OpeningSide.Bottom:
                    for (int r = rows - 1; r >= 0; r--)
                        for (int c = 0; c < cols; c++)
                            order.Add(new Vector2Int(c, r));
                    break;
                case OpeningSide.Left:
                    for (int c = 0; c < cols; c++)
                        for (int r = 0; r < rows; r++)
                            order.Add(new Vector2Int(c, r));
                    break;
                case OpeningSide.Right:
                    for (int c = cols - 1; c >= 0; c--)
                        for (int r = 0; r < rows; r++)
                            order.Add(new Vector2Int(c, r));
                    break;
            }

            return order;
        }



    }
}
