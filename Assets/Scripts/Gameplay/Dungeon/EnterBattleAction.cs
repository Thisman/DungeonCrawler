using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "EnterBattle", menuName = "Scenario/Enter Battle")]
    public class EnterBattleAction: ScenarioAction
	{
        public override Task Execute(ScenarioController controller)
        {
            return Task.CompletedTask;
        }
    }
}