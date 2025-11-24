using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Unit;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public interface IBattleController
    {
        Task<PlannedUnitAction> DecideActionAsync(
            UnitModel actor,
            BattleContext context,
            CancellationToken cancellationToken);
    }
}