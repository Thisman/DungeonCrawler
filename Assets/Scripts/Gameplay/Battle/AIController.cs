// Provides simple AI decision making for selecting squad actions during battle.
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
            SquadModel actor,
            BattleContext context,
            CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            var action = ChooseActionDefinition(actor, context);
            var validTargets = action.GetValidTargets(actor, context);
            var chosenTargets = ChooseTargets(action, validTargets, actor, context);

            var planned = new PlannedUnitAction(action, actor, chosenTargets);
            return planned;
        }

        private UnitAction ChooseActionDefinition(SquadModel actor, BattleContext context)
        {
            return _availableActions.First(a => a.Type == ActionType.SkipTurn);
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
