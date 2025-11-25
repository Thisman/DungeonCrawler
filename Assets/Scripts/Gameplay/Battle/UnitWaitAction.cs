// Represents a wait action that moves the squad to the end of the current round.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public class UnitWaitAction : UnitAction
    {
        public UnitWaitAction()
        {
            Name = "Wait";
            Id = "Wait";
            Type = ActionType.Wait;
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
