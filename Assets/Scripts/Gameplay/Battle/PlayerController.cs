// Handles player decision-making, default action selection, and interactive target picking during battle turns.
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

            var defaultAction = new UnitAttackAction();
            _sceneEventBus.Publish(new RequestActionSelect(defaultAction));
            var defaultValidTargets = defaultAction.GetValidTargets(actor, context);

            IDisposable skipActionSubscribtion = null;
            IDisposable waitActionSubscribtion = null;

            skipActionSubscribtion = _sceneEventBus.Subscribe<RequestWaitAction>(evt =>
            {
                var action = new UnitSkipTurnAction();
                var chosenTargets = ChooseTargets(action, action.GetValidTargets(actor, context));

                CompletePlanning(tcs, action, actor, chosenTargets);
                skipActionSubscribtion?.Dispose();
                waitActionSubscribtion?.Dispose();
            });

            waitActionSubscribtion = _sceneEventBus.Subscribe<RequestSkipTurnAction>(evt =>
            {
                var action = new UnitWaitAction();
                var chosenTargets = ChooseTargets(action, action.GetValidTargets(actor, context));

                CompletePlanning(tcs, action, actor, chosenTargets);
                waitActionSubscribtion?.Dispose();
                skipActionSubscribtion?.Dispose();
            });

            cancellationToken.Register(() =>
            {
                skipActionSubscribtion?.Dispose();
                waitActionSubscribtion?.Dispose();
                tcs.TrySetCanceled(cancellationToken);
            });

            _ = ChooseTargetsAsync(defaultAction, defaultValidTargets, actor, tcs, cancellationToken);

            return tcs.Task;
        }

        private void CompletePlanning(
            TaskCompletionSource<PlannedUnitAction> tcs,
            UnitAction action,
            UnitModel actor,
            IReadOnlyList<UnitModel> chosenTargets)
        {
            if (tcs.Task.IsCompleted)
            {
                return;
            }

            var planned = new PlannedUnitAction(action, actor, chosenTargets);
            tcs.TrySetResult(planned);
        }

        private async Task ChooseTargetsAsync(
            UnitAction action,
            IReadOnlyList<UnitModel> validTargets,
            UnitModel actor,
            TaskCompletionSource<PlannedUnitAction> tcs,
            CancellationToken cancellationToken)
        {
            if (action.Type != ActionType.Attack)
            {
                CompletePlanning(tcs, action, actor, ChooseTargets(action, validTargets));
                return;
            }

            while (!cancellationToken.IsCancellationRequested && !tcs.Task.IsCompleted)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var target = TryGetTargetUnderCursor(validTargets);
                    if (target != null)
                    {
                        CompletePlanning(tcs, action, actor, new List<UnitModel> { target });
                        break;
                    }
                }

                await Task.Yield();
            }
        }

        private UnitModel TryGetTargetUnderCursor(IReadOnlyList<UnitModel> validTargets)
        {
            var camera = Camera.main;
            if (camera == null || validTargets == null)
            {
                return null;
            }

            var screenPoint = Input.mousePosition;
            screenPoint.z = -camera.transform.position.z;
            var worldPoint = (Vector2)camera.ScreenToWorldPoint(screenPoint);

            var hit = Physics2D.OverlapPoint(worldPoint, LayerMask.GetMask("Unit"));
            var squadController = hit ? hit.GetComponentInParent<SquadController>() : null;
            var targetModel = squadController?.Model?.Unit;

            return validTargets?.FirstOrDefault(target => target == targetModel || target?.Id == targetModel?.Id);
        }

        private IReadOnlyList<UnitModel> ChooseTargets(
            UnitAction action,
            IReadOnlyList<UnitModel> validTargets)
        {
            switch (action.Type)
            {
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
                        var target = validTargets.First();
                        return new List<UnitModel>() { target };
                    }
            }

            return new List<UnitModel>();
        }
    }
}
