// Presents Unit stats and provides methods to modify them during gameplay.
using System;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Unit
{
    public class UnitStats
    {
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
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
        }

        public void Heal(float amount)
        {
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        }

        public void AddMaxHealth(float value)
        {
            MaxHealth += value;
            CurrentHealth = Math.Min(CurrentHealth, MaxHealth);
        }

        public void AddDefense(float phys, float magic, float absolute)
        {
            PhysicalDefense += phys;
            MagicDefense += magic;
            AbsoluteDefense += absolute;
        }

        public void AddDamageRange(float minDelta, float maxDelta)
        {
            MinDamage += minDelta;
            MaxDamage += maxDelta;
        }

        public void ModifyCrit(float chanceDelta, float multiplierDelta)
        {
            CritChance += chanceDelta;
            CritMultiplier += multiplierDelta;
        }

        public void ModifyInitiative(float delta)
        {
            Initiative += delta;
        }

        public void ModifyMissChance(float delta)
        {
            MissChance += delta;
        }

        public void AddExperience(float xp)
        {
            Experience += xp;
        }

        public void LevelUp()
        {
            Level++;
        }
    }
}
