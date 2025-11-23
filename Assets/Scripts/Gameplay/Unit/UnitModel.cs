// Represents a runtime combat unit by aggregating definition, stats, abilities, effects, and affiliation metadata.
using System;
using System.Collections.Generic;

namespace DungeonCrawler.Gameplay.Unit
{
    public class UnitModel
    {
        private readonly Dictionary<string, AbilityState> _abilities = new();
        private readonly List<EffectState> _activeEffects = new();

        public string Id { get; }
        public UnitDefinition Definition { get; }
        public UnitStats Stats { get; }
        public string SideId { get; }
        public string SquadId { get; }

        public IReadOnlyCollection<AbilityState> Abilities => _abilities.Values;
        public IReadOnlyCollection<EffectState> ActiveEffects => _activeEffects.AsReadOnly();

        public UnitModel(
            string id,
            UnitDefinition definition,
            UnitStats stats,
            string sideId = "",
            string squadId = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Unit id cannot be null or whitespace.", nameof(id));
            }

            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));

            Id = id;
            SideId = sideId ?? string.Empty;
            SquadId = squadId ?? string.Empty;
        }

        public void AddAbility(AbilityState ability)
        {
            if (ability == null)
            {
                throw new ArgumentNullException(nameof(ability));
            }

            if (_abilities.ContainsKey(ability.Id))
            {
                throw new InvalidOperationException($"Ability with id '{ability.Id}' is already registered for unit {Id}.");
            }

            _abilities.Add(ability.Id, ability);
        }

        public bool TryGetAbility(string abilityId, out AbilityState ability)
        {
            return _abilities.TryGetValue(abilityId, out ability);
        }

        public void SetAbilityCooldown(string abilityId, int cooldown)
        {
            if (!_abilities.TryGetValue(abilityId, out var ability))
            {
                throw new KeyNotFoundException($"Ability '{abilityId}' is not registered for unit {Id}.");
            }

            ability.SetCooldown(cooldown);
        }

        public void TickAbilityCooldowns(int delta = 1)
        {
            foreach (var ability in _abilities.Values)
            {
                ability.ReduceCooldown(delta);
            }
        }

        public void AddEffect(EffectState effect)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            _activeEffects.Add(effect);
        }

        public void TickEffects(int delta = 1)
        {
            for (var i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.Tick(delta);

                if (effect.IsExpired)
                {
                    _activeEffects.RemoveAt(i);
                }
            }
        }
    }

    public class AbilityState
    {
        public string Id { get; }
        public int Cooldown { get; private set; }
        public bool IsReady => Cooldown <= 0;

        public AbilityState(string id, int cooldown = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Ability id cannot be null or whitespace.", nameof(id));
            }

            Id = id;
            Cooldown = Math.Max(0, cooldown);
        }

        public void SetCooldown(int cooldown)
        {
            Cooldown = Math.Max(0, cooldown);
        }

        public void ReduceCooldown(int delta = 1)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "Delta must be non-negative.");
            }

            Cooldown = Math.Max(0, Cooldown - delta);
        }
    }

    public class EffectState
    {
        public string Id { get; }
        public int RemainingDuration { get; private set; }
        public bool IsExpired => RemainingDuration <= 0;

        public EffectState(string id, int duration)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Effect id cannot be null or whitespace.", nameof(id));
            }

            if (duration < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be non-negative.");
            }

            Id = id;
            RemainingDuration = duration;
        }

        public void Tick(int delta = 1)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "Delta must be non-negative.");
            }

            RemainingDuration = Math.Max(0, RemainingDuration - delta);
        }
    }
}
