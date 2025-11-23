// Represents a runtime combat unit by aggregating definition, stats, abilities, effects, and affiliation metadata.
using System;

namespace DungeonCrawler.Gameplay.Unit
{
    public class UnitModel
    {
        public string Id { get; }

        public UnitDefinition Definition { get; }

        public UnitStats Stats { get; }

        public UnitModel(string id, UnitDefinition definition, UnitStats stats) {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Unit id cannot be null or whitespace.", nameof(id));
            }

            Definition = definition;
            Stats = stats;

            Id = id;
        }
    }
}
