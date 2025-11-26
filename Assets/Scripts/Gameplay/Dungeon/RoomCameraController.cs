// Moves the camera to the center of the room trigger when the player enters it.
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [RequireComponent(typeof(Collider2D))]
    public class RoomCameraController : MonoBehaviour
    {
        [SerializeField] private Collider2D _trigger;

        private Camera _camera;

        private void Awake()
        {
            if (_trigger == null)
            {
                _trigger = GetComponent<Collider2D>();
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            var cameraToMove = _camera != null ? _camera : Camera.main;
            if (cameraToMove == null || _trigger == null)
            {
                return;
            }

            var targetPosition = (Vector3)_trigger.bounds.center;
            targetPosition.z = cameraToMove.transform.position.z;
            cameraToMove.transform.position = targetPosition;
        }
    }
}
