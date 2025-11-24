using System.Collections;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public enum TargetMode
    {
        None,
        Self,
        SingleAlly,
        MultipleAlly,
        AllAlly,
        SingleEnemy,
        MultipleEnemy,
        AllEnemy,
        Custom,
    }
}