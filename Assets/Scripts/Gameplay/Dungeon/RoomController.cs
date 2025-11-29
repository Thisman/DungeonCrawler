// Manages room exits visibility and positions spawned objects near exits.
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomController : MonoBehaviour
    {
        [Header("Exits State")]
        public bool HasNorthExit = false;
        public bool HasSouthExit = false;
        public bool HasWestExit = false;
        public bool HasEastExit = false;

        [Header("Exit Placeholders (walls / door frames)")]
        [SerializeField]
        private GameObject _northExit;

        [SerializeField]
        private GameObject _southExit;

        [SerializeField]
        private GameObject _westExit;

        [SerializeField]
        private GameObject _eastExit;

        [Header("Exiy enemy spawns (walls / door frames)")]
        [SerializeField]
        private GameObject _northEnemySpawn;

        [SerializeField]
        private GameObject _southEnemySpawn;

        [SerializeField]
        private GameObject _westEnemySpawn;

        [SerializeField]
        private GameObject _eastEnemySpawn;

        [Header("Room Collider")]
        [SerializeField]
        private BoxCollider2D _collider;

        public Vector2 Center => _collider.bounds.center;

        private void Awake()
        {
            UpdateExits();
        }

        /// <summary>
        /// Configures which exits are open and updates visual placeholders.
        /// </summary>
        public void ConfigureExits(bool hasNorth, bool hasSouth, bool hasWest, bool hasEast)
        {
            HasNorthExit = hasNorth;
            HasSouthExit = hasSouth;
            HasWestExit = hasWest;
            HasEastExit = hasEast;

            UpdateExits();
        }

        /// <summary>
        /// Places target object at the specified exit, on the inner boundary of the exit's collider.
        /// </summary>
        public void PlaceObjectAtExit(GameObject target, DungeonSide side)
        {
            if (target == null || _collider == null)
                return;

            if (!IsExitOpen(side))
                return;

            var targetTransform = target.transform;
            targetTransform.SetParent(transform, worldPositionStays: true);

            var exitPosition = GetPositionForExit(side);
            targetTransform.position = new Vector3(exitPosition.x, exitPosition.y, targetTransform.position.z);
        }

        private void UpdateExits()
        {
            // Предполагаем, что объекты _northExit/_southExit/... – это "заглушки" (стены),
            // которые должны быть активны, когда прохода НЕТ.
            if (_northExit != null)
                _northExit.SetActive(!HasNorthExit);

            if (_southExit != null)
                _southExit.SetActive(!HasSouthExit);

            if (_westExit != null)
                _westExit.SetActive(!HasWestExit);

            if (_eastExit != null)
                _eastExit.SetActive(!HasEastExit);
        }

        private bool IsExitOpen(DungeonSide exit)
        {
            return exit switch
            {
                DungeonSide.North => HasNorthExit,
                DungeonSide.South => HasSouthExit,
                DungeonSide.West => HasWestExit,
                DungeonSide.East => HasEastExit,
                _ => false
            };
        }

        private GameObject GetEnemySpawnForExit(DungeonSide exit)
        {
            return exit switch
            {
                DungeonSide.North => _northEnemySpawn,
                DungeonSide.South => _southEnemySpawn,
                DungeonSide.West => _westEnemySpawn,
                DungeonSide.East => _eastEnemySpawn,
                _ => null
            };
        }

        /// <summary>
        /// Returns a world position near the specified exit, at the inner boundary of the exit's collider.
        /// </summary>
        private Vector2 GetPositionForExit(DungeonSide exit)
        {
            var exitObj = GetEnemySpawnForExit(exit);
            if (exitObj == null)
                return Center;

            var exitCollider = exitObj.GetComponentInChildren<Collider2D>();
            if (exitCollider == null)
            {
                // Фолбэк — просто позиция выхода
                return exitObj.transform.position;
            }

            var bounds = exitCollider.bounds;

            return exit switch
            {
                // Северный выход: точка на нижней границе выхода (ближе к центру комнаты)
                DungeonSide.North => new Vector2(bounds.center.x, bounds.min.y),

                // Южный выход: на верхней границе выхода
                DungeonSide.South => new Vector2(bounds.center.x, bounds.max.y),

                // Западный: на правой границе выхода
                DungeonSide.West => new Vector2(bounds.max.x, bounds.center.y),

                // Восточный: на левой границе выхода
                DungeonSide.East => new Vector2(bounds.min.x, bounds.center.y),

                _ => Center
            };
        }
    }
}
