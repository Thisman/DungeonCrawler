// Presents Unit stats and provides methods to modify them during gameplay. Fires change notifications for stat updates.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Unit
{
    public class UnitStats
    {
        public event Action<UnitStats, string, object, object> OnChange;

        public AttackType AttackType { get; }

        public DamageType DamageType { get; }

        public UnitKind Kind { get; }

        public float MaxHealth { get; private set; }

        public float CurrentHealth { get; private set; }

        public float PhysicalDefense { get; private set; }

        public float MagicDefense { get; private set; }

        public float AbsoluteDefense { get; private set; }

        public float MinDamage { get; private set; }

        public float MaxDamage { get; private set; }

        public float Initiative { get; private set; }

        public float CritChance { get; private set; }

        public float CritMultiplier { get; private set; }

        public float MissChance { get; private set; }

        public int Level { get; private set; }

        public float Experience { get; private set; }

        public bool IsDead => CurrentHealth <= 0;

        public UnitStats(UnitDefinition def, int level = 1)
        {
            AttackType = def.AttackType;
            DamageType = def.DamageType;
            Kind = def.Kind;

            Level = level;

            MaxHealth = def.BaseHealth;
            CurrentHealth = MaxHealth;

            PhysicalDefense = def.BasePhysicalDefense;
            MagicDefense = def.BaseMagicDefense;
            AbsoluteDefense = def.BaseAbsoluteDefense;

            MinDamage = def.MinDamage;
            MaxDamage = def.MaxDamage;

            Initiative = def.Initiative;

            CritChance = def.BaseCritChance;
            CritMultiplier = def.BaseCritMultiplier;
            MissChance = def.BaseMissChance;
        }

        public void TakeDamage(float amount)
        {
            SetStat(ref CurrentHealth, Math.Max(0, CurrentHealth - amount), nameof(CurrentHealth));
        }

        public void Heal(float amount)
        {
            SetStat(ref CurrentHealth, Math.Min(MaxHealth, CurrentHealth + amount), nameof(CurrentHealth));
        }

        public void AddMaxHealth(float value)
        {
            var newMaxHealth = MaxHealth + value;
            SetStat(ref MaxHealth, newMaxHealth, nameof(MaxHealth));
            SetStat(ref CurrentHealth, Math.Min(CurrentHealth, newMaxHealth), nameof(CurrentHealth));
        }

        public void AddDefense(float phys, float magic, float absolute)
        {
            SetStat(ref PhysicalDefense, PhysicalDefense + phys, nameof(PhysicalDefense));
            SetStat(ref MagicDefense, MagicDefense + magic, nameof(MagicDefense));
            SetStat(ref AbsoluteDefense, AbsoluteDefense + absolute, nameof(AbsoluteDefense));
        }

        public void AddDamageRange(float minDelta, float maxDelta)
        {
            SetStat(ref MinDamage, MinDamage + minDelta, nameof(MinDamage));
            SetStat(ref MaxDamage, MaxDamage + maxDelta, nameof(MaxDamage));
        }

        public void ModifyCrit(float chanceDelta, float multiplierDelta)
        {
            SetStat(ref CritChance, CritChance + chanceDelta, nameof(CritChance));
            SetStat(ref CritMultiplier, CritMultiplier + multiplierDelta, nameof(CritMultiplier));
        }

        public void ModifyInitiative(float delta)
        {
            SetStat(ref Initiative, Initiative + delta, nameof(Initiative));
        }

        public void ModifyMissChance(float delta)
        {
            SetStat(ref MissChance, MissChance + delta, nameof(MissChance));
        }

        public void AddExperience(float xp)
        {
            SetStat(ref Experience, Experience + xp, nameof(Experience));
        }

        public void LevelUp()
        {
            SetStat(ref Level, Level + 1, nameof(Level));
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
