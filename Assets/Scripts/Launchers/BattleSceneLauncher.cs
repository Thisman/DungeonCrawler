// Boots the battle scene by building squads from inspector data, laying out squads, and running the battle state machine.
using System;
using System.Collections.Generic;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using DungeonCrawler.Systems.Battle;
using DungeonCrawler.UI.Battle;
using DungeonCrawler.UI.Common;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class BattleSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private List<SquadConfig> _squads = new List<SquadConfig>();

        [SerializeField]
        private BattleState _initialState = BattleState.Preparation;

        [SerializeField]
        private bool _useLogging = true;

        [SerializeField]
        private BaseUIController[] _uiPanels;

        [SerializeField]
        private SquadController _squadPrefab;

        [SerializeField]
        private Transform _friendlySquadsRoot;

        [SerializeField]
        private Transform _enemySquadsRoot;

        [SerializeField]
        private int _unitsPerRow = 3;

        [SerializeField]
        private Vector2 _squadSpacing = new Vector2(1.5f, 1.5f);

        [SerializeField]
        private BattleTargetPicker _battleTargetPicker;

        private BattleStateMachine _stateMachine;
        private UnitSystem _unitSystem;
        private GameEventBus _sceneEventBus;

        public GameEventBus SceneEventBus => _sceneEventBus;

        public BattleState CurrentBattleState => _stateMachine?.CurrentState ?? _initialState;

        private void Awake()
        {
            var squads = BuildSquads();
            _sceneEventBus = new GameEventBus();
            _unitSystem = new UnitSystem(_squadPrefab, transform, _friendlySquadsRoot, _enemySquadsRoot, _unitsPerRow, _squadSpacing);
            _unitSystem.InitializeSquads(squads);

            var context = new BattleContext(squads);
            var logger = _useLogging ? new BattleLogger() : null;

            _stateMachine = new BattleStateMachine(context, _sceneEventBus, logger);
            _battleTargetPicker.Initialize(_sceneEventBus);

            InitializeUIPanels();
        }

        private void Start()
        {
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

        [Serializable]
        public class SquadConfig
        {
            [Tooltip("Definition of the unit this squad represents.")]
            public UnitDefinition Definition;

            [Tooltip("Custom identifier for the unit model. If empty, a generated id will be used.")]
            public string Id;

            [Min(1)]
            [Tooltip("Number of units in the squad.")]
            public int UnitCount = 1;

            [Min(1)]
            [Tooltip("Starting level for the unit stats.")]
            public int Level = 1;
        }
    }
}
