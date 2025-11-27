// Boots a dungeon scene by spawning the player prefab and seeding their army squads from inspector data.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Dungeon;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using DungeonCrawler.Systems.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace DungeonCrawler.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class DungeonSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private List<SquadConfig> _squads = new();

        [SerializeField]
        private GameObject _playerPrefab;

        [Inject]
        private readonly GameInputSystem _gameInputSystem;

        private void Start()
        {
            InitializeInputSystem();
            InitializePlayer();
        }

        private void InitializePlayer()
        {
            Debug.Assert(_playerPrefab != null, "Player prefab didn't set!");

            var playerInstance = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
            var playerArmyController = playerInstance.GetComponent<PlayerArmyController>();

            Debug.Assert(playerArmyController != null, "PlayerArmyController not found!");

            var squads = BuildSquads();
            playerArmyController.Initialize(squads);
        }

        private void InitializeInputSystem()
        {
            _gameInputSystem.EnterDungeon();
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
