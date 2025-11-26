// Holds battle participants, queue, lifecycle state, and captured result data.
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
            CurrentRoundNumber = 0;
            Result = new BattleResult(Squads);
        }

        public List<SquadModel> Squads { get; }

        public IReadOnlyList<BattleGridController.BattleGridSlot> GridSlots { get; set; }

        public BattleQueue Queue { get; set; }

        public SquadModel ActiveUnit { get; set; }

        public BattleStatus Status { get; set; }

        public PlannedUnitAction PlannedActiion { get; set; }

        public int CurrentRoundNumber { get; set; }

        public BattleResult Result { get; }
    }
}
