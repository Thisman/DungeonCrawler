// Represents a special ability action placeholder for future mechanics.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public class UnitAbilityAction : UnitAction
    {
        public UnitAbilityAction()
        {
            Name = "Ability";
            Id = "Ability";
            Type = ActionType.Ability;
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
