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
            if (_locked) return;
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

        public void SetLocked(bool val, BlockColor unlockColor = BlockColor.Red)
        {
            _locked = val;
            if (_lockOverlay == null)
            {
                _lockOverlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _lockOverlay.name = "LockOverlay";
                _lockOverlay.transform.SetParent(transform, false);
                _lockOverlay.transform.localPosition = new Vector3(0f, 0f, -0.2f); // in front of blocks
                _lockOverlay.transform.localScale = new Vector3(_boxSize.x * 1.05f, _boxSize.y * 1.05f, 1f);
                var rend = _lockOverlay.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(Shader.Find("Unlit/Color"))
                    {
                        color = new Color(0.5f, 0.5f, 0.5f, 0.9f)
                    };
                    mat.renderQueue = 3000;
                    rend.sharedMaterial = mat;
                }
                var col = _lockOverlay.GetComponent<Collider>();
                if (col != null) GameObject.Destroy(col);

                _lockBadge = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _lockBadge.name = "LockBadge";
                _lockBadge.transform.SetParent(_lockOverlay.transform, false);
                _lockBadge.transform.localPosition = new Vector3(0f, 0f, -0.05f); // above overlay
                float badgeSize = Mathf.Min(_boxSize.x, _boxSize.y) * 0.3f;
                _lockBadge.transform.localScale = new Vector3(badgeSize, badgeSize, 1f);
                var badgeRend = _lockBadge.GetComponent<Renderer>();
                if (badgeRend != null)
                {
                    var bmat = new Material(Shader.Find("Unlit/Color"));
                    bmat.renderQueue = 3001;
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
