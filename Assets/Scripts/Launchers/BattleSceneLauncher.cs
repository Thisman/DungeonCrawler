// Boots the battle scene by building squads from inspector data and running the battle state machine.
using System;
using System.Collections.Generic;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
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

        private BattleStateMachine _stateMachine;
        private GameEventBus _sceneEventBus;

        public GameEventBus SceneEventBus => _sceneEventBus;

        public BattleState CurrentBattleState => _stateMachine?.CurrentState ?? _initialState;

        private void Awake()
        {
            var context = new BattleContext(BuildSquads());
            var logger = _useLogging ? new BattleLogger() : null;

            _sceneEventBus = new GameEventBus();
            _stateMachine = new BattleStateMachine(context, _sceneEventBus, logger);
        }

        private void Start()
        {
            _stateMachine.Start();
        }

        private void OnDestroy()
        {
            _stateMachine?.Stop();
        }

        private IEnumerable<SquadModel> BuildSquads()
        {
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
                yield return new SquadModel(unitModel, config.UnitCount);
            }
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
