using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonCrawler.Systems.Input
{
    public class GameInputSystem : MonoBehaviour
    {
        public static GameInputSystem Instance { get; private set; }

        public event Action<Vector2> BattleClick;

        [Header("Input System")]
        [SerializeField] private InputActionAsset _inputActions; // сюда кидаешь GameInputActions.asset в инспекторе

        private InputAction _battlePointAction;
        private InputAction _battleClickAction;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ищем action map и actions
            var battleMap = _inputActions.FindActionMap("Battle", throwIfNotFound: true);
            _battlePointAction = battleMap.FindAction("Point", throwIfNotFound: true);
            _battleClickAction = battleMap.FindAction("Click", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _battlePointAction.Enable();
            _battleClickAction.Enable();

            _battleClickAction.performed += OnBattleClickPerformed;
        }

        private void OnDisable()
        {
            _battleClickAction.performed -= OnBattleClickPerformed;

            _battlePointAction.Disable();
            _battleClickAction.Disable();
        }

        private void OnBattleClickPerformed(InputAction.CallbackContext ctx)
        {
            // Берём текущую позицию указателя
            var screenPos = _battlePointAction.ReadValue<Vector2>();
            BattleClick?.Invoke(screenPos);
        }

        // Опционально: включение/отключение карты "Battle" по состоянию боя
        public void EnableBattleControls()
        {
            _battlePointAction.Enable();
            _battleClickAction.Enable();
        }

        public void DisableBattleControls()
        {
            _battlePointAction.Disable();
            _battleClickAction.Disable();
        }
    }
}
