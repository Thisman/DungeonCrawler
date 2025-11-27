// Holds and executes a sequence of scenario actions in order.
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField]
        private List<ScenarioAction> _actions = new();

        public IReadOnlyList<ScenarioAction> Actions => _actions;

        public async Task RunAsync()
        {
            foreach (var action in _actions)
            {
                if (action == null)
                {
                    continue;
                }

                await action.Execute();
            }
        }
    }
}
