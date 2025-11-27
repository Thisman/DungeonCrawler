// Detects nearby scenario controllers and runs their actions when the interact input is triggered.
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [DisallowMultipleComponent]
    public class PlayerScenarioController : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _interactAction;

        [SerializeField]
        private float _interactionRadius = 2f;

        private bool _isRunningScenario;

        private void OnEnable()
        {
            if (_interactAction != null)
            {
                _interactAction.action.Enable();
                _interactAction.action.performed += OnInteractPerformed;
            }
        }

        private void OnDisable()
        {
            if (_interactAction != null)
            {
                _interactAction.action.performed -= OnInteractPerformed;
                _interactAction.action.Disable();
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            _ = TryRunScenarioAsync();
        }

        private async Task TryRunScenarioAsync()
        {
            if (_isRunningScenario)
            {
                return;
            }

            var controller = FindClosestScenarioController();
            if (controller == null)
            {
                return;
            }

            _isRunningScenario = true;
            try
            {
                await controller.RunAsync();
            }
            finally
            {
                _isRunningScenario = false;
            }
        }

        private ScenarioController FindClosestScenarioController()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _interactionRadius);
            ScenarioController closest = null;
            var closestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit == null)
                {
                    continue;
                }

                var controller = hit.GetComponentInParent<ScenarioController>();
                if (controller == null)
                {
                    continue;
                }

                var delta = controller.transform.position - transform.position;
                var distance = delta.sqrMagnitude;
                if (distance < closestDistance)
                {
                    closest = controller;
                    closestDistance = distance;
                }
            }

            return closest;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
#endif
    }
}
