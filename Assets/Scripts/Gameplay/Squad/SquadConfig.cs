using DungeonCrawler.Gameplay.Unit;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Squad
{
    [Serializable]
    public class SquadConfig
    {
        public UnitDefinition Definition;

        public string Id;

        [Min(1)]
        public int UnitCount = 1;

        [Min(1)]
        public int Level = 1;
    }
}
