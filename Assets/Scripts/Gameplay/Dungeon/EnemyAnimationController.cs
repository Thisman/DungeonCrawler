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

        [SerializeField]
        private SpriteRenderer[] _mirrors;

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
            TryApplySquadIcon(_spriteRenderer);
            foreach (var item in _mirrors)
            {
                TryApplySquadIcon(item);
            }
        }

        public void ShowMirror(DungeonSide side) {
            var mirrorIndex = side switch
            {
                DungeonSide.North => 0,
                DungeonSide.South => 1,
                DungeonSide.West => 2,
                DungeonSide.East => 3,
                _ => -1,
            };

            _mirrors[mirrorIndex].gameObject.SetActive(true);
        }

        private void TryApplySquadIcon(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null || _enemyArmyController.Squads == null || _enemyArmyController.Squads.Count == 0)
            {
                return;
            }

            var leadSquad = _enemyArmyController.Squads[0];
            var icon = leadSquad?.Unit.Definition.Icon;

            if (icon != null)
            {
                spriteRenderer.sprite = icon;
            }
        }
    }
}
