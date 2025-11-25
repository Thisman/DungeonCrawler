// Represents a skip turn action that immediately ends the squad's current turn.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public class UnitSkipTurnAction : UnitAction
    {
        public UnitSkipTurnAction()
        {
            Name = "SkipTurn";
            Id = "SkipTurn";
            Type = ActionType.SkipTurn;
        }

        public override bool CanExecute(SquadModel actor, BattleContext context)
        {
            return true;
        }

        public override IReadOnlyList<SquadModel> GetValidTargets(SquadModel actor, BattleContext context)
        {
            return new List<SquadModel>();
        }
    }
}
