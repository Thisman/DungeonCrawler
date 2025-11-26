// Provides targeting logic for the basic attack action, selecting valid opposing squads with frontline restrictions.
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

            var actorDefinition = actor.Unit?.Definition;
            var actorIsFriendly = actorDefinition != null && actorDefinition.IsFriendly();
            var actorIsEnemy = actorDefinition != null && actorDefinition.IsEnemy();

            foreach (var squad in context.Squads)
            {
                if (squad.IsEmpty() || squad.IsDead)
                {
                    continue;
                }

                var targetDefinition = squad.Unit.Definition;

                if (actorIsFriendly && targetDefinition.IsEnemy())
                {
                    validTargets.Add(squad);
                }
                else if (actorIsEnemy && targetDefinition.IsFriendly())
                {
                    validTargets.Add(squad);
                }
            }

            if (actorDefinition?.AttackType == AttackType.Melee)
            {
                var targetIsEnemySide = actorIsFriendly;
                var targetFrontlineSquads = context.GetAliveSquadsInRow(targetIsEnemySide, BattleRow.Front);

                if (targetFrontlineSquads.Count > 0)
                {
                    validTargets.RemoveAll(target => !IsFrontRowTarget(context, target));
                }
            }

            return validTargets;
        }

        private static bool IsFrontRowTarget(BattleContext context, SquadModel target)
        {
            return context.TryGetPlacement(target, out var placement) && placement.Row == BattleRow.Front;
        }
    }
}
