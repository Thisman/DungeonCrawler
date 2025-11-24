using DungeonCrawler.Gameplay.Unit;
using System.Collections.Generic;

namespace DungeonCrawler.Gameplay.Battle
{
    public class UnitAttackAction : UnitAction
    {
        public UnitAttackAction() {
            Name = "Attack";
            Id = "Attack";
            Type = ActionType.Attack;
        }

        public override bool CanExecute(UnitModel actor, BattleContext context)
        {
            return true;
        }

        public override IReadOnlyList<UnitModel> GetValidTargets(UnitModel actor, BattleContext context)
        {
            return new List<UnitModel>();
        }
    }
}