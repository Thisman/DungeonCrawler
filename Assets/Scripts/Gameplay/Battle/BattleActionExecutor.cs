// Executes battle actions and triggers related squad animations.
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using DungeonCrawler.Systems.Battle;
using System.Linq;
using System.Threading.Tasks;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleActionExecutor
    {
        private readonly GameEventBus _eventBus;
        private readonly UnitSystem _unitSystem;

        public BattleActionExecutor(GameEventBus eventBus, UnitSystem unitSystem)
        {
            _eventBus = eventBus;
            _unitSystem = unitSystem;
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
            var animationController = GetAnimationController(plan.Actor, context);
            if (animationController != null)
            {
                await animationController.PlayWaitAnimation();
            }
        }

        private async Task HandleSkipTurnAsync(PlannedUnitAction plan, BattleContext context)
        {
            var animationController = GetAnimationController(plan.Actor, context);
            if (animationController != null)
            {
                await animationController.PlaySkipTurnAnimation();
            }
        }

        private Task HandleAttackAsync(PlannedUnitAction plan, BattleContext context)
        {
            // здесь только абстрактный каркас, без конкретного урона
            // DamageSystem.ApplyAttack(action.Actor, action.Targets, context);
            return Task.CompletedTask;
        }

        private Task HandleAbilityAsync(PlannedUnitAction plan, BattleContext context)
        {
            // AbilitySystem.Execute(action.Definition, action.Actor, action.Targets, context);
            return Task.CompletedTask;
        }

        private SquadAnimationController GetAnimationController(UnitModel actor, BattleContext context)
        {
            if (actor == null || context?.Squads == null || _unitSystem == null)
            {
                return null;
            }

            var squad = context.Squads.FirstOrDefault(squadModel => squadModel.Unit == actor);
            var controller = _unitSystem.GetController(squad);
            return controller?.AnimationController;
        }
    }

}
