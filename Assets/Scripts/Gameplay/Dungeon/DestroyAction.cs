// Destroys the scenario controller's game object to remove the associated scenario effect.
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "Destroy", menuName = "Scenario/Destroy")]
    public class DestroyAction : ScenarioAction
    {
        public override Task<ActionResult> Execute(ScenarioController controller)
        {
            if (controller == null)
            {
                return Task.FromResult(ActionResult.Continue);
            }

            Object.Destroy(controller.gameObject);
            return Task.FromResult(ActionResult.Break);
        }
    }
}
