// Stores the enemy's squads and allows initialization from scene setup.
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [DisallowMultipleComponent]
    public class EnemyArmyController : MonoBehaviour
    {
        [SerializeField]
        private List<BattleSceneLauncher.SquadConfig> _configs = new();

        public List<SquadModel> Squads { get; private set; } = new();

        private void Awake()
        {
            Squads = BuildSquads();
        }

        private List<SquadModel> BuildSquads()
        {
            var squads = new List<SquadModel>(_configs.Count);

            foreach (var config in _configs)
            {
                if (config == null || config.Definition == null)
                {
                    Debug.LogWarning("Squad config is missing a UnitDefinition and will be skipped.");
                    continue;
                }

                var id = string.IsNullOrWhiteSpace(config.Id)
                    ? GenerateUnitId(config.Definition)
                    : config.Id.Trim();

                var unitModel = new UnitModel(id, config.Definition, new UnitStats(config.Definition, config.Level));
                squads.Add(new SquadModel(unitModel, config.UnitCount));
            }

            return squads;
        }

        private static string GenerateUnitId(UnitDefinition definition)
        {
            return $"{definition.Name}_{Guid.NewGuid():N}";
        }
    }
}
