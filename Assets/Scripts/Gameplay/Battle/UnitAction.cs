using DungeonCrawler.Gameplay.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public abstract class UnitAction
    {
        public string Id;

        public string Name;

        public ActionType Type;

        public abstract bool CanExecute(UnitModel actor, BattleContext context);

        public abstract IReadOnlyList<UnitModel> GetValidTargets(UnitModel actor, BattleContext context);
    }
}
