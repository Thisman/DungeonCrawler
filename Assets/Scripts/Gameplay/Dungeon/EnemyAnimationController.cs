// Applies the lead enemy squad's icon to the sprite renderer for visualization.
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [DisallowMultipleComponent]
    public class EnemyAnimationController : MonoBehaviour
    {
        [SerializeField]
        private EnemyArmyController _enemyArmyController;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            if (_enemyArmyController == null)
            {
                _enemyArmyController = GetComponent<EnemyArmyController>();
            }

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Start()
        {
            if (_enemyArmyController == null)
            {
                _enemyArmyController = GetComponent<EnemyArmyController>();
            }

            TryApplySquadIcon();
        }

        private void TryApplySquadIcon()
        {
            if (_spriteRenderer == null || _enemyArmyController?.Squads == null || _enemyArmyController.Squads.Count == 0)
            {
                return;
            }

            var leadSquad = _enemyArmyController.Squads[0];
            var icon = leadSquad?.Unit?.Definition?.Icon;

            if (icon != null)
            {
                _spriteRenderer.sprite = icon;
            }
        }
    }
}
