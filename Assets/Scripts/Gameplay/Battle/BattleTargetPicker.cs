// Picks battle targets on pointer input and publishes selection events through the game event bus.
using Assets.Scripts.Gameplay.Battle;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Systems.Input;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleTargetPicker : MonoBehaviour
    {
        [SerializeField] private LayerMask unitLayerMask;

        private GameEventBus _eventBus;
        private GameInputSystem _inputSystem;

        public void Initialize(GameEventBus eventBus)
        {
            _eventBus = eventBus;
            _inputSystem = GameInputSystem.Instance;
            if (_inputSystem != null)
            {
                _inputSystem.BattleClick += OnBattleClick;
            }
        }

        private void OnDisable()
        {
            if (_inputSystem != null)
            {
                _inputSystem.BattleClick -= OnBattleClick;
            }
        }

        private void OnBattleClick(Vector2 screenPosition)
        {
            if (_eventBus == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var worldPoint = camera.ScreenToWorldPoint(screenPosition);
            worldPoint.z = 0f;

            var hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, unitLayerMask);
            if (!hit.collider)
            {
                return;
            }

            var squadController = hit.collider.GetComponentInParent<SquadController>();
            var targetModel = squadController.Model;
            if (targetModel == null)
            {
                return;
            }

            _eventBus.Publish(new RequestSelectTarget(targetModel));
        }
    }
}
