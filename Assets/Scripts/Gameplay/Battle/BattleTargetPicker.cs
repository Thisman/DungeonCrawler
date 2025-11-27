// Picks battle targets on pointer input and publishes selection events through the game event bus.
using Assets.Scripts.Gameplay.Battle;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Systems.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleTargetPicker : MonoBehaviour
    {
        [SerializeField] private LayerMask unitLayerMask;

        [Inject]
        private readonly GameEventBus _eventBus;

        [Inject]
        private readonly InputActionAsset _inputActions;

        private InputAction _PointAction;
        private InputAction _ClickAction;

        private void Start()
        {
            var battleMap = _inputActions.FindActionMap("Battle", throwIfNotFound: true);
            _PointAction = battleMap.FindAction("Point", throwIfNotFound: true);
            _ClickAction = battleMap.FindAction("Click", throwIfNotFound: true);
            _ClickAction.performed += HandleClick;
        }

        private void OnDisable()
        {
            _ClickAction.performed -= HandleClick;
        }

        private void HandleClick(InputAction.CallbackContext ctx)
        {
            var camera = Camera.main;
            var screenPosition = _PointAction.ReadValue<Vector2>();
            var worldPoint = camera.ScreenToWorldPoint(screenPosition);
            worldPoint.z = 0f;

            var hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, unitLayerMask);
            if (!hit.collider)
            {
                return;
            }

            var squadController = hit.collider.GetComponentInParent<SquadController>();
            var targetModel = squadController.Model;
            _eventBus.Publish(new RequestSelectTarget(targetModel));
        }
    }
}
