using DungeonCrawler.Core.EventBus;
using System.Threading.Tasks;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleActionExecutor : IBattleActionExecutor
    {
        private readonly GameEventBus _eventBus;

        public BattleActionExecutor(GameEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(PlannedUnitAction plan, BattleContext context)
        {
            switch (plan.Action.Type)
            {
                case ActionType.SkipTurn:
                    break;

                case ActionType.Wait:
                    await HandleWait(plan, context);
                    break;

                case ActionType.Attack:
                    await HandleAttackAsync(plan, context);
                    break;

                case ActionType.Ability:
                    await HandleAbilityAsync(plan, context);
                    break;
            }
        }

        private Task HandleWait(PlannedUnitAction plan, BattleContext context)
        {
            // Переставить юнита в конец текущего раунда
            //context.Queue.MoveToRoundEnd(action.Actor);
            return Task.CompletedTask;

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
    }

}