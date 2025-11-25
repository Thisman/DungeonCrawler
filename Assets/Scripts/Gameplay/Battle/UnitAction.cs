// Defines a unit action contract for squads including execution and targeting rules.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public abstract class UnitAction
    {
        public string Id;

        public string Name;

        public ActionType Type;

        public abstract bool CanExecute(SquadModel actor, BattleContext context);

        public abstract IReadOnlyList<SquadModel> GetValidTargets(SquadModel actor, BattleContext context);
    }
}
