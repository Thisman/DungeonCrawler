// Describes a resolved damage event including participants, value, type, and hit status.
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public class DamageInstance
    {
        public DamageInstance(SquadModel attacker, SquadModel target, float amount, DamageType damageType, bool isHit)
        {
            Attacker = attacker;
            Target = target;
            Amount = amount;
            DamageType = damageType;
            IsHit = isHit;
        }

        public SquadModel Attacker { get; }

        public SquadModel Target { get; }

        public float Amount { get; }

        public DamageType DamageType { get; }

        public bool IsHit { get; }
    }
}
