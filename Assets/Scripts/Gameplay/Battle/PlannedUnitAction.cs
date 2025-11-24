using DungeonCrawler.Gameplay.Unit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public sealed class PlannedUnitAction
    {
        public UnitAction Action { get; }

        public UnitModel Actor { get; }

        public IReadOnlyList<UnitModel> Targets { get; }

        public PlannedUnitAction(UnitAction action, UnitModel actor, IReadOnlyList<UnitModel> targets) {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Targets = targets ?? Array.Empty<UnitModel>();
        }
    }
}
