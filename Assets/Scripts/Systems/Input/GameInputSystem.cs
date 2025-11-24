// Centralizes input handling with support for enabling mode-specific action sets like battle input.
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonCrawler.Systems.Input
{
    [DefaultExecutionOrder(-5000)]
    public class GameInputSystem : MonoBehaviour
    {
        private static GameInputSystem _instance;

        private readonly InputAction _battleClickAction = new InputAction(
            "BattleClick",
            InputActionType.Button,
            "<Mouse>/leftButton");

        private readonly InputAction _battlePointerAction = new InputAction(
            "BattlePointer",
            InputActionType.PassThrough,
            "<Pointer>/position");

        private InputMode _activeMode = InputMode.None;

        public static GameInputSystem Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = FindFirstObjectByType<GameInputSystem>();
                if (_instance != null)
                {
                    return _instance;
                }

                var inputSystemObject = new GameObject(nameof(GameInputSystem));
                _instance = inputSystemObject.AddComponent<GameInputSystem>();
                DontDestroyOnLoad(inputSystemObject);

                return _instance;
            }
        }

        public event Action<Vector2> BattleClick;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            DisableCurrentMode();
            _battleClickAction.Dispose();
            _battlePointerAction.Dispose();
        }

        public void EnableMode(InputMode mode)
        {
            if (_activeMode == mode)
            {
                return;
            }

            DisableCurrentMode();
            _activeMode = mode;

            switch (mode)
            {
                case InputMode.Battle:
                    EnableBattleMode();
                    break;
                default:
                    break;
            }
        }

        public void DisableMode(InputMode mode)
        {
            if (_activeMode != mode)
            {
                return;
            }

            DisableCurrentMode();
        }

        private void EnableBattleMode()
        {
            _battlePointerAction.Enable();
            _battleClickAction.Enable();
            _battleClickAction.performed += HandleBattleClick;
        }

        private void DisableBattleMode()
        {
            _battleClickAction.performed -= HandleBattleClick;
            _battleClickAction.Disable();
            _battlePointerAction.Disable();
        }

        private void HandleBattleClick(InputAction.CallbackContext context)
        {
            if (_activeMode != InputMode.Battle || BattleClick == null)
            {
                return;
            }

            var screenPosition = _battlePointerAction.ReadValue<Vector2>();
            BattleClick?.Invoke(screenPosition);
        }

        private void DisableCurrentMode()
        {
            switch (_activeMode)
            {
                case InputMode.Battle:
                    DisableBattleMode();
                    break;
                default:
                    break;
            }

            _activeMode = InputMode.None;
        }
    }
}
