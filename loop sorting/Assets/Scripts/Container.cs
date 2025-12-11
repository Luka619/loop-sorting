using System;
using System.Collections.Generic;

namespace LoopSorting
{
    public sealed class Container
    {
        private readonly int _capacity;
        private readonly List<Block> _blocks;

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
        public IReadOnlyList<Block> Blocks => _blocks;

        public bool TryPeek(out Block block)
        {
            if (_blocks.Count == 0)
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

            // Insert at the end to fill inner-most first.
            _blocks.Add(block);
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
