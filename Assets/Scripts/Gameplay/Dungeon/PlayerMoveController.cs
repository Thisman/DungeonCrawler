using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

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
        _moveAction = _actions.FindAction("Dungeon/Move", true);
    }

    private void Update()
    {
        var a = 1 + 1;
        var b = a + 1;
    }

    private void OnEnable()
    {
        if (_moveAction == null)
        {
            _moveAction = _actions.FindAction("Dungeon/Move", true);
        }

        if (_moveAction != null)
        {
            _moveAction.started += OnMove;
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
        }
    }

    private void OnDisable()
    {
        if (_moveAction != null)
        {
            _moveAction.started -= OnMove;
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
        }

        MovementDirection = Vector2.zero;
        _rigidbody2D.linearVelocity = Vector2.zero;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Started:
            case InputActionPhase.Performed:
                MovementDirection = ctx.ReadValue<Vector2>();
                break;

            case InputActionPhase.Canceled:
                MovementDirection = Vector2.zero;
                break;
        }
    }

    private void FixedUpdate()
    {
        var clampedInput = Vector2.ClampMagnitude(MovementDirection, 1f);
        _rigidbody2D.linearVelocity = clampedInput * _moveSpeed;
    }
}
