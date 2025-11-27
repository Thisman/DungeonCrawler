// Emits a configured debug message when the action is executed.
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "DebugAction", menuName = "Scenario/Debug Action")]
    public class DebugAction : ScenarioAction
    {
        [TextArea]
        [SerializeField]
        private string _message = "Scenario debug action executed.";

        public override Task Execute()
        {
            Debug.Log(_message);
            return Task.CompletedTask;
        }
    }
}
