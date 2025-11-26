using UnityEngine;
using System.Collections;

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

        public void Awake()
		{
            _northExit.SetActive(!HasNorthExit);
            _southExit.SetActive(!HasSouthExit);
            _westExit.SetActive(!HasWestExit);
            _eastExit.SetActive(!HasEastExit);
		}
	}
}