using System;
using System.Collections.Generic;
using System.Linq;

namespace LoopSorting
{
    public enum ReleaseResult
    {
        Success,
        Empty,
        NoPort,
        BeltBlocked
    }

    public sealed class LoopSortingGame
    {
        private readonly Conveyor _conveyor;
        private readonly List<Container> _containers;
        private readonly Dictionary<int, int> _containerPorts;
        private readonly int _beltCapacity;

        public LoopSortingGame(
            int conveyorLength,
            IReadOnlyList<Container> containers,
            IReadOnlyDictionary<int, int> containerToBeltIndex,
            int beltCapacity = 0)
        {
            if (containers == null)
            {
                throw new ArgumentNullException(nameof(containers));
            }

            if (containerToBeltIndex == null)
            {
                throw new ArgumentNullException(nameof(containerToBeltIndex));
            }

            _containers = new List<Container>(containers);
            _conveyor = new Conveyor(conveyorLength);
            _containerPorts = new Dictionary<int, int>(containerToBeltIndex);
            _beltCapacity = beltCapacity;

            foreach (var port in _containerPorts)
            {
                if (port.Key < 0 || port.Key >= _containers.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(containerToBeltIndex), "Container index out of range.");
                }

                _conveyor.RegisterContainerPort(port.Value, _containers[port.Key]);
            }
        }

        public Conveyor Conveyor => _conveyor;
        public IReadOnlyList<Container> Containers => _containers;

        public ReleaseResult TryReleaseFromContainer(int containerIndex)
        {
            if (!_containerPorts.TryGetValue(containerIndex, out var beltIndex))
            {
                return ReleaseResult.NoPort;
            }

            var container = _containers[containerIndex];
            if (!container.TryPeek(out var block))
            {
                return ReleaseResult.Empty;
            }

            if (_beltCapacity > 0 && _conveyor.BlockCount >= _beltCapacity)
            {
                return ReleaseResult.BeltBlocked;
            }

            if (!_conveyor.TryPlaceAt(beltIndex, block))
            {
                return ReleaseResult.BeltBlocked;
            }

            container.TryPop(out _);
            return ReleaseResult.Success;
        }

        public void TickConveyor(int? blockedPort = null, bool allowInsert = true)
        {
            _conveyor.Advance(blockedPort, allowInsert);
        }

        public void TickConveyor(int? blockedPort, List<ConveyorPortEvent> events, bool allowInsert = true)
        {
            _conveyor.Advance(blockedPort, events, allowInsert);
        }

        public bool IsSolved(bool requireFull = false)
        {
            if (requireFull)
            {
                return _containers.All(c => c.IsUniformAndFull());
            }

            return _containers.All(c => c.IsUniform());
        }

        public int TotalBlockCount()
        {
            return _containers.Sum(c => c.Count) + _conveyor.BlockCount;
        }
    }
}
