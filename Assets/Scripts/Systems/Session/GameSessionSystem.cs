// Stores current player and enemy squad rosters for active gameplay sessions.
using System.Collections.Generic;
using System.Linq;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Systems.Session
{
    public class GameSessionSystem
    {
        public List<SquadModel> PlayerSquads { get; private set; } = new();

        public List<SquadModel> EnemiesSquads { get; private set; } = new();

        public void SetPlayerSquads(IEnumerable<SquadModel> squads)
        {
            PlayerSquads = squads?.Where(s => s != null).ToList() ?? new List<SquadModel>();
        }

        public void SetEnemySquads(IEnumerable<SquadModel> squads)
        {
            EnemiesSquads = squads?.Where(s => s != null).ToList() ?? new List<SquadModel>();
        }
    }
}
