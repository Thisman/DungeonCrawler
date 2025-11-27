// Seeds the initial session squads and loads configured additive scenes at application start.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using DungeonCrawler.Systems.SceneManagement;
using DungeonCrawler.Systems.Session;
using UnityEngine;
using VContainer;

namespace DungeonCrawler.Launchers
{
    [DisallowMultipleComponent]
    public class RootSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private List<SquadConfig> _squadsConfig;

        [SerializeField]
        private List<string> _additiveScenes = new();

        [Inject]
        private readonly GameSessionSystem _gameSessionSystem;

        [Inject]
        private readonly SceneLoaderSystem _sceneLoaderSystem;

        private void Start()
        {
            ApplySquadConfig();
            LoadAdditiveScenes();
        }

        private void ApplySquadConfig()
        {
            Debug.Assert(_squadsConfig != null, "Squad configs did'n set!");

            _gameSessionSystem.SetPlayerSquads(BuildSquads(_squadsConfig));
        }

        private void LoadAdditiveScenes()
        {
            foreach (var sceneName in _additiveScenes)
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    continue;
                }

                _sceneLoaderSystem.LoadAdditiveScene(sceneName.Trim());
            }
        }

        private static List<SquadModel> BuildSquads(List<SquadConfig> configs)
        {
            var squads = new List<SquadModel>();

            if (configs == null)
            {
                return squads;
            }

            foreach (var config in configs)
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
