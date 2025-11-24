using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace DungeonCrawler.Gameplay.Battle
{
    public class PlayerController : IBattleController
    {
        private readonly GameEventBus _sceneEventBus;

        public PlayerController(GameEventBus sceneEventBus)
        {
            _sceneEventBus = sceneEventBus;
        }

        public Task<PlannedUnitAction> DecideActionAsync(
            UnitModel actor,
            BattleContext context,
            CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<PlannedUnitAction>();

            // 1. Экшен по умолчанию — атака
            UnitAction currentAction = new UnitAttackAction();
            IReadOnlyList<UnitModel> currentValidTargets = currentAction.GetValidTargets(actor, context);

            // Дать знать UI, что сейчас выбрана атака
            _sceneEventBus.Publish(new RequestSelectAction(currentAction));

            IDisposable clickSubscription = null;
            IDisposable skipActionSubscription = null;
            IDisposable waitActionSubscription = null;

            void Cleanup()
            {
                clickSubscription?.Dispose();
                skipActionSubscription?.Dispose();
                waitActionSubscription?.Dispose();
            }

            // 2. Клик по юниту -> проверяем, что он валидный, и завершаем планирование
            clickSubscription = _sceneEventBus.Subscribe<RequestSelectTarget>(evt =>
            {
                if (tcs.Task.IsCompleted)
                    return;

                var target = evt.Target;
                if (!currentValidTargets.Contains(target.Unit))
                    return; // кликнули по невалидной цели — игнорируем

                var chosenTargets = new List<UnitModel> { target.Unit };
                var planned = new PlannedUnitAction(currentAction, actor, chosenTargets);

                Cleanup();
                tcs.TrySetResult(planned);
            });

            // 3. Нажатие "Пропустить ход"
            skipActionSubscription = _sceneEventBus.Subscribe<RequestSkipTurnAction>(evt =>
            {
                if (tcs.Task.IsCompleted)
                    return;

                var action = new UnitSkipTurnAction();
                var validTargets = action.GetValidTargets(actor, context);
                var chosenTargets = ChooseTargets(action, validTargets, actor, context);

                var planned = new PlannedUnitAction(action, actor, chosenTargets);

                Cleanup();
                tcs.TrySetResult(planned);
            });

            // 4. Нажатие "Подождать"
            waitActionSubscription = _sceneEventBus.Subscribe<RequestWaitAction>(evt =>
            {
                if (tcs.Task.IsCompleted)
                    return;

                var action = new UnitWaitAction();
                var validTargets = action.GetValidTargets(actor, context);
                var chosenTargets = ChooseTargets(action, validTargets, actor, context);

                var planned = new PlannedUnitAction(action, actor, chosenTargets);

                Cleanup();
                tcs.TrySetResult(planned);
            });

            // 5. Отмена (например, стейт-машина завершила бой)
            cancellationToken.Register(() =>
            {
                Cleanup();
                tcs.TrySetCanceled(cancellationToken);
            });

            return tcs.Task;
        }

        private IReadOnlyList<UnitModel> ChooseTargets(
            UnitAction action,
            IReadOnlyList<UnitModel> validTargets,
            UnitModel actor,
            BattleContext context)
        {

            switch (action.Type)
            {
                case ActionType.Attack:
                    {
                        // цель с минимальным здоровьем
                        var best = validTargets.OrderBy(t => t.Stats.CurrentHealth).First();
                        return new List<UnitModel>() { best };
                    }
                case ActionType.SkipTurn:
                    {
                        return new List<UnitModel>();
                    }
                case ActionType.Wait:
                    {
                        return new List<UnitModel>();
                    }
                case ActionType.Ability:
                    {
                        // для MVP просто выбираем первую цель
                        var target = validTargets.First();
                        return new List<UnitModel>() { target };
                    }
            }

            return new List<UnitModel>();
        }
    }
}