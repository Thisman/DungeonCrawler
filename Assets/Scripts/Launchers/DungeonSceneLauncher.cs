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
using VContainer.Unity;

namespace DungeonCrawler.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class DungeonSceneLauncher : MonoBehaviour
    {
        [SerializeField]
        private GameObject _playerPrefab;

        [Inject] private readonly GameInputSystem _gameInputSystem;
        [Inject] private readonly GameSessionSystem _gameSessionSystem;
        [Inject] private readonly SceneLoaderSystem _sceneLoaderSystem;
        [Inject] private readonly GameEventBus _sceneEventBus;
        [Inject] private readonly IObjectResolver _resolver;

        private IDisposable _enterBattleSubscription;
        private GameObject _player;

        private void Start()
        {
            InitializePlayer();
            SetPlayerSquads();
        }

        private void OnEnable()
        {
            SetPlayerSquads();
            SubscribeToBattleRequests();
            InitializeInputSystem();
        }

        private void OnDisable()
        {
            _enterBattleSubscription?.Dispose();
            _enterBattleSubscription = null;
        }

        private void InitializePlayer()
        {
            Debug.Assert(_playerPrefab != null, "Player prefab didn't set!");

            // Родитель тот же, что у Dungeon Root.
            _player = _resolver.Instantiate(
                _playerPrefab,
                transform.position,
                Quaternion.identity,
                transform.parent);
        }

        private void SetPlayerSquads()
        {
            if (_player == null)
                return;

            var playerArmyController = _player.GetComponent<PlayerArmyController>();
            Debug.Assert(playerArmyController != null, "PlayerArmyController not found!");

            playerArmyController.SetSquads(_gameSessionSystem.PlayerSquads);
        }

        private void InitializeInputSystem()
        {
            // Возвращаемся в режим управления данжем.
            _gameInputSystem.EnterDungeon();
        }

        private void SubscribeToBattleRequests()
        {
            _enterBattleSubscription ??=
                _sceneEventBus.SubscribeAsync<RequestEnterBattle>(HandleEnterBattleAsync);
        }

        private async Task HandleEnterBattleAsync(RequestEnterBattle request)
        {
            if (request == null)
                return;

            // ВАЖНО: без ConfigureAwait(false), чтобы продолжение шло по Unity main thread.
            await RunBattleAsync(request.Enemies);
        }

        private async Task RunBattleAsync(List<SquadModel> enemies)
        {
            if (enemies == null)
                return;

            _gameSessionSystem.SetEnemySquads(enemies);

            var currentSceneName = gameObject.scene.name;

            // Загружаем сцену битвы аддитивно и запоминаем, что после выгрузки нужно вернуться в currentSceneName.
            var battleHandle = _sceneLoaderSystem.LoadAdditiveScene("Gamplay_BattleScene", currentSceneName);

            SceneUnloadResult unloadResult;
            try
            {
                // Без ConfigureAwait(false) — продолжение в Unity main thread.
                unloadResult = await battleHandle.WhenUnloaded;
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
                _gameSessionSystem.SetPlayerSquads(
                    battleResult
                        .GetPlayerSquads()
                        .Select(result => result.Squad));
            }

            // На этом этапе:
            // - Сцена битвы выгружена
            // - Dungeon Root снова активен (через SceneLoaderSystem)
            // Здесь можно безопасно переинициализировать инпут под данж.
            InitializeInputSystem();

            _gameSessionSystem.SetEnemySquads(Array.Empty<SquadModel>());
            _sceneEventBus.Publish(new BattleEnded(battleResult));
        }
    }
}
