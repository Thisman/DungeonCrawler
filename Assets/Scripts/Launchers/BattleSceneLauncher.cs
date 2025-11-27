// Boots the battle scene by building squads from inspector data, laying out squads, and running the battle state machine.
using System;
using System.Collections.Generic;
using Assets.Scripts.Gameplay.Battle;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using DungeonCrawler.Systems.Battle;
using DungeonCrawler.Systems.Input;
using DungeonCrawler.UI.Common;
using UnityEngine;
using VContainer;

namespace DungeonCrawler.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class BattleSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private BaseUIController[] _uiPanels;

        [SerializeField]
        private SquadController _squadPrefab;

        [SerializeField]
        private List<SquadConfig> _squads = new();

        [Inject]
        private readonly UnitSystem _unitSystem;

        [Inject]
        private readonly BattleContext _context;

        [Inject]
        private readonly GameEventBus _sceneEventBus;

        [Inject]
        private readonly BattleStateMachine _stateMachine;

        [Inject]
        private readonly GameInputSystem _gameInputSystem;

        private void Start()
        {
            InitializeUIPanels();
            InitalizeInputSystem();

            var buildedSquads = BuildSquads();
            _context.Status = BattleStatus.Preparation;
            _context.Squads = buildedSquads;
            _context.Result = new BattleResult(buildedSquads);

            _unitSystem.Initalize(buildedSquads, _squadPrefab);
            _stateMachine.Start();
        }

        private void OnDestroy()
        {
            _stateMachine?.Stop();
            _unitSystem?.Dispose();
        }

        private void InitializeUIPanels()
        {
            foreach (var uiPanel in _uiPanels)
            {
                uiPanel.Initialize(_sceneEventBus);
            }
        }

        private void InitalizeInputSystem()
        {
            _gameInputSystem.EnterBattle();
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
