using System;
using System.Collections.Generic;

namespace LoopSorting
{
    public enum ConveyorPortOutcome
    {
        Inserted,
        RejectedLocked,
        RejectedBusy,
        RejectedFull,
        RejectedMismatch,
        SkippedEmptyBoxPreferredTarget
    }

    public readonly struct ConveyorPortEvent
    {
        public int BeltIndex { get; }
        public Container Container { get; }
        public Block Block { get; }
        public ConveyorPortOutcome Outcome { get; }

        public ConveyorPortEvent(int beltIndex, Container container, Block block, ConveyorPortOutcome outcome)
        {
            BeltIndex = beltIndex;
            Container = container;
            Block = block;
            Outcome = outcome;
        }
    }

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

        public void Advance(int? blockedPort = null, bool allowInsert = true, Func<int, bool> canInsertAtPort = null)
        {
            Advance(blockedPort, events: null, allowInsert: allowInsert, canInsertAtPort: canInsertAtPort);
        }

        public void Advance(int? blockedPort, List<ConveyorPortEvent> events, bool allowInsert = true, Func<int, bool> canInsertAtPort = null)
        {
            if (blockedPort.HasValue)
            {
                int blockIndex = blockedPort.Value;
                ValidateIndex(blockIndex);

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
                    // Full belt: still rotate to honor the "always moving" rule.
                    var last = _slots[_slots.Length - 1];
                    for (int i = _slots.Length - 1; i >= 1; i--)
                    {
                        _slots[i] = _slots[i - 1];
                    }
                    _slots[0] = last;
                }
                else
                {
                    // Identify the waiting chain behind the blocked port so those blocks stay in place.
                    var waiting = new bool[_slots.Length];
                    int idx = (blockIndex - 1 + _slots.Length) % _slots.Length;
                    while (idx != blockIndex)
                    {
                        if (!_slots[idx].HasValue)
                        {
                            break;
                        }
                        waiting[idx] = true;
                        idx = (idx - 1 + _slots.Length) % _slots.Length;
                    }

                    // Rotate only the movable slots so gaps keep moving with the belt.
                    var movable = new List<int>(_slots.Length);
                    for (int i = 0; i < _slots.Length; i++)
                    {
                        if (!waiting[i])
                        {
                            movable.Add(i);
                        }
                    }

                    if (movable.Count > 0)
                    {
                        var oldSlots = (Block?[])_slots.Clone();
                        for (int i = 0; i < movable.Count; i++)
                        {
                            _slots[movable[i]] = null;
                        }

                        for (int i = 0; i < movable.Count; i++)
                        {
                            int src = movable[i];
                            var block = oldSlots[src];
                            if (!block.HasValue) continue;

                            int dst = movable[(i + 1) % movable.Count];
                            if (dst == blockIndex)
                            {
                                _slots[src] = block;
                            }
                            else
                            {
                                _slots[dst] = block;
                            }
                        }
                    }
                }
            }
            else
            {
                // Normal advance: single rotation keeps relative order stable.
                var last = _slots[_slots.Length - 1];
                for (int i = _slots.Length - 1; i >= 1; i--)
                {
                    _slots[i] = _slots[i - 1];
                }
                _slots[0] = last;
            }

            if (!allowInsert)
            {
                return;
            }

            // Try to drop blocks into connected containers after movement.
            foreach (var port in _ports)
            {
                int beltIndex = port.Key;
                if (canInsertAtPort != null && !canInsertAtPort(beltIndex))
                {
                    continue;
                }
                var slot = _slots[beltIndex];
                if (!slot.HasValue)
                {
                    continue;
                }

                var container = port.Value;
                if (container == null)
                {
                    continue;
                }

                // New rule: a block should not enter an empty container if there exists any other
                // non-empty, non-full container whose front color matches this block.
                if (container.IsEmpty && ExistsPreferredNonEmptyTarget(container, slot.Value))
                {
                    events?.Add(new ConveyorPortEvent(beltIndex, container, slot.Value, ConveyorPortOutcome.SkippedEmptyBoxPreferredTarget));
                    continue;
                }

                var block = slot.Value;
                if (container.Locked)
                {
                    events?.Add(new ConveyorPortEvent(beltIndex, container, block, ConveyorPortOutcome.RejectedLocked));
                    continue;
                }
                if (container.Busy)
                {
                    events?.Add(new ConveyorPortEvent(beltIndex, container, block, ConveyorPortOutcome.RejectedBusy));
                    continue;
                }
                if (container.Count >= container.Capacity)
                {
                    events?.Add(new ConveyorPortEvent(beltIndex, container, block, ConveyorPortOutcome.RejectedFull));
                    continue;
                }
                if (container.Count > 0 && container.Blocks[0].Color != block.Color)
                {
                    events?.Add(new ConveyorPortEvent(beltIndex, container, block, ConveyorPortOutcome.RejectedMismatch));
                    continue;
                }
                if (!container.TryPush(block))
                {
                    // Should not happen given checks above, but keep it safe.
                    events?.Add(new ConveyorPortEvent(beltIndex, container, block, ConveyorPortOutcome.RejectedMismatch));
                    continue;
                }

                events?.Add(new ConveyorPortEvent(beltIndex, container, block, ConveyorPortOutcome.Inserted));
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
