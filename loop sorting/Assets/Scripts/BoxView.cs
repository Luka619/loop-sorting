using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    [RequireComponent(typeof(BoxCollider))]
    public class BoxView : MonoBehaviour
    {
        public int ContainerIndex { get; private set; }
        public GameRuntimeController Controller { get; private set; }

        private readonly List<GameObject> _blockVisuals = new List<GameObject>();
        private readonly List<BlockColor?> _slotColors = new List<BlockColor?>();
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
        private bool _locked;
        private bool _completed;
        private GameObject _completedOverlay;
        private LineRenderer _frontOutline;
        private readonly List<LineRenderer> _boxOutlineSegments = new List<LineRenderer>();
        private readonly Dictionary<int, Coroutine> _incomingCoroutines = new Dictionary<int, Coroutine>();
        private readonly HashSet<int> _incomingAnimatingSlots = new HashSet<int>();
        private readonly Dictionary<int, Vector3> _slotFinalLocalPos = new Dictionary<int, Vector3>();
        private Vector3 _mouthLocalPos;
        private Vector3 _mouthLocalNormal;

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
        private const float OutlineZ = -0.12f;
        private const float BoxOutlineZ = -0.35f;
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
            _slotHidden.Clear();
            _incomingCoroutines.Clear();
            _incomingAnimatingSlots.Clear();
            _slotFinalLocalPos.Clear();
            CacheMouth();

            var collider = GetComponent<BoxCollider>();
            collider.size = new Vector3(size.x, size.y, 0.3f);
            collider.center = Vector3.zero;

            EnsureSlotCapacity();
            BuildBoxOutline();
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
                        if (_slotHidden[slot] != blocks[i].Hidden) { matches = false; break; }
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

            // Enforce run-based hidden logic: same-color consecutive blocks share hidden state;
            // if the run touches the outermost position, reveal the whole run.
            for (int i = 0; i < _tmpIndices.Count;)
            {
                int idx = _tmpIndices[i];
                var color = _slotColors[idx].Value;
                bool runHidden = _slotHidden[idx];
                if (i == 0)
                {
                    runHidden = false; // front run always revealed
                }

                int j = i;
                while (j < _tmpIndices.Count)
                {
                    int idx2 = _tmpIndices[j];
                    if (_slotColors[idx2].Value != color) break;
                    _slotHidden[idx2] = runHidden;
                    j++;
                }
                i = j;
            }

            RefreshVisuals();

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
                _slotHidden[i] = false;
            }

            int count = Mathf.Min(newCount, _capacity);
            int start = Mathf.Clamp(_capacity - count, 0, _capacity);
            for (int i = 0; i < count; i++)
            {
                int slot = start + i;
                if (slot < 0 || slot >= _slotColors.Count) break;
                _slotColors[slot] = blocks[i].Color;
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
                _slotHidden.Add(false);
            }
            while (_slotColors.Count > _capacity)
            {
                int lastIdx = _slotColors.Count - 1;
                CancelIncoming(lastIdx);
                _slotColors.RemoveAt(_slotColors.Count - 1);
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

            var cellSize = new Vector2(
                _boxSize.x / _columns,
                _boxSize.y / _rows
            );

            var origin = new Vector2(-_boxSize.x * 0.5f + cellSize.x * 0.5f, _boxSize.y * 0.5f - cellSize.y * 0.5f);
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
            var renderer = _blockVisuals[slotIndex].GetComponent<Renderer>();
            if (renderer != null)
            {
                bool hidden = _slotHidden[slotIndex] && slotIndex > 0;
                var matColor = hidden ? new Color(0.3f, 0.3f, 0.3f, 1f) : BlockVisual.ToUnityColor(color);
                renderer.sharedMaterial.color = matColor;
            }
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

            var cellSize = new Vector2(_boxSize.x / _columns, _boxSize.y / _rows);

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
                float px = -_boxSize.x * 0.5f + vx * cellSize.x;
                float py = _boxSize.y * 0.5f - vy * cellSize.y;
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
                _frontOutline.sharedMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = Color.black
                };
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
                var mat = new Material(Shader.Find("Unlit/Color"))
                {
                    color = Color.white,
                };
                mat.renderQueue = 3300;
                lr.sharedMaterial = mat;
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
            _locked = val;
            if (_lockOverlay == null)
            {
                _lockOverlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _lockOverlay.name = "LockOverlay";
                _lockOverlay.transform.SetParent(transform, false);
                _lockOverlay.transform.localPosition = new Vector3(0f, 0f, LockOverlayZ); // in front of blocks
                _lockOverlay.transform.localScale = new Vector3(_boxSize.x * 1.05f, _boxSize.y * 1.05f, 1f);
                var rend = _lockOverlay.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(Shader.Find("Unlit/Color"))
                    {
                        color = new Color(0.5f, 0.5f, 0.5f, 0.9f)
                    };
                    mat.renderQueue = LockOverlayQueue;
                    rend.sharedMaterial = mat;
                }
                var col = _lockOverlay.GetComponent<Collider>();
                if (col != null) GameObject.Destroy(col);

                _lockBadge = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _lockBadge.name = "LockBadge";
                _lockBadge.transform.SetParent(_lockOverlay.transform, false);
                _lockBadge.transform.localPosition = new Vector3(0f, 0f, LockBadgeZ); // above overlay
                float badgeSize = Mathf.Min(_boxSize.x, _boxSize.y) * 0.3f;
                _lockBadge.transform.localScale = new Vector3(badgeSize, badgeSize, 1f);
                var badgeRend = _lockBadge.GetComponent<Renderer>();
                if (badgeRend != null)
                {
                    var bmat = new Material(Shader.Find("Unlit/Color"));
                    bmat.renderQueue = LockBadgeQueue;
                    badgeRend.sharedMaterial = bmat;
                }
                var badgeCol = _lockBadge.GetComponent<Collider>();
                if (badgeCol != null) GameObject.Destroy(badgeCol);
            }
            if (_lockBadge != null)
            {
                var badgeR = _lockBadge.GetComponent<Renderer>();
                if (badgeR != null)
                {
                    badgeR.sharedMaterial.color = BlockVisual.ToUnityColor(unlockColor);
                }
            }
            _lockOverlay.SetActive(val);
        }

        public void SetCompleted(bool val)
        {
            _completed = val;
            if (_completedOverlay == null)
            {
                _completedOverlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _completedOverlay.name = "CompletedOverlay";
                _completedOverlay.transform.SetParent(transform, false);
                _completedOverlay.transform.localPosition = new Vector3(0f, 0f, CompletedOverlayZ); // above blocks, below lock
                _completedOverlay.transform.localScale = new Vector3(_boxSize.x * 1.02f, _boxSize.y * 1.02f, 1f);
                var rend = _completedOverlay.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(Shader.Find("Unlit/Color"))
                    {
                        color = new Color(0.2f, 0.8f, 0.2f, 0.35f)
                    };
                    mat.renderQueue = CompletedQueue;
                    rend.sharedMaterial = mat;
                }
                var col = _completedOverlay.GetComponent<Collider>();
                if (col != null) GameObject.Destroy(col);
            }
            _completedOverlay.SetActive(val);
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
