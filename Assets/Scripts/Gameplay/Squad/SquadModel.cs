// Encapsulates unit grouping data with count management, total health tracking, and change notifications.
using System;
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Squad
{
    public class SquadModel
    {
        private int _unitCount;
        private float _currentTotalHealth;

        public SquadModel(UnitModel unit, int unitCount)
        {
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));

            if (unitCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitCount), "Unit count cannot be negative.");
            }

            _unitCount = unitCount;
            _currentTotalHealth = CalculateMaxTotalHealth(_unitCount);
        }

        public event Action<SquadModel, int, int> Changed;

        public UnitModel Unit { get; }

        public int UnitCount => _unitCount;

        public float CurrentTotalHealth => _currentTotalHealth;

        public bool IsDead => _unitCount <= 0 || _currentTotalHealth <= 0f;

        public void AddUnits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add cannot be negative.");
            }

            _currentTotalHealth += CalculateMaxTotalHealth(amount);
            UpdateCount(_unitCount + amount);
        }

        public void RemoveUnits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to remove cannot be negative.");
            }

            UpdateCount(Math.Max(0, _unitCount - amount), alignHealthToCount: true);
        }

        public void ApplyDamage(float amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage amount cannot be negative.");
            }

            if (IsDead)
            {
                return;
            }

            var maxHealthPerUnit = CalculateMaxTotalHealth(1);

            if (maxHealthPerUnit <= 0f)
            {
                UpdateCount(0);
                _currentTotalHealth = 0f;
                return;
            }

            _currentTotalHealth = Math.Max(0f, _currentTotalHealth - amount);
            var newCount = (int)Math.Ceiling(_currentTotalHealth / maxHealthPerUnit);

            if (newCount <= 0)
            {
                _currentTotalHealth = 0f;
            }

            UpdateCount(newCount);
        }

        public bool IsEmpty() => _unitCount == 0;

        private void UpdateCount(int newCount, bool alignHealthToCount = false)
        {
            if (newCount == _unitCount)
            {
                return;
            }

            var oldCount = _unitCount;
            _unitCount = newCount;

            if (alignHealthToCount)
            {
                _currentTotalHealth = Math.Min(_currentTotalHealth, CalculateMaxTotalHealth(_unitCount));
            }

            Changed?.Invoke(this, newCount, oldCount);
        }

        private float CalculateMaxTotalHealth(int unitCount)
        {
            return (Unit?.Stats?.MaxHealth ?? 0f) * Math.Max(0, unitCount);
        }
    }
}
