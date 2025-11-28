// Defines the base contract for scriptable scenario actions executed by controllers.
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public enum ActionResult
    {
        Continue,
        Break
    }

    public abstract class ScenarioAction : ScriptableObject
    {
        public abstract Task<ActionResult> Execute(ScenarioController controller);
    }
}
