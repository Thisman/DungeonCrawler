// Builds and maintains an initiative-based turn queue for living squads, supporting multi-round previews with separators.
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
        private readonly Queue<SquadModel> _queue = new();
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

        public IReadOnlyList<SquadModel> GetAvailableQueue(int unitsCount)
        {
            if (unitsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitsCount), "Unit count cannot be negative.");
            }

            _requestedUnitsCount = unitsCount;
            EnsureQueueFilled();
            return _queue.ToList();
        }

        public SquadModel GetNext()
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

            if (!IsAlive(squad))
            {
                return;
            }

            var items = new List<SquadModel>(1 + _queue.Count) { squad };
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

            if (!IsAlive(squad))
            {
                return;
            }

            _queue.Enqueue(squad);
            EnsureQueueFilled();
        }

        public void MoveToCurrentRoundEnd(SquadModel squad)
        {
            if (squad == null)
            {
                throw new ArgumentNullException(nameof(squad));
            }

            var queueItems = _queue.ToList();
            queueItems.RemoveAll(item => item == squad);

            var roundBoundaryIndex = queueItems.IndexOf(null);
            var currentRoundItems = roundBoundaryIndex >= 0
                ? queueItems.GetRange(0, roundBoundaryIndex)
                : queueItems;

            // Перемещаем юнита в конец текущего раунда
            currentRoundItems.RemoveAll(item => item == squad); // на всякий случай, если он был только в хвосте
            currentRoundItems.Add(squad);

            // Собираем очередь только из текущего раунда,
            // хвост и разделители даст EnsureQueueFilled
            var rebuiltQueue = new List<SquadModel>(currentRoundItems);

            _queue.Clear();
            foreach (var item in rebuiltQueue)
            {
                _queue.Enqueue(item);
            }

            _roundPosition = 0;
            EnsureQueueFilled(); // он сам добавит один null между раундами, если нужно
        }


        public void Calculate()
        {
            CalculateRoundOrder();
            _queue.Clear();
            _roundPosition = 0;
            EnsureQueueFilled();
        }

        private void CalculateRoundOrder()
        {
            _roundOrder.Clear();
            _roundOrder.AddRange(_squads
                .Where(IsAlive)
                .OrderByDescending(s => s.Unit.Stats.Initiative));
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

        private int CountUnits(IEnumerable<SquadModel> items)
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

        private bool IsAlive(SquadModel squad)
        {
            return squad != null && !squad.IsDead && !squad.IsEmpty();
        }
    }
}
