// Describes a resolved damage event including participants, value, type, and hit status.
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Battle
{
    public class DamageInstance
    {
        public DamageInstance(UnitModel attacker, UnitModel target, float amount, DamageType damageType, bool isHit)
        {
            Attacker = attacker;
            Target = target;
            Amount = amount;
            DamageType = damageType;
            IsHit = isHit;
        }

        public UnitModel Attacker { get; }

        public UnitModel Target { get; }

        public float Amount { get; }

        public DamageType DamageType { get; }

        public bool IsHit { get; }
    }
}
