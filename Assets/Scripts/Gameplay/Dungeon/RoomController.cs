using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomController : MonoBehaviour
    {
        public bool HasNorthExit = false;
        public bool HasSouthExit = false;
        public bool HasWestExit = false;
        public bool HasEastExit = false;

        [SerializeField]
        private GameObject _northExit;

        [SerializeField]
        private GameObject _southExit;

        [SerializeField]
        private GameObject _westExit;

        [SerializeField]
        private GameObject _eastExit;

        [SerializeField]
        private BoxCollider2D _collider;

        public Vector2 Center => _collider.bounds.center;

        private void Awake()
        {
            UpdateExits();
        }

        public void ConfigureExits(bool hasNorth, bool hasSouth, bool hasWest, bool hasEast)
        {
            HasNorthExit = hasNorth;
            HasSouthExit = hasSouth;
            HasWestExit = hasWest;
            HasEastExit = hasEast;

            UpdateExits();
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
    }
}
