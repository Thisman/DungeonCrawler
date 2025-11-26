// Rotates the player sprite to match movement direction and applies the army's lead icon.
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [DisallowMultipleComponent]
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField]
        private PlayerMoveController _moveController;

        [SerializeField]
        private PlayerArmyController _playerArmyController;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        private bool _isFacingLeft;

        private void Awake()
        {
            if (_moveController == null)
            {
                _moveController = GetComponent<PlayerMoveController>();
            }

            if (_playerArmyController == null)
            {
                _playerArmyController = GetComponent<PlayerArmyController>();
            }

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _isFacingLeft = _spriteRenderer != null && _spriteRenderer.flipX;
        }

        private void Start()
        {
            if (_playerArmyController == null)
            {
                _playerArmyController = GetComponent<PlayerArmyController>();
            }

            TryApplySquadIcon();
        }

        private void LateUpdate()
        {
            if (_moveController == null || _spriteRenderer == null)
            {
                return;
            }

            var direction = _moveController.MovementDirection;

            if (Mathf.Abs(direction.x) > Mathf.Epsilon)
            {
                _isFacingLeft = direction.x < 0f;
                _spriteRenderer.flipX = !_isFacingLeft;
            }
            else if (direction.y != 0f)
            {
                _spriteRenderer.flipX = !_isFacingLeft;
            }
        }

        private void TryApplySquadIcon()
        {
            if (_spriteRenderer == null || _playerArmyController?.Squads == null || _playerArmyController.Squads.Count == 0)
            {
                return;
            }

            var leadSquad = _playerArmyController.Squads[0];
            var icon = leadSquad?.Unit?.Definition?.Icon;

            if (icon != null)
            {
                _spriteRenderer.sprite = icon;
            }
        }
    }
}
