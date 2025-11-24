using DungeonCrawler.Gameplay.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
