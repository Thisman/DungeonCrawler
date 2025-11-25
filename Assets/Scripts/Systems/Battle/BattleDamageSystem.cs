// Resolves attack actions into damage instances without applying them.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using UnityEngine;

namespace DungeonCrawler.Systems.Battle
{
    public class BattleDamageSystem
    {
        private readonly DefenseSettings _defenseSettings;
        private readonly AntiStreakSettings _antiStreakSettings;
        private readonly Dictionary<SquadModel, CritState> _critStates = new();
        private readonly Dictionary<SquadModel, DodgeState> _dodgeStates = new();

        public BattleDamageSystem(): this(DefenseSettings.Default, AntiStreakSettings.Default)
        {
        }

        public BattleDamageSystem(DefenseSettings defenseSettings, AntiStreakSettings antiStreakSettings)
        {
            _defenseSettings = defenseSettings;
            _antiStreakSettings = antiStreakSettings;
        }

        public Task<IReadOnlyList<DamageInstance>> ResolveDamageAsync(PlannedUnitAction plan)
        {
            if (plan?.Actor?.Unit?.Stats == null || plan.Targets == null)
            {
                return Task.FromResult<IReadOnlyList<DamageInstance>>(new List<DamageInstance>());
            }

            var results = new List<DamageInstance>();

            foreach (var target in plan.Targets.Where(target => target?.Unit != null))
            {
                var attackerStats = plan.Actor.Unit.Stats;
                var targetStats = target.Unit.Stats;

                var attackType = attackerStats.DamageType;
                var isMagicAttack = attackType == DamageType.Magical;

                var dodgeState = GetDodgeState(target);
                var dodgeEff = isMagicAttack
                    ? 0f
                    : CalculateEffectiveDodge(targetStats, dodgeState);

                var hitChance = isMagicAttack ? 1f : Mathf.Clamp01(1f - dodgeEff);
                var isHit = Random.value <= hitChance;

                RegisterDodgeResult(dodgeState, wasDodged: !isHit);

                if (!isHit)
                {
                    results.Add(new DamageInstance(plan.Actor, target, 0f, attackerStats.DamageType, false));
                    continue;
                }

                var baseDamage = Random.Range(attackerStats.MinDamage, attackerStats.MaxDamage);

                var critState = GetCritState(plan.Actor);
                var critEff = CalculateEffectiveCrit(attackerStats, critState);
                var isCrit = Random.value <= critEff;

                RegisterCritResult(critState, isCrit);

                var critDamage = isCrit
                    ? baseDamage * Mathf.Max(1f, attackerStats.CritMultiplier)
                    : baseDamage;

                var finalDamage = CalculateFinalDamage(attackType, critDamage, targetStats);
                finalDamage = Mathf.Max(_defenseSettings.MinimumHitDamage, finalDamage);

                var attackerCount = Mathf.Max(0, plan.Actor.UnitCount);
                finalDamage *= attackerCount;

                results.Add(new DamageInstance(plan.Actor, target, finalDamage, attackerStats.DamageType, true));
            }

            return Task.FromResult<IReadOnlyList<DamageInstance>>(results);
        }

        private float CalculateFinalDamage(DamageType attackType, float critDamage, UnitStats targetStats)
        {
            switch (attackType)
            {
                case DamageType.Physical:
                    return ApplyAbsorbReduction(critDamage, targetStats.AbsoluteDefense, targetStats.PhysicalDefense, _defenseSettings.PhysicalDefenseCurveK);
                case DamageType.Magical:
                    return ApplyAbsorbReduction(critDamage, targetStats.AbsoluteDefense, targetStats.MagicDefense, _defenseSettings.MagicDefenseCurveK);
                case DamageType.Pure:
                    return Mathf.Max(0f, critDamage);
                default:
                    return Mathf.Max(0f, critDamage);
            }
        }

        private float ApplyAbsorbReduction(float critDamage, float absoluteDefense, float curveDefense, float defenseCurveK)
        {
            var afterAbsolute = Mathf.Max(0f, critDamage - Mathf.Max(0f, absoluteDefense));
            var absorb = CalculateAbsorb(Mathf.Max(0f, curveDefense), defenseCurveK);
            var reduced = afterAbsolute * (1f - absorb);
            return Mathf.Max(0f, reduced);
        }

        private float CalculateAbsorb(float defense, float curveK)
        {
            if (defense <= 0f)
            {
                return 0f;
            }

            return defense / (defense + Mathf.Max(Mathf.Epsilon, curveK));
        }

        private float CalculateEffectiveDodge(UnitStats targetStats, DodgeState dodgeState)
        {
            var baseChance = NormalizeChance(targetStats.MissChance);
            var successOverflow = Mathf.Max(0, dodgeState.SuccessStreak - _antiStreakSettings.Dodge.SuccessThreshold);
            var failOverflow = Mathf.Max(0, dodgeState.FailStreak - _antiStreakSettings.Dodge.FailThreshold);

            var adjusted = baseChance
                           * (1f - _antiStreakSettings.Dodge.SuccessPenalty * successOverflow)
                           * (1f + _antiStreakSettings.Dodge.FailBonus * failOverflow);

            return Mathf.Clamp(adjusted, _antiStreakSettings.Dodge.MinChance, _antiStreakSettings.Dodge.MaxChance);
        }

        private float CalculateEffectiveCrit(UnitStats attackerStats, CritState critState)
        {
            var baseChance = NormalizeChance(attackerStats.CritChance);
            var successOverflow = Mathf.Max(0, critState.SuccessStreak - _antiStreakSettings.Crit.SuccessThreshold);
            var failOverflow = Mathf.Max(0, critState.FailStreak - _antiStreakSettings.Crit.FailThreshold);

            var adjusted = baseChance
                           * (1f - _antiStreakSettings.Crit.SuccessPenalty * successOverflow)
                           * (1f + _antiStreakSettings.Crit.FailBonus * failOverflow);

            return Mathf.Clamp(adjusted, _antiStreakSettings.Crit.MinChance, _antiStreakSettings.Crit.MaxChance);
        }

        private void RegisterCritResult(CritState critState, bool isCrit)
        {
            if (isCrit)
            {
                critState.SuccessStreak++;
                critState.FailStreak = 0;
                return;
            }

            critState.FailStreak++;
            critState.SuccessStreak = 0;
        }

        private void RegisterDodgeResult(DodgeState dodgeState, bool wasDodged)
        {
            if (wasDodged)
            {
                dodgeState.SuccessStreak++;
                dodgeState.FailStreak = 0;
                return;
            }

            dodgeState.FailStreak++;
            dodgeState.SuccessStreak = 0;
        }

        private CritState GetCritState(SquadModel attacker)
        {
            if (_critStates.TryGetValue(attacker, out var state))
            {
                return state;
            }

            state = new CritState();
            _critStates[attacker] = state;
            return state;
        }

        private DodgeState GetDodgeState(SquadModel target)
        {
            if (_dodgeStates.TryGetValue(target, out var state))
            {
                return state;
            }

            state = new DodgeState();
            _dodgeStates[target] = state;
            return state;
        }

        private float NormalizeChance(float percentValue)
        {
            return Mathf.Clamp01(percentValue * 0.01f);
        }

        private sealed class CritState
        {
            public int SuccessStreak;
            public int FailStreak;
        }

        private sealed class DodgeState
        {
            public int SuccessStreak;
            public int FailStreak;
        }

        public readonly struct DefenseSettings
        {
            public static readonly DefenseSettings Default = new(50f, 50f, 1f);

            public DefenseSettings(float physicalDefenseCurveK, float magicDefenseCurveK, float minimumHitDamage)
            {
                PhysicalDefenseCurveK = physicalDefenseCurveK;
                MagicDefenseCurveK = magicDefenseCurveK;
                MinimumHitDamage = minimumHitDamage;
            }

            public float PhysicalDefenseCurveK { get; }

            public float MagicDefenseCurveK { get; }

            public float MinimumHitDamage { get; }
        }

        public readonly struct AntiStreakSettings
        {
            public static readonly AntiStreakSettings Default = new(
                new CritSettings(0.1f, 0.05f, 2, 2, 0.01f, 0.75f),
                new DodgeSettings(0.1f, 0.05f, 2, 2, 0.01f, 0.6f));

            public AntiStreakSettings(CritSettings crit, DodgeSettings dodge)
            {
                Crit = crit;
                Dodge = dodge;
            }

            public CritSettings Crit { get; }

            public DodgeSettings Dodge { get; }
        }

        public readonly struct CritSettings
        {
            public CritSettings(float successPenalty, float failBonus, int successThreshold, int failThreshold, float minChance, float maxChance)
            {
                SuccessPenalty = successPenalty;
                FailBonus = failBonus;
                SuccessThreshold = successThreshold;
                FailThreshold = failThreshold;
                MinChance = minChance;
                MaxChance = maxChance;
            }

            public float SuccessPenalty { get; }

            public float FailBonus { get; }

            public int SuccessThreshold { get; }

            public int FailThreshold { get; }

            public float MinChance { get; }

            public float MaxChance { get; }
        }

        public readonly struct DodgeSettings
        {
            public DodgeSettings(float successPenalty, float failBonus, int successThreshold, int failThreshold, float minChance, float maxChance)
            {
                SuccessPenalty = successPenalty;
                FailBonus = failBonus;
                SuccessThreshold = successThreshold;
                FailThreshold = failThreshold;
                MinChance = minChance;
                MaxChance = maxChance;
            }

            public float SuccessPenalty { get; }

            public float FailBonus { get; }

            public int SuccessThreshold { get; }

            public int FailThreshold { get; }

            public float MinChance { get; }

            public float MaxChance { get; }
        }
    }
}
