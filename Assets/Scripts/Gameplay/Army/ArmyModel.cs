// Manages a collection of squads within capacity limits and exposes change notifications.
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Army
{
    public enum ArmyChangeType
    {
        Added,
        Removed
    }

    public class ArmyModel
    {
        private readonly List<SquadModel> _squads;

        public ArmyModel(IEnumerable<SquadModel> squads, int maxSlots)
        {
            if (maxSlots <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSlots), "Max slots must be greater than zero.");
            }

            if (squads == null)
            {
                throw new ArgumentNullException(nameof(squads));
            }

            _squads = new List<SquadModel>(squads);
            MaxSlots = maxSlots;

            if (_squads.Count > MaxSlots)
            {
                throw new ArgumentException("Initial squads exceed the maximum allowed slots.", nameof(squads));
            }
        }

        public event Action<ArmyModel, ArmyChangeType, IReadOnlyList<SquadModel>> ArmyChanged;

        public int MaxSlots { get; }

        public IReadOnlyList<SquadModel> GetSquads() => _squads.AsReadOnly();

        public SquadModel GetHeroSquad() => _squads.FirstOrDefault(squad => squad.Unit.Definition.Kind == UnitKind.Hero);

        public void AddSquad(SquadModel squad)
        {
            if (squad == null)
            {
                throw new ArgumentNullException(nameof(squad));
            }

            if (_squads.Count >= MaxSlots)
            {
                throw new InvalidOperationException("Army has reached the maximum number of squads.");
            }

            _squads.Add(squad);
            ArmyChanged?.Invoke(this, ArmyChangeType.Added, new[] { squad });
        }

        public bool RemoveSquad(SquadModel squad)
        {
            if (squad == null)
            {
                throw new ArgumentNullException(nameof(squad));
            }

            var removed = _squads.Remove(squad);
            if (removed)
            {
                ArmyChanged?.Invoke(this, ArmyChangeType.Removed, new[] { squad });
            }

            return removed;
        }
    }
}
