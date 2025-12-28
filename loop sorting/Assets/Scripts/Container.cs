using System;
using System.Collections.Generic;

namespace LoopSorting
{
    public sealed class Container
    {
        private readonly int _capacity;
        private readonly List<Block> _blocks;
        private bool _locked;
        private bool _busy;

        public Container(int capacity, IEnumerable<Block> initialBlocks = null)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            }

            _capacity = capacity;
            _blocks = initialBlocks != null ? new List<Block>(initialBlocks) : new List<Block>();

            if (_blocks.Count > _capacity)
            {
                throw new ArgumentException("Initial blocks exceed capacity.", nameof(initialBlocks));
            }
        }

        public int Capacity => _capacity;
        public int Count => _blocks.Count;
        public bool IsEmpty => _blocks.Count == 0;
        public bool Locked => _locked;
        public bool Busy => _busy;
        public IReadOnlyList<Block> Blocks => _blocks;

        public void SetLocked(bool locked)
        {
            _locked = locked;
        }

        public void SetBusy(bool busy)
        {
            _busy = busy;
        }

        /// <summary>
        /// Remove blocks that match predicate and return them.
        /// </summary>
        public List<Block> RemoveBlocksWhere(Func<Block, bool> predicate)
        {
            var removed = new List<Block>();
            if (_locked)
            {
                return removed;
            }
            for (int i = _blocks.Count - 1; i >= 0; i--)
            {
                if (predicate(_blocks[i]))
                {
                    removed.Add(_blocks[i]);
                    _blocks.RemoveAt(i);
                }
            }
            removed.Reverse();
            return removed;
        }

        /// <summary>
        /// Clear current blocks and add new ones up to capacity.
        /// </summary>
        public void ClearAndAdd(IEnumerable<Block> blocks)
        {
            if (_locked)
            {
                return;
            }
            _blocks.Clear();
            AddBlocks(blocks);
        }

        /// <summary>
        /// Append blocks in list order (outer-to-inner) up to capacity.
        /// </summary>
        public void AddBlocks(IEnumerable<Block> blocks)
        {
            if (_locked)
            {
                return;
            }
            foreach (var b in blocks)
            {
                if (_blocks.Count >= _capacity) break;
                _blocks.Add(b);
            }
        }

        public bool TryPeek(out Block block)
        {
            if (_locked || _blocks.Count == 0)
            {
                block = default;
                return false;
            }

            block = _blocks[0];
            return true;
        }

        public bool TryPop(out Block block)
        {
            if (!TryPeek(out block))
            {
                return false;
            }

            _blocks.RemoveAt(0);
            return true;
        }

        public bool CanAccept(Block block)
        {
            if (_locked || _busy)
            {
                return false;
            }

            if (_blocks.Count >= _capacity)
            {
                return false;
            }

            if (_blocks.Count == 0)
            {
                return true;
            }

            return _blocks[0].Color == block.Color;
        }

        public bool TryPush(Block block)
        {
            if (!CanAccept(block))
            {
                return false;
            }

            // Insert at the front (mouth-side) so blocks never pass through deeper blocks.
            // This prevents creating split patterns like A-B-A when pushing an A into a box whose front is A.
            _blocks.Insert(0, block);
            return true;
        }

        public bool TryForcePush(Block block, bool ignoreBusy = true, bool ignoreLocked = false)
        {
            if (_locked && !ignoreLocked)
            {
                return false;
            }

            if (_busy && !ignoreBusy)
            {
                return false;
            }

            if (_blocks.Count >= _capacity)
            {
                return false;
            }

            _blocks.Insert(0, block);
            return true;
        }

        public bool IsUniform()
        {
            if (_blocks.Count <= 1)
            {
                return true;
            }

            var color = _blocks[0].Color;
            for (int i = 1; i < _blocks.Count; i++)
            {
                if (_blocks[i].Color != color)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsUniformAndFull()
        {
            return _blocks.Count == _capacity && IsUniform();
        }
    }
}
