// Moves the player character based on directional input from the new Input System.
using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMoveController : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _moveAction;

        [SerializeField]
        private float _moveSpeed = 5f;

        private Rigidbody2D _rigidbody2D;

        public Vector2 MovementDirection { get; private set; }

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _moveAction?.action.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.action.Disable();
            MovementDirection = Vector2.zero;
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            MovementDirection = _moveAction != null ? _moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        }

        private void FixedUpdate()
        {
            var clampedInput = Vector2.ClampMagnitude(MovementDirection, 1f);
            _rigidbody2D.linearVelocity = clampedInput * _moveSpeed;
        }
    }
}
