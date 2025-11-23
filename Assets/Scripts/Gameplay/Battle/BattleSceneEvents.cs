using DungeonCrawler.Gameplay.Battle;
using System.Collections;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public record BattleStateChanged(BattleState FromState, BattleState ToState, BattleContext Context);

    public record RequestBattlePreparationFinish();
}