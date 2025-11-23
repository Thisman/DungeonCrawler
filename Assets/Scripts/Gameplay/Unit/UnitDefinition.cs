// Defines the data model for a gameplay unit as a ScriptableObject asset.
using UnityEngine;
using DungeonCrawler.Gameplay.Battle;

namespace DungeonCrawler.Gameplay.Unit
{
    [CreateAssetMenu(fileName = "Unit_Default", menuName = "DungeonCrawler/Unit/Definition")]
    public class UnitDefinition : ScriptableObject
    {
        public Sprite Icon;

        public string Name = "Unit_Default";

        public UnitKind Kind = UnitKind.Neutral;

        public AttackType AttackType = AttackType.Melee;

        public DamageType DamageType = DamageType.Physical;

        [Min(1)]
        public float BaseHealth = 100;

        [Range(0, 100)]
        public float BasePhysicalDefense = 0;

        [Range(0, 100)]
        public float BaseMagicDefense = 0;

        [Range(0, 100)]
        public float BaseAbsoluteDefense = 0;

        [Min(1)]
        public float MinDamage = 10;

        [Min(1)]
        public float MaxDamage = 20;

        [Min(1)]
        public float Initiative = 2;

        [Range(0, 100)]
        public float BaseCritChance = 1;

        [Min(1)]
        public float BaseCritMultiplier = 1.1f;

        [Range(0, 100)]
        public float BaseMissChance = 5;

        public (float min, float max) GetBaseDamageRange() => (MinDamage, MaxDamage);

        public bool IsFriendly() => Kind == UnitKind.Ally || Kind == UnitKind.Hero;

        public bool IsAlly() => Kind == UnitKind.Ally;

        public bool IsHero() => Kind == UnitKind.Hero;

        public bool IsEnemy() => Kind == UnitKind.Enemy;

        public bool IsNeutral() => Kind == UnitKind.Neutral;
    }
}
