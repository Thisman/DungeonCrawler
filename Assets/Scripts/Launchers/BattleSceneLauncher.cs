// Boots the battle scene by building squads from inspector data, laying out squads, and running the battle state machine.
using Assets.Scripts.Gameplay.Battle;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Systems.Battle;
using DungeonCrawler.Systems.Input;
using DungeonCrawler.Systems.Session;
using DungeonCrawler.Systems.SceneManagement;
using DungeonCrawler.UI.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using DungeonCrawler.Gameplay.Battle;

namespace DungeonCrawler.Launchers
{
    [DisallowMultipleComponent]
    public class BattleSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private BaseUIController[] _uiPanels;

        [SerializeField]
        private SquadController _squadPrefab;

        [Inject] private readonly UnitSystem _unitSystem;
        [Inject] private readonly BattleContext _context;
        [Inject] private readonly GameEventBus _sceneEventBus;
        [Inject] private readonly BattleStateMachine _stateMachine;
        [Inject] private readonly GameInputSystem _gameInputSystem;
        [Inject] private readonly GameSessionSystem _gameSessionSystem;
        [Inject] private readonly SceneLoaderSystem _sceneLoaderSystem;

        private IDisposable _finishBattleSubscription;

        private void Start()
        {
            InitializeUIPanels();
            InitializeInputSystem();

            var buildedSquads = new List<SquadModel>();
            buildedSquads.AddRange(_gameSessionSystem.PlayerSquads);
            buildedSquads.AddRange(_gameSessionSystem.EnemiesSquads);

            _context.Squads = buildedSquads;
            _context.Status = BattleStatus.Preparation;
            _context.Result = new BattleResult(buildedSquads);

            _unitSystem.Initalize(buildedSquads, _squadPrefab);
            _stateMachine.Start();
        }

        private void OnEnable()
        {
            _finishBattleSubscription ??=
                _sceneEventBus.Subscribe<RequestFinishBattle>(HandleRequestFinishBattle);
        }

        private void OnDisable()
        {
            _finishBattleSubscription?.Dispose();
            _finishBattleSubscription = null;
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
                if (uiPanel != null)
                    uiPanel.Initialize(_sceneEventBus);
            }
        }

        private void InitializeInputSystem()
        {
            // Переключаем инпут в боевой режим.
            _gameInputSystem.EnterBattle();
        }

        private void HandleRequestFinishBattle(RequestFinishBattle request)
        {
            var result = request?.Result ?? _context?.Result;
            _ = _sceneLoaderSystem.UnloadAdditiveScene(gameObject.scene.name, result);
        }
    }
}
