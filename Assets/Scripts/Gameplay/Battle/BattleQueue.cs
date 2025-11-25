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
            RemoveDeadUnitsFromQueue();
            EnsureQueueFilled();
            return _queue.Where(item => item == null || !item.IsDead).ToList();
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

            SquadModel next = null;

            while (_queue.Count > 0 && next == null)
            {
                var candidate = _queue.Dequeue();

                if (candidate == null || !candidate.IsDead)
                {
                    next = candidate;
                }
            }

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

            if (squad.IsDead)
            {
                throw new ArgumentException("Squad must be alive to enter the queue.", nameof(squad));
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

            if (squad.IsDead)
            {
                throw new ArgumentException("Squad must be alive to enter the queue.", nameof(squad));
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

            if (squad.IsDead)
            {
                throw new ArgumentException("Squad must be alive to enter the queue.", nameof(squad));
            }

            var queueItems = _queue.ToList();
            queueItems.RemoveAll(item => item == squad);

            var roundBoundaryIndex = queueItems.IndexOf(null);
            var currentRoundItems = roundBoundaryIndex >= 0
                ? queueItems.GetRange(0, roundBoundaryIndex)
                : queueItems;

            // ���������� ����� � ����� �������� ������
            currentRoundItems.RemoveAll(item => item == squad); // �� ������ ������, ���� �� ��� ������ � ������
            currentRoundItems.Add(squad);

            // �������� ������� ������ �� �������� ������,
            // ����� � ����������� ���� EnsureQueueFilled
            var rebuiltQueue = new List<SquadModel>(currentRoundItems);

            _queue.Clear();
            foreach (var item in rebuiltQueue)
            {
                _queue.Enqueue(item);
            }

            _roundPosition = 0;
            EnsureQueueFilled(); // �� ��� ������� ���� null ����� ��������, ���� �����
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
                .Where(squad => squad != null && !squad.IsDead)
                .OrderByDescending(s => s.Unit.Stats.Initiative));
        }

        private void EnsureQueueFilled()
        {
            RemoveDeadUnitsFromQueue();
            RemoveDeadFromRoundOrder();

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
                    var squad = _roundOrder[_roundPosition];
                    _roundPosition++;

                    if (squad != null && !squad.IsDead)
                    {
                        _queue.Enqueue(squad);
                        unitsInQueue++;
                    }
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
                if (item != null && !item.IsDead)
                {
                    count++;
                }
            }

            return count;
        }

        private void RemoveDeadUnitsFromQueue()
        {
            if (_queue.Count == 0)
            {
                return;
            }

            var items = _queue.ToList();
            _queue.Clear();

            foreach (var item in items)
            {
                if (item == null || !item.IsDead)
                {
                    _queue.Enqueue(item);
                }
            }
        }

        private void RemoveDeadFromRoundOrder()
        {
            _roundOrder.RemoveAll(item => item == null || item.IsDead);
            _roundPosition = Math.Min(_roundPosition, _roundOrder.Count);
        }
    }
}
