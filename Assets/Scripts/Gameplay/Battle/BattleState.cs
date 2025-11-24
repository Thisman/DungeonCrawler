using System.Collections;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public enum BattleState
    {
        None,
        Preparation,
        RoundInit,
        RoundStart,
        TurnInit,
        TurnStart,
        WaitForAction,
        TurnEnd,
        RoundEnd,
        Result,
        Finish,
    }
}