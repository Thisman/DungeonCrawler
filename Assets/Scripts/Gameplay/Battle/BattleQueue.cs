// Builds and maintains an initiative-based turn queue for squads, supporting multi-round previews with separators.
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleQueue
    {
        private readonly List<SquadModel> _squads;
        private readonly List<SquadModel> _roundOrder = new();
        private readonly Queue<SquadModel?> _queue = new();
        private int _roundPosition;
        private int _requestedUnitsCount;

        public BattleQueue(IEnumerable<SquadModel> squads)
        {
            if (squads == null)
            {
                throw new ArgumentNullException(nameof(squads));
            }

            _squads = squads.ToList();

            if (_squads.Any(s => s == null))
            {
                throw new ArgumentException("Squads collection cannot contain null values.", nameof(squads));
            }

            CalculateRoundOrder();
        }

        public IReadOnlyList<SquadModel?> GetAvailableQueue(int unitsCount)
        {
            if (unitsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitsCount), "Unit count cannot be negative.");
            }

            _requestedUnitsCount = unitsCount;
            EnsureQueueFilled();
            return _queue.ToList();
        }

        public SquadModel? GetNext()
        {
            if (_queue.Count == 0)
            {
                EnsureQueueFilled();
            }

            if (_queue.Count == 0)
            {
                return null;
            }

            var next = _queue.Dequeue();

            if (_requestedUnitsCount > 0)
            {
                EnsureQueueFilled();
            }

            return next;
        }

        public void AddFirst(SquadModel squad)
        {
            if (squad == null)
            {
                throw new ArgumentNullException(nameof(squad));
            }

            var items = new List<SquadModel?>(1 + _queue.Count) { squad };
            items.AddRange(_queue);

            _queue.Clear();

            foreach (var item in items)
            {
                _queue.Enqueue(item);
            }

            EnsureQueueFilled();
        }

        public void AddLast(SquadModel squad)
        {
            if (squad == null)
            {
                throw new ArgumentNullException(nameof(squad));
            }

            _queue.Enqueue(squad);
            EnsureQueueFilled();
        }

        public void Calculate()
        {
            CalculateRoundOrder();
            _queue.Clear();
            _roundPosition = 0;
            EnsureQueueFilled();
        }

        public void MoveToCurrentRoundEnd(SquadModel squad)
        {
            if (squad == null)
            {
                throw new ArgumentNullException(nameof(squad));
            }

            if (_queue.Count == 0)
            {
                return;
            }

            var items = _queue.ToList();
            var separatorIndex = items.FindIndex(item => item == null);
            var currentRoundEnd = separatorIndex >= 0 ? separatorIndex : items.Count;

            for (var i = 0; i < currentRoundEnd; i++)
            {
                if (items[i] == squad)
                {
                    items.RemoveAt(i);
                    currentRoundEnd--;
                    break;
                }
            }

            var insertIndex = currentRoundEnd;
            items.Insert(insertIndex, squad);

            _queue.Clear();

            foreach (var item in items)
            {
                _queue.Enqueue(item);
            }

            EnsureQueueFilled();
        }

        private void CalculateRoundOrder()
        {
            _roundOrder.Clear();
            _roundOrder.AddRange(_squads.OrderByDescending(s => s.Unit.Stats.Initiative));
        }

        private void EnsureQueueFilled()
        {
            if (_roundOrder.Count == 0 || _requestedUnitsCount == 0)
            {
                return;
            }

            var unitsInQueue = CountUnits(_queue);

            while (unitsInQueue < _requestedUnitsCount)
            {
                if (_roundPosition == 0 && _queue.Count > 0)
                {
                    _queue.Enqueue(null);
                }

                while (_roundPosition < _roundOrder.Count && unitsInQueue < _requestedUnitsCount)
                {
                    _queue.Enqueue(_roundOrder[_roundPosition]);
                    _roundPosition++;
                    unitsInQueue++;
                }

                if (_roundPosition >= _roundOrder.Count)
                {
                    _roundPosition = 0;
                }
            }
        }

        private int CountUnits(IEnumerable<SquadModel?> items)
        {
            var count = 0;

            foreach (var item in items)
            {
                if (item != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
