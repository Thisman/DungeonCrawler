using DungeonCrawler.Gameplay.Battle;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public interface IBattleActionExecutor
    {
        Task ExecuteAsync(PlannedUnitAction action, BattleContext context);
    }

}