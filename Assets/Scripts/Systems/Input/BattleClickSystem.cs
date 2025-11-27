using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace DungeonCrawler.Systems.Input
{
    public class BattleClickSystem : MonoBehaviour
    {
        public event Action<Vector2> OnClick;

        [Inject]
        private readonly InputActionAsset _inputActions;

        private InputAction _battlePointAction;
        private InputAction _battleClickAction;

        private void Start()
        {
            var battleMap = _inputActions.FindActionMap("Battle", throwIfNotFound: true);
            _battlePointAction = battleMap.FindAction("Point", throwIfNotFound: true);
            _battleClickAction = battleMap.FindAction("Click", throwIfNotFound: true);
            _battleClickAction.performed += OnBattleClickPerformed;
        }

        private void OnDisable()
        {
            _battleClickAction.performed -= OnBattleClickPerformed;
        }

        private void OnBattleClickPerformed(InputAction.CallbackContext ctx)
        {
            // Берём текущую позицию указателя
            var screenPos = _battlePointAction.ReadValue<Vector2>();
            OnClick?.Invoke(screenPos);
        }
    }
}
