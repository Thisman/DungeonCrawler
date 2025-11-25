// Represents an action chosen by a squad including its actor and targets.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public sealed class PlannedUnitAction
    {
        public UnitAction Action { get; }

        public SquadModel Actor { get; }

        public IReadOnlyList<SquadModel> Targets { get; }

        public PlannedUnitAction(UnitAction action, SquadModel actor, IReadOnlyList<SquadModel> targets)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Targets = targets ?? Array.Empty<SquadModel>();
        }
    }
}
