// Boots the battle scene by building squads from inspector data, laying out squads, and running the battle state machine.
using System;
using System.Collections.Generic;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
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
        private GameEventBus _sceneEventBus;

        public GameEventBus SceneEventBus => _sceneEventBus;

        public BattleState CurrentBattleState => _stateMachine?.CurrentState ?? _initialState;

        private void Awake()
        {
            var squads = BuildSquads();
            var context = new BattleContext(squads);
            var logger = _useLogging ? new BattleLogger() : null;

            _sceneEventBus = new GameEventBus();
            _stateMachine = new BattleStateMachine(context, _sceneEventBus, logger);
            _battleTargetPicker.Initialize(_sceneEventBus);

            ArrangeSquads(squads);
            InitializeUIPanels();
        }

        private void Start()
        {
            _stateMachine.Start();
        }

        private void OnDestroy()
        {
            _stateMachine?.Stop();
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

        private void ArrangeSquads(IReadOnlyList<SquadModel> squads)
        {
            if (_squadPrefab == null)
            {
                Debug.LogWarning("Squad prefab is not assigned; squad layout will be skipped.");
                return;
            }

            if (squads == null)
            {
                return;
            }

            var friendlyIndex = 0;
            var enemyIndex = 0;

            foreach (var squad in squads)
            {
                var isEnemy = squad.Unit.Definition.IsEnemy();
                var parent = isEnemy ? _enemySquadsRoot : _friendlySquadsRoot;
                var squadInstance = Instantiate(_squadPrefab, parent ? parent : transform);
                var slotIndex = isEnemy ? enemyIndex++ : friendlyIndex++;

                squadInstance.transform.localPosition = CalculateLocalPosition(slotIndex, isEnemy);
                squadInstance.Initalize(squad);
            }
        }

        private Vector3 CalculateLocalPosition(int index, bool isEnemy)
        {
            var slotsPerRow = Mathf.Max(1, _unitsPerRow);
            var row = index / slotsPerRow;
            var column = index % slotsPerRow;
            var direction = isEnemy ? 1f : -1f;

            return new Vector3(direction * column * _squadSpacing.x, -row * _squadSpacing.y, 0f);
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
