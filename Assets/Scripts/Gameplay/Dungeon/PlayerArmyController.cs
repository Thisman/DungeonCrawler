// Stores the player's squads and accepts initialization data from scene launchers.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [DisallowMultipleComponent]
    public class PlayerArmyController : MonoBehaviour
    {
        public List<SquadModel> Squads { get; private set; } = new();

        public void SetSquads(List<SquadModel> squads)
        {
            Squads = squads ?? new List<SquadModel>();
        }
    }
}
