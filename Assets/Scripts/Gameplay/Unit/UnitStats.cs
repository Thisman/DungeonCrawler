// Presents Unit stats and provides methods to modify them during gameplay. Fires change notifications for stat updates.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Unit
{
    public class UnitStats
    {
        private float _maxHealth;
        private float _currentHealth;
        private float _physicalDefense;
        private float _magicDefense;
        private float _absoluteDefense;
        private float _minDamage;
        private float _maxDamage;
        private float _initiative;
        private float _critChance;
        private float _critMultiplier;
        private float _missChance;
        private int _level;
        private float _experience;

        public event Action<UnitStats, string, object, object> OnChange;

        public AttackType AttackType { get; }

        public DamageType DamageType { get; }

        public UnitKind Kind { get; }

        public float MaxHealth => _maxHealth;

        public float CurrentHealth => _currentHealth;

        public float PhysicalDefense => _physicalDefense;

        public float MagicDefense => _magicDefense;

        public float AbsoluteDefense => _absoluteDefense;

        public float MinDamage => _minDamage;

        public float MaxDamage => _maxDamage;

        public float Initiative => _initiative;

        public float CritChance => _critChance;

        public float CritMultiplier => _critMultiplier;

        public float MissChance => _missChance;

        public int Level => _level;

        public float Experience => _experience;

        public bool IsDead => CurrentHealth <= 0;

        public UnitStats(UnitDefinition def, int level = 1)
        {
            AttackType = def.AttackType;
            DamageType = def.DamageType;
            Kind = def.Kind;

            _level = level;

            _maxHealth = def.BaseHealth;
            _currentHealth = _maxHealth;

            _physicalDefense = def.BasePhysicalDefense;
            _magicDefense = def.BaseMagicDefense;
            _absoluteDefense = def.BaseAbsoluteDefense;

            _minDamage = def.MinDamage;
            _maxDamage = def.MaxDamage;

            _initiative = def.Initiative;

            _critChance = def.BaseCritChance;
            _critMultiplier = def.BaseCritMultiplier;
            _missChance = def.BaseMissChance;
        }

        public void TakeDamage(float amount)
        {
            SetStat(ref _currentHealth, Math.Max(0, _currentHealth - amount), nameof(CurrentHealth));
        }

        public void Heal(float amount)
        {
            SetStat(ref _currentHealth, Math.Min(_maxHealth, _currentHealth + amount), nameof(CurrentHealth));
        }

        public void AddMaxHealth(float value)
        {
            var newMaxHealth = _maxHealth + value;
            SetStat(ref _maxHealth, newMaxHealth, nameof(MaxHealth));
            SetStat(ref _currentHealth, Math.Min(_currentHealth, newMaxHealth), nameof(CurrentHealth));
        }

        public void AddDefense(float phys, float magic, float absolute)
        {
            SetStat(ref _physicalDefense, _physicalDefense + phys, nameof(PhysicalDefense));
            SetStat(ref _magicDefense, _magicDefense + magic, nameof(MagicDefense));
            SetStat(ref _absoluteDefense, _absoluteDefense + absolute, nameof(AbsoluteDefense));
        }

        public void AddDamageRange(float minDelta, float maxDelta)
        {
            SetStat(ref _minDamage, _minDamage + minDelta, nameof(MinDamage));
            SetStat(ref _maxDamage, _maxDamage + maxDelta, nameof(MaxDamage));
        }

        public void ModifyCrit(float chanceDelta, float multiplierDelta)
        {
            SetStat(ref _critChance, _critChance + chanceDelta, nameof(CritChance));
            SetStat(ref _critMultiplier, _critMultiplier + multiplierDelta, nameof(CritMultiplier));
        }

        public void ModifyInitiative(float delta)
        {
            SetStat(ref _initiative, _initiative + delta, nameof(Initiative));
        }

        public void ModifyMissChance(float delta)
        {
            SetStat(ref _missChance, _missChance + delta, nameof(MissChance));
        }

        public void AddExperience(float xp)
        {
            SetStat(ref _experience, _experience + xp, nameof(Experience));
        }

        public void LevelUp()
        {
            SetStat(ref _level, _level + 1, nameof(Level));
        }

        private void SetStat<T>(ref T field, T newValue, string fieldName)
        {
            var oldValue = field;

            if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                return;
            }

            field = newValue;
            OnChange?.Invoke(this, fieldName, newValue, oldValue);
        }
    }
}
