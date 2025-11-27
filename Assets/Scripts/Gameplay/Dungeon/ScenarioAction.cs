// Defines the base contract for scriptable scenario actions executed by controllers.
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public abstract class ScenarioAction : ScriptableObject
    {
        public abstract Task Execute();
    }
}
