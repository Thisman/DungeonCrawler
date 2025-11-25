// Provides targeting logic for the basic attack action, selecting only opposing squads.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

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

        public override bool CanExecute(SquadModel actor, BattleContext context)
        {
            return true;
        }

        public override IReadOnlyList<SquadModel> GetValidTargets(SquadModel actor, BattleContext context)
        {
            var validTargets = new List<SquadModel>();

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

                if (actor.Unit.Definition.IsFriendly() && targetDefinition.IsEnemy())
                {
                    validTargets.Add(squad);
                }
                else if (actor.Unit.Definition.IsEnemy() && targetDefinition.IsFriendly())
                {
                    validTargets.Add(squad);
                }
            }

            return validTargets;
        }
    }
}
