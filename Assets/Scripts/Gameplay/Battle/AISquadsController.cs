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
    public class AISquadsController : IBattleController
    {
        private readonly IReadOnlyList<UnitAction> _availableActions;
        private readonly GameEventBus _sceneEventBus;
        private readonly Random _random = new Random();

        public AISquadsController(IReadOnlyList<UnitAction> availableActions, GameEventBus sceneEventBus)
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
            var action = ChooseActionDefinition();
            var validTargets = action.GetValidTargets(actor, context);
            var chosenTargets = ChooseTargets(action, validTargets, actor, context);

            var planned = new PlannedUnitAction(action, actor, chosenTargets);
            return planned;
        }

        private UnitAction ChooseActionDefinition()
        {
            return _availableActions.First(a => a.Type == ActionType.Attack);
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
                    var roll = _random.NextDouble();

                    if (roll < 0.7)
                    {
                        var lowestHealth = validTargets
                            .OrderBy(t => t.Unit.Stats.CurrentHealth)
                            .First();

                        return new List<SquadModel> { lowestHealth };
                    }

                    if (roll < 0.85)
                    {
                        var highestInitiative = validTargets
                            .OrderByDescending(t => t.Unit.Stats.Initiative)
                            .First();

                        return new List<SquadModel> { highestInitiative };
                    }

                    var highestTotalHealth = validTargets
                        .OrderByDescending(t => t.CurrentTotalHealth)
                        .First();

                    return new List<SquadModel> { highestTotalHealth };
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
