// Provides targeting logic for the basic attack action, selecting only opposing living units.
using DungeonCrawler.Gameplay.Unit;
using System.Collections.Generic;

namespace DungeonCrawler.Gameplay.Battle
{
    public class UnitAttackAction : UnitAction
    {
        public UnitAttackAction() {
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
            var validTargets = new List<UnitModel>();

            if (actor == null || context == null)
            {
                return validTargets;
            }

            foreach (var squad in context.Squads)
            {
                if (squad.IsEmpty() || squad.IsDead)
                {
                    continue;
                }

                var targetDefinition = squad.Unit.Definition;

                if (actor.Definition.IsFriendly() && targetDefinition.IsEnemy())
                {
                    validTargets.Add(squad.Unit);
                }
                else if (actor.Definition.IsEnemy() && targetDefinition.IsFriendly())
                {
                    validTargets.Add(squad.Unit);
                }
            }

            return validTargets;
        }
    }
}