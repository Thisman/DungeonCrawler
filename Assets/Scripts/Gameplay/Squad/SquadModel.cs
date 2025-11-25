// Encapsulates unit grouping data with health tracking, count management, and change notifications.
using System;
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Squad
{
    public class SquadModel
    {
        private int _unitCount;
        private float _currentTotalHealth;
        private bool _isDead;

        public SquadModel(UnitModel unit, int unitCount)
        {
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));

            if (unitCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitCount), "Unit count cannot be negative.");
            }

            _unitCount = unitCount;
            _currentTotalHealth = _unitCount * Unit.Stats.MaxHealth;
        }

        public event Action<SquadModel, int, int> Changed;

        public UnitModel Unit { get; }

        public int UnitCount => _unitCount;

        public float CurrentTotalHealth => _currentTotalHealth;

        public bool IsDead => _isDead;

        public void AddUnits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add cannot be negative.");
            }

            var newCount = _unitCount + amount;
            _currentTotalHealth = Math.Min(newCount * Unit.Stats.MaxHealth, _currentTotalHealth + amount * Unit.Stats.MaxHealth);

            UpdateCount(newCount);
        }

        public void RemoveUnits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to remove cannot be negative.");
            }

            var newCount = Math.Max(0, _unitCount - amount);
            _currentTotalHealth = Math.Min(_currentTotalHealth, newCount * Unit.Stats.MaxHealth);

            UpdateCount(newCount);
        }

        public void ApplyDamage(float amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot be negative.");
            }

            if (_isDead)
            {
                return;
            }

            _currentTotalHealth = Math.Max(0, _currentTotalHealth - amount);

            var maxHealthPerUnit = Unit.Stats.MaxHealth;
            var newCount = maxHealthPerUnit > 0 ? (int)Math.Ceiling(_currentTotalHealth / maxHealthPerUnit) : 0;

            if (newCount <= 0)
            {
                _currentTotalHealth = 0;
                MarkDead();
                UpdateCount(0);
                return;
            }

            UpdateCount(newCount);
        }

        public bool IsEmpty() => _unitCount == 0;

        private void UpdateCount(int newCount, bool forceNotify = false)
        {
            if (newCount == _unitCount && !forceNotify)
            {
                return;
            }

            var oldCount = _unitCount;
            _unitCount = newCount;

            if (_unitCount <= 0)
            {
                MarkDead();
            }

            Changed?.Invoke(this, newCount, oldCount);
        }

        private void MarkDead()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            _currentTotalHealth = 0;
        }
    }
}
