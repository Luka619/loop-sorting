using System;
using System.Collections.Generic;

namespace LoopSorting
{
    public sealed class Conveyor
    {
        private readonly Block?[] _slots;
        private readonly Dictionary<int, Container> _ports = new Dictionary<int, Container>();

        public Conveyor(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Conveyor length must be positive.");
            }

            _slots = new Block?[length];
        }

        public int Length => _slots.Length;
        public IReadOnlyList<Block?> Slots => _slots;
        public int BlockCount => CountBlocks();

        public List<Block> RemoveBlocksWhere(Func<Block, bool> predicate)
        {
            var removed = new List<Block>();
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].HasValue && predicate(_slots[i].Value))
                {
                    removed.Add(_slots[i].Value);
                    _slots[i] = null;
                }
            }
            return removed;
        }

        public void ClearSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = null;
            }
        }

        public void FillSequential(IEnumerable<Block> blocks)
        {
            ClearSlots();
            int idx = 0;
            foreach (var b in blocks)
            {
                if (idx >= _slots.Length) break;
                _slots[idx++] = b;
            }
        }

        public bool TryPlaceAt(int index, Block block)
        {
            ValidateIndex(index);

            if (_slots[index].HasValue)
            {
                return false;
            }

            _slots[index] = block;
            return true;
        }

        public Block? GetSlot(int index)
        {
            ValidateIndex(index);
            return _slots[index];
        }

        public bool IsSlotFree(int index)
        {
            ValidateIndex(index);
            return !_slots[index].HasValue;
        }

        public void RegisterContainerPort(int beltIndex, Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            ValidateIndex(beltIndex);
            _ports[beltIndex] = container;
        }

        public void Advance(int? blockedPort = null)
        {
            int? blockIndex = blockedPort;
            bool blockActive = blockIndex.HasValue && _slots[blockIndex.Value].HasValue;

            bool hasEmpty = false;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].HasValue)
                {
                    hasEmpty = true;
                    break;
                }
            }

            if (!hasEmpty)
            {
                // Full belt: advance is a pure rotation (simultaneous move).
                // Note: blockedPort logic relies on empty slots; when full, we keep moving to allow ports to consume blocks.
                var last = _slots[_slots.Length - 1];
                for (int i = _slots.Length - 1; i >= 1; i--)
                {
                    _slots[i] = _slots[i - 1];
                }
                _slots[0] = last;
            }
            else
            {
                // Move belt contents forward in reverse order to avoid overwriting.
                for (int i = _slots.Length - 1; i >= 0; i--)
                {
                    if (!_slots[i].HasValue)
                    {
                        continue;
                    }

                    int next = (i + 1) % _slots.Length;

                    // If blocking is active and this segment is before the blocked port,
                    // do not move into the blocked port.
                    if (blockActive && i < blockIndex.Value && next == blockIndex.Value)
                    {
                        continue;
                    }

                    if (_slots[next].HasValue)
                    {
                        continue;
                    }

                    _slots[next] = _slots[i];
                    _slots[i] = null;
                }
            }

            // Try to drop blocks into connected containers after movement.
            foreach (var port in _ports)
            {
                int beltIndex = port.Key;
                var slot = _slots[beltIndex];
                if (!slot.HasValue)
                {
                    continue;
                }

                var container = port.Value;
                // New rule: a block should not enter an empty container if there exists any other
                // non-empty, non-full container whose front color matches this block.
                if (container.IsEmpty && ExistsPreferredNonEmptyTarget(container, slot.Value))
                {
                    continue;
                }
                if (!container.TryPush(slot.Value))
                {
                    continue;
                }

                _slots[beltIndex] = null;
            }
        }

        private bool ExistsPreferredNonEmptyTarget(Container emptyTarget, Block block)
        {
            foreach (var kv in _ports)
            {
                var c = kv.Value;
                if (c == null || c == emptyTarget) continue;
                if (c.Count <= 0) continue; // only consider non-empty containers
                if (c.CanAccept(block)) return true;
            }
            return false;
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private int CountBlocks()
        {
            int count = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].HasValue)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
