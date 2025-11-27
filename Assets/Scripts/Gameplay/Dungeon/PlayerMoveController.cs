// Moves the player character based on directional input from the new Input System.
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using VContainer;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMoveController : MonoBehaviour
    {

        [SerializeField]
        private float _moveSpeed = 5f;

        [Inject]
        private readonly InputActionAsset _actions;

        private Rigidbody2D _rigidbody2D;
        private InputAction _moveAction;

        public Vector2 MovementDirection { get; private set; }

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _moveAction = _actions.FindAction("Player/Move", true);
        }

        private void OnDisable()
        {
            MovementDirection = Vector2.zero;
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            MovementDirection = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        }

        private void FixedUpdate()
        {
            var clampedInput = Vector2.ClampMagnitude(MovementDirection, 1f);
            _rigidbody2D.linearVelocity = clampedInput * _moveSpeed;
        }
    }
}
