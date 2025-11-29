// Creates enemy instances with scaled squads based on dungeon progression.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public class EnemyGenerator : MonoBehaviour
    {
        [SerializeField]
        private GameObject _enemyPrefab;

        [SerializeField]
        private UnitDefinition[] _unitPool = Array.Empty<UnitDefinition>();

        [SerializeField, Min(1)]
        private int _minUnitsPerSquad = 1;

        [SerializeField, Min(1)]
        private int _maxUnitsPerSquad = 4;

        [SerializeField, Range(0f, 1f)]
        private float _strongerEnemyChanceStep = 0.05f;

        [Inject]
        private readonly IObjectResolver _resolver;

        public GameObject CreateEnemy(int roomIndex, Transform parent)
        {
            if (_enemyPrefab == null)
            {
                Debug.LogWarning("[EnemyGenerator] Enemy prefab is not assigned.");
                return null;
            }

            var unitDefinition = GetRandomUnitDefinition();

            if (unitDefinition == null)
            {
                Debug.LogWarning("[EnemyGenerator] No valid UnitDefinition found for enemy creation.");
                return null;
            }

            var enemy = _resolver.Instantiate(_enemyPrefab, parent);

            ConfigureArmy(enemy, unitDefinition, roomIndex);

            return enemy;
        }

        private void ConfigureArmy(GameObject enemy, UnitDefinition unitDefinition, int roomIndex)
        {
            var armyController = enemy.GetComponent<EnemyArmyController>();

            if (armyController == null)
            {
                Debug.LogWarning("[EnemyGenerator] Spawned enemy is missing EnemyArmyController component.");
                return;
            }

            var squadConfig = new SquadConfig
            {
                Definition = unitDefinition,
                Id = string.Empty,
                UnitCount = CalculateUnitCount(roomIndex),
                Level = 1
            };

            armyController.SetConfigs(new List<SquadConfig> { squadConfig });
        }

        private int CalculateUnitCount(int roomIndex)
        {
            var clampedMax = Mathf.Max(_minUnitsPerSquad, _maxUnitsPerSquad);
            var progression = Mathf.Clamp01(roomIndex * _strongerEnemyChanceStep);
            var dynamicMax = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(_minUnitsPerSquad, clampedMax, progression)),
                _minUnitsPerSquad,
                clampedMax);

            return UnityEngine.Random.Range(_minUnitsPerSquad, dynamicMax + 1);
        }

        private UnitDefinition GetRandomUnitDefinition()
        {
            if (_unitPool == null || _unitPool.Length == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            var weightedUnits = new List<(UnitDefinition definition, float weight)>(_unitPool.Length);

            for (int i = 0; i < _unitPool.Length; i++)
            {
                var definition = _unitPool[i];

                if (definition == null)
                {
                    continue;
                }

                var weight = Mathf.Max(1f, _unitPool.Length - i);
                totalWeight += weight;
                weightedUnits.Add((definition, weight));
            }

            if (weightedUnits.Count == 0 || totalWeight <= 0f)
            {
                return null;
            }

            var roll = UnityEngine.Random.value * totalWeight;

            foreach (var (definition, weight) in weightedUnits)
            {
                if (roll <= weight)
                {
                    return definition;
                }

                roll -= weight;
            }

            return weightedUnits[weightedUnits.Count - 1].definition;
        }
    }
}
