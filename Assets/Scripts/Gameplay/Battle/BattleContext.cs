// Holds battle participants, queue, lifecycle state, and captured result data.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleContext
    {
        public BattleContext()
        {
            CurrentRoundNumber = 0;
        }

        public List<SquadModel> Squads { get; set; }

        public IReadOnlyList<BattleGridController.BattleGridSlot> GridSlots { get; set; }

        public BattleQueue Queue { get; set; }

        public SquadModel ActiveUnit { get; set; }

        public BattleStatus Status { get; set; }

        public PlannedUnitAction PlannedActiion { get; set; }

        public int CurrentRoundNumber { get; set; }

        public BattleResult Result { get; set; }
    }
}
