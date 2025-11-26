// Provides targeting logic for the basic attack action, selecting opposing squads with attack-type reach rules.
using System;
using System.Collections.Generic;
using System.Linq;
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

            var attackType = actor?.Unit?.Stats.AttackType ?? AttackType.Melee;

            foreach (var squad in context.Squads)
            {
                if (squad.IsEmpty() || squad.IsDead)
                {
                    continue;
                }

                var targetDefinition = squad.Unit.Definition;

                if (actor.Unit.Definition.IsFriendly() && targetDefinition.IsEnemy())
                {
                    if (attackType != AttackType.Melee || IsMeleeReachable(squad, context.GridSlots))
                    {
                        validTargets.Add(squad);
                    }
                }
                else if (actor.Unit.Definition.IsEnemy() && targetDefinition.IsFriendly())
                {
                    if (attackType != AttackType.Melee || IsMeleeReachable(squad, context.GridSlots))
                    {
                        validTargets.Add(squad);
                    }
                }
            }

            return validTargets;
        }

        private static bool IsMeleeReachable(SquadModel targetSquad, IReadOnlyList<BattleGridController.BattleGridSlot> gridSlots)
        {
            if (gridSlots == null)
            {
                return true;
            }

            var targetSlot = gridSlots.FirstOrDefault(slot => slot?.Squad == targetSquad);
            if (targetSlot == null)
            {
                // ÷ель вне сетки Ч считаем достижимой
                return true;
            }

            // ≈сли цель стоит во фронтальном р€ду Ч всегда достижима
            if (targetSlot.Type == BattleGridSlotType.Front)
            {
                return true;
            }

            // ---- Ћогика дл€ заднего р€да ----

            // »ндекс фронтального слота, который блокирует позицию цели
            int frontIndex = targetSlot.Index + 3;

            // »щем этот фронтальный слот
            var blockingSlot = gridSlots.FirstOrDefault(slot =>
                slot != null &&
                slot.Index == frontIndex &&
                slot.Type == BattleGridSlotType.Front &&
                slot.Side == targetSlot.Side
            );

            // ≈сли соответствующего слота нет Ч подр€д вступление невозможно, считаем недостижимым
            if (blockingSlot == null)
            {
                return false;
            }

            // ≈сли фронтальный слот пустой Ч цель достижима
            return blockingSlot.IsEmpty;
        }
    }
}
