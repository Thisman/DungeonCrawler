// Holds battle participants, queue, lifecycle state, and squad placement data for targeting rules.
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
            SquadPlacements = new Dictionary<SquadModel, SquadPlacement>();
        }

        public List<SquadModel> Squads { get; }

        public BattleQueue Queue { get; set; }

        public SquadModel ActiveUnit { get; set; }

        public BattleStatus Status { get; set; }

        public PlannedUnitAction PlannedActiion { get; set; }

        public int CurrentRoundNumber { get; set; }

        public Dictionary<SquadModel, SquadPlacement> SquadPlacements { get; }

        public void SetSquadPlacement(SquadModel squad, bool isEnemySide, BattleRow row)
        {
            if (squad == null)
            {
                return;
            }

            SquadPlacements[squad] = new SquadPlacement(isEnemySide, row);
        }

        public bool TryGetPlacement(SquadModel squad, out SquadPlacement placement)
        {
            if (squad == null)
            {
                placement = default;
                return false;
            }

            return SquadPlacements.TryGetValue(squad, out placement);
        }

        public IReadOnlyList<SquadModel> GetAliveSquadsInRow(bool isEnemySide, BattleRow row)
        {
            var result = new List<SquadModel>();

            foreach (var pair in SquadPlacements)
            {
                if (pair.Key == null || pair.Key.IsDead || pair.Key.IsEmpty())
                {
                    continue;
                }

                if (pair.Value.IsEnemySide == isEnemySide && pair.Value.Row == row)
                {
                    result.Add(pair.Key);
                }
            }

            return result;
        }
    }

    public record SquadPlacement(bool IsEnemySide, BattleRow Row);
}
