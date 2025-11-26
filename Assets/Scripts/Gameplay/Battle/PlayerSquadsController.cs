// Handles player decision making for selecting squad actions during battle.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;

namespace DungeonCrawler.Gameplay.Battle
{
    public class PlayerSquadsController : IBattleController
    {
        private readonly GameEventBus _sceneEventBus;

        public PlayerSquadsController(GameEventBus sceneEventBus)
        {
            _sceneEventBus = sceneEventBus;
        }

        public Task<PlannedUnitAction> DecideActionAsync(
            SquadModel actor,
            BattleContext context,
            CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<PlannedUnitAction>();

            UnitAction currentAction = new UnitAttackAction();
            IReadOnlyList<SquadModel> currentValidTargets = currentAction.GetValidTargets(actor, context);

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

            clickSubscription = _sceneEventBus.Subscribe<RequestSelectTarget>(evt =>
            {
                if (tcs.Task.IsCompleted)
                    return;

                var target = evt.Target;
                if (!currentValidTargets.Contains(target))
                    return;

                var chosenTargets = new List<SquadModel> { target };
                var planned = new PlannedUnitAction(currentAction, actor, chosenTargets);

                Cleanup();
                tcs.TrySetResult(planned);
            });

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

            cancellationToken.Register(() =>
            {
                Cleanup();
                tcs.TrySetCanceled(cancellationToken);
            });

            return tcs.Task;
        }

        private IReadOnlyList<SquadModel> ChooseTargets(
            UnitAction action,
            IReadOnlyList<SquadModel> validTargets,
            SquadModel actor,
            BattleContext context)
        {
            switch (action.Type)
            {
                case ActionType.Attack:
                {
                    var best = validTargets.OrderBy(t => t.Unit.Stats.CurrentHealth).First();
                    return new List<SquadModel>() { best };
                }
                case ActionType.SkipTurn:
                {
                    return new List<SquadModel>();
                }
                case ActionType.Wait:
                {
                    return new List<SquadModel>();
                }
                case ActionType.Ability:
                {
                    var target = validTargets.First();
                    return new List<SquadModel>() { target };
                }
            }

            return new List<SquadModel>();
        }
    }
}
