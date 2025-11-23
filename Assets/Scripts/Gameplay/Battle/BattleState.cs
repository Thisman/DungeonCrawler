using System.Collections;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public enum BattleState
    {
        Preparation,
        RoundInit,
        RoundStart,
        TurnInit,
        TurnStart,
        WaitForAction,
        TurnEnd,
        RoundEnd,
        Result
    }
}