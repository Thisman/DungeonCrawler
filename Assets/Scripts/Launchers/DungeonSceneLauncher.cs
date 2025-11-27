// Boots a dungeon scene by spawning the player prefab and seeding their army squads from inspector data.
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Dungeon;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Systems.Input;
using DungeonCrawler.Systems.SceneManagement;
using DungeonCrawler.Systems.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonCrawler.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class DungeonSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private GameObject _playerPrefab;

        [Inject]
        private readonly GameInputSystem _gameInputSystem;

        [Inject]
        private readonly GameSessionSystem _gameSessionSystem;

        [Inject]
        private readonly SceneLoaderSystem _sceneLoaderSystem;

        [Inject]
        private readonly GameEventBus _sceneEventBus;

        private IDisposable _enterBattleSubscription;

        private GameObject _player;

        private void Start()
        {
            InitializePlayer();
            SetPlayerSquads();
        }

        private void OnEnable()
        {
            InitializeInputSystem();
            SetPlayerSquads();
            SubscribeToBattleRequests();
        }

        private void OnDisable()
        {
            _enterBattleSubscription?.Dispose();
            _enterBattleSubscription = null;
        }

        private void InitializePlayer()
        {
            Debug.Assert(_playerPrefab != null, "Player prefab didn't set!");

            _player = Instantiate(_playerPrefab, transform.position, Quaternion.identity, transform.parent);
        }

        private void SetPlayerSquads()
        {
            if (_player == null)
            {
                return;
            }

            var playerArmyController = _player.GetComponent<PlayerArmyController>();

            Debug.Assert(playerArmyController != null, "PlayerArmyController not found!");

            playerArmyController.SetSquads(_gameSessionSystem.PlayerSquads);
        }

        private void InitializeInputSystem()
        {
            // OnEnable runs only on the initial activation of this component; when the battle scene is loaded
            // additively the launcher stays enabled, so we need to explicitly switch the input map back to
            // the dungeon here after the battle scene is unloaded.
            _gameInputSystem.EnterDungeon();
        }

        private void SubscribeToBattleRequests()
        {
            _enterBattleSubscription ??= _sceneEventBus.SubscribeAsync<RequestEnterBattle>(HandleEnterBattleAsync);
        }

        private async Task HandleEnterBattleAsync(RequestEnterBattle request)
        {
            if (request == null)
            {
                return;
            }

            await RunBattleAsync(request.Enemies).ConfigureAwait(false);
        }

        private async Task RunBattleAsync(List<SquadModel> enemies)
        {
            if (enemies == null)
            {
                return;
            }

            _gameSessionSystem.SetEnemySquads(enemies);
            var currentSceneName = gameObject.scene.name;
            var battleHandle = _sceneLoaderSystem.LoadAdditiveScene("Gamplay_BattleScene", currentSceneName);

            SceneUnloadResult unloadResult;
            try
            {
                unloadResult = await battleHandle.WhenUnloaded.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.LogError($"DungeonSceneLauncher: Failed to unload battle scene. {exception}");
                _gameSessionSystem.SetEnemySquads(Array.Empty<SquadModel>());
                _sceneEventBus.Publish(new BattleEnded(null));
                return;
            }

            var battleResult = unloadResult.Data as BattleResult;

            if (battleResult != null)
            {
                _gameSessionSystem.SetPlayerSquads(battleResult.GetPlayerSquads().Select(result => result.Squad));
            }

            _gameSessionSystem.SetEnemySquads(Array.Empty<SquadModel>());
            _gameInputSystem.EnterDungeon();
            _sceneEventBus.Publish(new BattleEnded(battleResult));
        }
    }
}
