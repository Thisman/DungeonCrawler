using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using System.Collections;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleTargetPicker : MonoBehaviour
    {
        [SerializeField] private LayerMask unitLayerMask;

        private GameEventBus _eventBus;

        public void Initialize(GameEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Update()
        {
            if (_eventBus == null)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            var camera = Camera.main;
            if (camera == null)
                return;

            var worldPoint = camera.ScreenToWorldPoint(Input.mousePosition);
            worldPoint.z = 0f;

            var hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, unitLayerMask);
            if (!hit.collider)
                return;

            var squadController = hit.collider.GetComponentInParent<SquadController>();
            var targetModel = squadController.Model;
            if (targetModel == null)
                return;

            _eventBus.Publish(new RequestSelectTarget(targetModel));
        }
    }
}