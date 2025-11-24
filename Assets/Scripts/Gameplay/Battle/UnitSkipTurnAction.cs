using DungeonCrawler.Gameplay.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public class UnitSkipTurnAction : UnitAction
    {
        public UnitSkipTurnAction() {
            Name = "SkipTurn";
            Id = "SkipTurn";
            Type = ActionType.SkipTurn;
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