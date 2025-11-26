// Boots a dungeon scene by spawning the player prefab and seeding their army squads from inspector data.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Player;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class DungeonSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private List<BattleSceneLauncher.SquadConfig> _squads = new();

        [SerializeField]
        private GameObject _playerPrefab;

        private PlayerArmyController _playerArmyController;

        private void Start()
        {
            var squads = BuildSquads();
            InitializePlayer();

            _playerArmyController?.Initialize(squads);
        }

        private void InitializePlayer()
        {
            if (_playerPrefab == null)
            {
                Debug.LogError("Player prefab is not assigned in DungeonLauncher.");
                return;
            }

            var playerInstance = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
            _playerArmyController = playerInstance.GetComponent<PlayerArmyController>()
                ?? playerInstance.GetComponentInChildren<PlayerArmyController>();

            if (_playerArmyController == null)
            {
                Debug.LogError("Spawned player prefab is missing a PlayerArmyController component.");
            }
        }

        private List<SquadModel> BuildSquads()
        {
            var squads = new List<SquadModel>(_squads.Count);

            foreach (var config in _squads)
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
