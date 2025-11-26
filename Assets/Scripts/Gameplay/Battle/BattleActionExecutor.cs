// Executes battle actions and triggers related squad animations.
using Assets.Scripts.Gameplay.Battle;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Systems.Battle;
using System.Linq;
using System.Threading.Tasks;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleActionExecutor
    {
        private readonly GameEventBus _eventBus;
        private readonly UnitSystem _unitSystem;
        private readonly BattleDamageSystem _battleDamageSystem;

        public BattleActionExecutor(GameEventBus eventBus, UnitSystem unitSystem, BattleDamageSystem battleDamageSystem)
        {
            _eventBus = eventBus;
            _unitSystem = unitSystem;
            _battleDamageSystem = battleDamageSystem;
        }

        public async Task ExecuteAsync(PlannedUnitAction plan, BattleContext context)
        {
            switch (plan.Action.Type)
            {
                case ActionType.SkipTurn:
                    await HandleSkipTurnAsync(plan, context);
                    break;

                case ActionType.Wait:
                    await HandleWaitAsync(plan, context);
                    break;

                case ActionType.Attack:
                    await HandleAttackAsync(plan, context);
                    break;

                case ActionType.Ability:
                    await HandleAbilityAsync(plan, context);
                    break;
            }
        }

        private async Task HandleWaitAsync(PlannedUnitAction plan, BattleContext context)
        {
            var squadController = GetSquadController(plan.Actor, context);
            if (squadController != null)
            {
                await squadController.Wait();
            }

            context?.Queue?.MoveToCurrentRoundEnd(context.ActiveUnit);
        }

        private async Task HandleSkipTurnAsync(PlannedUnitAction plan, BattleContext context)
        {
            var squadController = GetSquadController(plan.Actor, context);
            if (squadController != null)
            {
                await squadController.SkipTurn();
            }
        }

        private async Task HandleAttackAsync(PlannedUnitAction plan, BattleContext context)
        {
            var damageInstances = await _battleDamageSystem?.ResolveDamageAsync(plan);

            foreach (var damage in damageInstances)
            {
                await _unitSystem.ApplyDamage(damage);
            }
        }

        private Task HandleAbilityAsync(PlannedUnitAction plan, BattleContext context)
        {
            // AbilitySystem.Execute(action.Definition, action.Actor, action.Targets, context);
            return Task.CompletedTask;
        }

        private SquadController GetSquadController(SquadModel actor, BattleContext context)
        {
            if (actor == null || context?.Squads == null || _unitSystem == null)
            {
                return null;
            }

            return _unitSystem.GetController(actor);
        }
    }

}
