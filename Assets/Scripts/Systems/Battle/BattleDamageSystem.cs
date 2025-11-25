// Resolves attack actions into damage instances without applying them.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DungeonCrawler.Gameplay.Battle;
using UnityEngine;

namespace DungeonCrawler.Systems.Battle
{
    public class BattleDamageSystem
    {
        public Task<IReadOnlyList<DamageInstance>> ResolveDamageAsync(PlannedUnitAction plan)
        {
            if (plan?.Actor?.Stats == null || plan.Targets == null)
            {
                return Task.FromResult<IReadOnlyList<DamageInstance>>(new List<DamageInstance>());
            }

            var results = new List<DamageInstance>();

            foreach (var target in plan.Targets.Where(target => target != null))
            {
                var isHit = Random.Range(0f, 100f) >= plan.Actor.Stats.MissChance;
                var amount = isHit
                    ? Random.Range(plan.Actor.Stats.MinDamage, plan.Actor.Stats.MaxDamage)
                    : 0f;

                results.Add(new DamageInstance(plan.Actor, target, amount, plan.Actor.Stats.DamageType, isHit));
            }

            return Task.FromResult<IReadOnlyList<DamageInstance>>(results);
        }
    }
}
