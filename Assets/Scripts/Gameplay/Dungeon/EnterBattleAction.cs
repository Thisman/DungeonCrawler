// Requests entering a battle for the scenario controller's enemy squads and waits until the battle ends.
using DungeonCrawler.Core.EventBus;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "EnterBattle", menuName = "Scenario/Enter Battle")]
    public class EnterBattleAction : ScenarioAction
    {
        public override async Task Execute(ScenarioController controller)
        {
            if (controller == null)
            {
                return;
            }

            var sceneEventBus = controller.SceneEventBus;
            if (sceneEventBus == null)
            {
                Debug.LogWarning("EnterBattleAction: Scene event bus is not available.");
                return;
            }

            var enemyArmy = controller.GetComponent<EnemyArmyController>();
            if (enemyArmy == null)
            {
                Debug.LogWarning("EnterBattleAction: EnemyArmyController was not found on the scenario controller.");
                return;
            }

            var battleEndedTask = new TaskCompletionSource<BattleEnded>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var battleEndedSubscription = sceneEventBus.Subscribe<BattleEnded>(evt => battleEndedTask.TrySetResult(evt));

            sceneEventBus.Publish(new RequestEnterBattle(enemyArmy.Squads));
            await battleEndedTask.Task.ConfigureAwait(false);
        }
    }
}
