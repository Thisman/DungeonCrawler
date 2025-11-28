// Holds and executes a sequence of scenario actions in order.
using DungeonCrawler.Core.EventBus;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField]
        private List<ScenarioAction> _actions = new();

        [Inject]
        private readonly GameEventBus _sceneEventBus;

        public IReadOnlyList<ScenarioAction> Actions => _actions;

        public GameEventBus SceneEventBus => _sceneEventBus;

        public async Task RunAsync()
        {
            foreach (var action in _actions)
            {
                if (action == null)
                {
                    continue;
                }

                var result = await action.Execute(this);
                if (result == ActionResult.Break)
                {
                    break;
                }
            }
        }
    }
}
