using DungeonCrawler.Gameplay.Dungeon;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonCrawler.Gameplay.Dungeon
{
    class DungeonGenerator: MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject _startRoomPrefab;

        [SerializeField]
        private GameObject _endRoomPrefab;

        [SerializeField]
        private GameObject _defaultRoomPrefab;

        [Header("Main Path Settings")]
        [SerializeField, Min(2)]
        private int _mainPathLength = 8;

        [SerializeField]
        private Vector2 _roomSize = new(18f, 10f); // размер комнаты в мире

        [SerializeField, Range(0f, 1f)]
        private float _keepDirectionProbability = 0.7f; // прямолинейность пути

        [Header("Debug")]
        [SerializeField]
        private bool _generateOnStart = true;

        [Inject]
        private readonly IObjectResolver _resolver;

        private static readonly Vector2Int North = new(0, 1);
        private static readonly Vector2Int South = new(0, -1);
        private static readonly Vector2Int East = new(1, 0);
        private static readonly Vector2Int West = new(-1, 0);

        private static readonly Vector2Int[] Directions =
        {
            East,
            North,
            West,
            South
        };

        private readonly HashSet<Vector2Int> _occupied = new();
        private readonly List<Vector2Int> _mainPathCells = new();

        private void Start()
        {
            if (_generateOnStart)
            {
                Generate();
            }
        }

        public void Generate()
        {
            // На будущее: можно добавить очистку уже заспавненных комнат
            _occupied.Clear();
            _mainPathCells.Clear();

            GenerateMainPath();
            SpawnRoomsWithExits();
        }

        private void GenerateMainPath()
        {
            var current = Vector2Int.zero;
            _mainPathCells.Add(current);
            _occupied.Add(current);

            var currentDirection = East; // стартовое направление

            while (_mainPathCells.Count < _mainPathLength)
            {
                var nextDirection = ChooseNextDirection(currentDirection);
                var nextCell = current + nextDirection;

                // Не допускаем самопересечений
                if (_occupied.Contains(nextCell))
                {
                    bool found = false;
                    foreach (var dir in Directions)
                    {
                        var candidate = current + dir;
                        if (_occupied.Contains(candidate))
                            continue;

                        nextDirection = dir;
                        nextCell = candidate;
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        Debug.LogWarning("[LevelGeneratorMainPath] Не удалось продолжить путь, путь получился короче заданного.");
                        break;
                    }
                }

                _mainPathCells.Add(nextCell);
                _occupied.Add(nextCell);

                current = nextCell;
                currentDirection = nextDirection;
            }
        }

        private Vector2Int ChooseNextDirection(Vector2Int currentDirection)
        {
            if (currentDirection != Vector2Int.zero &&
                Random.value < _keepDirectionProbability)
            {
                return currentDirection;
            }

            var index = Random.Range(0, Directions.Length);
            return Directions[index];
        }

        private void SpawnRoomsWithExits()
        {
            // 1) Предварительно считаем для каждой клетки, какие у неё есть соседи
            var exitMap = BuildExitMap();

            // 2) Спавним комнаты
            for (int i = 0; i < _mainPathCells.Count; i++)
            {
                var cell = _mainPathCells[i];
                var worldPos = new Vector3(
                    cell.x * _roomSize.x,
                    cell.y * _roomSize.y,
                    0f
                );

                var prefab = i switch
                {
                    0 => _startRoomPrefab,
                    _ when i == _mainPathCells.Count - 1 => _endRoomPrefab,
                    _ => _defaultRoomPrefab
                };

                var go = _resolver.Instantiate(
                    prefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

                go.name = i switch
                {
                    0 => "Room_Start",
                    _ when i == _mainPathCells.Count - 1 => "Room_End",
                    _ => $"Room_{i:00}"
                };

                var roomController = go.GetComponent<RoomController>();
                if (roomController == null)
                {
                    Debug.LogError($"[LevelGeneratorMainPath] На префабе {_startRoomPrefab.name} нет RoomController.");
                    continue;
                }

                if (!exitMap.TryGetValue(cell, out var exits))
                {
                    // Теоретически не должно случаться
                    Debug.LogWarning($"[LevelGeneratorMainPath] Для клетки {cell} нет записи в exitMap.");
                }

                roomController.ConfigureExits(
                    hasNorth: exits.HasNorth,
                    hasSouth: exits.HasSouth,
                    hasWest: exits.HasWest,
                    hasEast: exits.HasEast
                );
            }
        }

        private struct ExitFlags
        {
            public bool HasNorth;
            public bool HasSouth;
            public bool HasWest;
            public bool HasEast;
        }

        private Dictionary<Vector2Int, ExitFlags> BuildExitMap()
        {
            var result = new Dictionary<Vector2Int, ExitFlags>(_mainPathCells.Count);

            var cellSet = new HashSet<Vector2Int>(_mainPathCells);

            foreach (var cell in _mainPathCells)
            {
                var flags = new ExitFlags
                {
                    HasNorth = cellSet.Contains(cell + North),
                    HasSouth = cellSet.Contains(cell + South),
                    HasWest = cellSet.Contains(cell + West),
                    HasEast = cellSet.Contains(cell + East)
                };

                result[cell] = flags;
            }

            return result;
        }
    }
}
