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

        // Render / layering guideline (z 越小越靠近相机):
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

            var collider = GetComponent<BoxCollider>();
            collider.size = new Vector3(size.x, size.y, 0.3f);
            collider.center = Vector3.zero;

            EnsureSlotCapacity();
            BuildBoxOutline();
        }

        public void SyncBlocks(IReadOnlyList<Block> blocks)
        {
            EnsureSlotCapacity();

            _tmpIndices.Clear();
            for (int i = 0; i < _slotColors.Count; i++)
            {
                if (_slotColors[i].HasValue) _tmpIndices.Add(i);
            }

            int oldCount = _tmpIndices.Count;
            int newCount = Mathf.Min(blocks.Count, _capacity);

            if (newCount < oldCount)
            {
                int removeCount = oldCount - newCount;
                for (int r = 0; r < removeCount && _tmpIndices.Count > 0; r++)
                {
                    int idx = _tmpIndices[0];
                    _tmpIndices.RemoveAt(0);
                    _slotColors[idx] = null;
                    _slotHidden[idx] = false;
                }
            }
            else if (newCount > oldCount)
            {
                int addCount = newCount - oldCount;
                for (int a = 0; a < addCount; a++)
                {
                    int targetSlot = FindFirstEmptyFromInner();
                    if (targetSlot < 0) break;
                    _slotColors[targetSlot] = blocks[oldCount + a].Color;
                    _slotHidden[targetSlot] = blocks[oldCount + a].Hidden;
                }
            }

            // Refresh occupied list and update colors in place (outer->inner order).
            _tmpIndices.Clear();
            for (int i = 0; i < _slotColors.Count; i++)
            {
                if (_slotColors[i].HasValue) _tmpIndices.Add(i);
            }
            int seqLen = Mathf.Min(_tmpIndices.Count, newCount);
            for (int i = 0; i < seqLen; i++)
            {
                _slotColors[_tmpIndices[i]] = blocks[i].Color;
                _slotHidden[_tmpIndices[i]] = blocks[i].Hidden;
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

            _blockVisuals[slotIndex].transform.localPosition = new Vector3(pos.x, pos.y, 0f);
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

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var cellSize = new Vector2(_boxSize.x / _columns, _boxSize.y / _rows);

            foreach (var idx in indices)
            {
                int col = idx % _columns;
                int row = idx / _columns;
                if (_cellOrder != null && _cellOrder.Count == _columns * _rows && idx < _cellOrder.Count)
                {
                    col = _cellOrder[idx].x;
                    row = _cellOrder[idx].y;
                }
                var origin = new Vector2(-_boxSize.x * 0.5f + cellSize.x * 0.5f, _boxSize.y * 0.5f - cellSize.y * 0.5f);
                var pos = origin + new Vector2(col * cellSize.x, -row * cellSize.y);
                min = Vector2.Min(min, pos);
                max = Vector2.Max(max, pos);
            }

            if (min.x > max.x || min.y > max.y)
            {
                if (_frontOutline != null) _frontOutline.gameObject.SetActive(false);
                return;
            }

            // Ensure black outline wraps blocks clearly (larger margin)
            var margin = cellSize * 0.6f;
            var bl = new Vector3(min.x - margin.x, min.y - margin.y, OutlineZ);
            var tl = new Vector3(min.x - margin.x, max.y + margin.y, OutlineZ);
            var tr = new Vector3(max.x + margin.x, max.y + margin.y, OutlineZ);
            var br = new Vector3(max.x + margin.x, min.y - margin.y, OutlineZ);

            if (_frontOutline == null)
            {
                var go = new GameObject("FrontOutline");
                go.transform.SetParent(transform, false);
                _frontOutline = go.AddComponent<LineRenderer>();
                _frontOutline.useWorldSpace = false;
                _frontOutline.loop = true;
                _frontOutline.startWidth = 0.07f;
                _frontOutline.endWidth = 0.07f;
                _frontOutline.sharedMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = Color.black
                };
                _frontOutline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _frontOutline.receiveShadows = false;
                _frontOutline.numCapVertices = 2;
                _frontOutline.numCornerVertices = 2;
            }

            _frontOutline.positionCount = 4;
            _frontOutline.SetPosition(0, bl);
            _frontOutline.SetPosition(1, tl);
            _frontOutline.SetPosition(2, tr);
            _frontOutline.SetPosition(3, br);
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

        private int FindFirstEmptyFromInner()
        {
            for (int i = _slotColors.Count - 1; i >= 0; i--)
            {
                if (!_slotColors[i].HasValue) return i;
            }
            return -1;
        }

    }
}
