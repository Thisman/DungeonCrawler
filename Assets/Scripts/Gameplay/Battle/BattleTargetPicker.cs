// Picks battle targets on pointer input and publishes selection events through the game event bus.
using Assets.Scripts.Gameplay.Battle;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Systems.Input;
using UnityEngine;
using VContainer;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleTargetPicker : MonoBehaviour
    {
        [SerializeField] private LayerMask unitLayerMask;

        [Inject]
        private readonly GameEventBus _eventBus;

        [Inject]
        private readonly BattleClickSystem _battleClickSystem;

        private void Start()
        {
            if (_battleClickSystem != null)
            {
                _battleClickSystem.OnClick += OnBattleClick;
            }
        }

        private void OnDisable()
        {
            if (_battleClickSystem != null)
            {
                _battleClickSystem.OnClick -= OnBattleClick;
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
