using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DungeonCrawler.Gameplay.Battle
{
    public class AIController : IBattleController
    {
        private readonly IReadOnlyList<UnitAction> _availableActions;
        private readonly GameEventBus _sceneEventBus;

        public AIController(IReadOnlyList<UnitAction> availableActions, GameEventBus sceneEventBus)
        {
            _availableActions = availableActions;
            _sceneEventBus = sceneEventBus;
        }

        public async Task<PlannedUnitAction> DecideActionAsync(
            UnitModel actor,
            BattleContext context,
            CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            var action = ChooseActionDefinition(actor, context);
            var validTargets = action.GetValidTargets(actor, context);
            var chosenTargets = ChooseTargets(action, validTargets, actor, context);

            var planned = new PlannedUnitAction(action, actor, chosenTargets);
            return planned;
        }

        private UnitAction ChooseActionDefinition(UnitModel actor, BattleContext context) {
            return _availableActions.First(a => a.Type == ActionType.SkipTurn);
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
