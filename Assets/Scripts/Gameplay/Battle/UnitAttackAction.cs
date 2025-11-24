// Defines a basic attack action that targets enemy units relative to the acting unit.
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using System.Collections.Generic;
using System.Linq;

namespace DungeonCrawler.Gameplay.Battle
{
    public class UnitAttackAction : UnitAction
    {
        public UnitAttackAction()
        {
            Name = "Attack";
            Id = "Attack";
            Type = ActionType.Attack;
        }

        public override bool CanExecute(UnitModel actor, BattleContext context)
        {
            return true;
        }

        public override IReadOnlyList<UnitModel> GetValidTargets(UnitModel actor, BattleContext context)
        {
            if (actor == null || context?.Squads == null)
            {
                return new List<UnitModel>();
            }

            var actorKind = actor.Definition.Kind;

            return context.Squads
                .Select(squad => squad.Unit)
                .Where(target => IsEnemyForActor(actorKind, target))
                .ToList();
        }

        private static bool IsEnemyForActor(UnitKind actorKind, UnitModel target)
        {
            if (target == null)
            {
                return false;
            }

            return actorKind switch
            {
                UnitKind.Enemy => target.Definition.IsFriendly(),
                UnitKind.Ally or UnitKind.Hero => target.Definition.IsEnemy(),
                _ => target.Definition.Kind != actorKind
            };
        }
    }
}
