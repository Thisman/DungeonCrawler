// Holds battle participants, queue, and lifecycle state.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleContext
    {
        public BattleContext(IEnumerable<SquadModel> squads = null, BattleStatus status = BattleStatus.Preparation)
        {
            Squads = squads != null ? new List<SquadModel>(squads) : new List<SquadModel>();
            Status = status;
        }

        public List<SquadModel> Squads { get; }

        public BattleQueue Queue { get; set; }

        public SquadModel ActiveUnit { get; set; }

        public BattleStatus Status { get; set; }
    }
}
