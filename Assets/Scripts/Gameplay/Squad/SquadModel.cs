// Encapsulates unit grouping data with count management and change notifications.
using System;
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Squad
{
    public class SquadModel
    {
        private int _unitCount;

        public SquadModel(UnitModel unit, int unitCount)
        {
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));

            if (unitCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitCount), "Unit count cannot be negative.");
            }

            _unitCount = unitCount;
        }

        public event Action<SquadModel, int, int> Changed;

        public UnitModel Unit { get; }

        public int UnitCount => _unitCount;

        public void AddUnits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add cannot be negative.");
            }

            UpdateCount(_unitCount + amount);
        }

        public void RemoveUnits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to remove cannot be negative.");
            }

            UpdateCount(Math.Max(0, _unitCount - amount));
        }

        public bool IsEmpty() => _unitCount == 0;

        private void UpdateCount(int newCount)
        {
            if (newCount == _unitCount)
            {
                return;
            }

            var oldCount = _unitCount;
            _unitCount = newCount;
            Changed?.Invoke(this, newCount, oldCount);
        }
    }
}
