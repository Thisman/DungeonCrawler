// Boots a dungeon scene by spawning the player prefab and seeding their army squads from inspector data.
using DungeonCrawler.Gameplay.Dungeon;
using DungeonCrawler.Systems.Input;
using DungeonCrawler.Systems.Session;
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
        }

        private void InitializePlayer()
        {
            Debug.Assert(_playerPrefab != null, "Player prefab didn't set!");

            _player = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
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
            _gameInputSystem.EnterDungeon();
        }
    }
}
