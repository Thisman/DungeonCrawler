using DungeonCrawler.Gameplay.Dungeon;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
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

        [Header("Branch Settings")]
        [SerializeField, Min(0)]
        private int _maxBranches = 4;

        [SerializeField, Range(0f, 1f)]
        private float _branchChancePerMainRoom = 0.5f;

        [SerializeField]
        private Vector2Int _branchLengthRange = new(1, 3); // [min; max]

        [SerializeField, Range(0f, 1f)]
        private float _loopConnectionChance = 0.5f; // шанс сделать петлю в конце сегмента ветки

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

        /// <summary>Занятые клетки сетки (есть комната).</summary>
        private readonly HashSet<Vector2Int> _occupied = new();

        /// <summary>Клетки основного пути (по порядку).</summary>
        private readonly List<Vector2Int> _mainPathCells = new();

        /// <summary>Все клетки (главный путь + ветки).</summary>
        private readonly List<Vector2Int> _allCells = new();

        /// <summary>Граф соединений: каждая клетка → множество соседей, с которыми ЕСТЬ проход.</summary>
        private readonly Dictionary<Vector2Int, HashSet<Vector2Int>> _connections = new();

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
            _allCells.Clear();
            _connections.Clear();

            GenerateMainPath();
            GenerateBranches();
            SpawnRoomsWithExits();
        }

        #region Main Path

        private void GenerateMainPath()
        {
            var current = Vector2Int.zero;

            _mainPathCells.Add(current);
            _allCells.Add(current);
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
                        Debug.LogWarning("[DungeonGenerator] Не удалось продолжить главный путь, путь получился короче заданного.");
                        break;
                    }
                }

                _mainPathCells.Add(nextCell);
                _allCells.Add(nextCell);
                _occupied.Add(nextCell);

                ConnectCells(current, nextCell);

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

        #endregion

        #region Branches

        private void GenerateBranches()
        {
            if (_maxBranches <= 0)
                return;

            int branchesCreated = 0;

            // не трогаем старт и финиш для веток (по желанию можно изменить)
            for (int i = 1; i < _mainPathCells.Count - 1; i++)
            {
                if (branchesCreated >= _maxBranches)
                    break;

                if (Random.value > _branchChancePerMainRoom)
                    continue;

                var startCell = _mainPathCells[i];

                // Выбираем возможные направления для начала ветки
                var availableDirs = new List<Vector2Int>();
                foreach (var dir in Directions)
                {
                    var candidate = startCell + dir;
                    if (_occupied.Contains(candidate))
                        continue;

                    availableDirs.Add(dir);
                }

                if (availableDirs.Count == 0)
                    continue;

                var dirIndex = Random.Range(0, availableDirs.Count);
                var currentDirection = availableDirs[dirIndex];

                int minLen = Mathf.Max(1, _branchLengthRange.x);
                int maxLen = Mathf.Max(minLen, _branchLengthRange.y);
                int branchLen = Random.Range(minLen, maxLen + 1);

                var current = startCell;

                for (int step = 0; step < branchLen; step++)
                {
                    var next = current + currentDirection;

                    // Если следующая клетка занята — раньше заканчиваем ветку
                    if (_occupied.Contains(next))
                    {
                        break;
                    }

                    _occupied.Add(next);
                    _allCells.Add(next);

                    ConnectCells(current, next);

                    current = next;

                    // Возможная петля: соединяемся с уже существующей комнатой по соседству,
                    // но только если ещё нет соединения (чтобы реально создать новый путь).
                    if (Random.value < _loopConnectionChance)
                    {
                        TryCreateLoopFrom(current);
                    }

                    // По желанию можно добавить смену направления ветки, но пока оставим прямой.
                }

                branchesCreated++;
            }
        }

        /// <summary>
        /// Пытаемся создать петлю: соединяем текущую комнату с одной из соседних занятых клеток,
        /// с которой ещё нет прохода.
        /// </summary>
        private void TryCreateLoopFrom(Vector2Int cell)
        {
            _connections.TryGetValue(cell, out var neighbors);

            var candidates = new List<Vector2Int>();

            foreach (var dir in Directions)
            {
                var candidate = cell + dir;

                if (!_occupied.Contains(candidate))
                    continue;

                if (neighbors != null && neighbors.Contains(candidate))
                    continue; // уже есть соединение

                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
                return;

            var index = Random.Range(0, candidates.Count);
            var target = candidates[index];

            ConnectCells(cell, target);
        }

        /// <summary>
        /// Добавить двустороннее соединение между двумя комнатами (ребро графа).
        /// </summary>
        private void ConnectCells(Vector2Int a, Vector2Int b)
        {
            if (!_connections.TryGetValue(a, out var neighborsA))
            {
                neighborsA = new HashSet<Vector2Int>();
                _connections[a] = neighborsA;
            }

            if (!_connections.TryGetValue(b, out var neighborsB))
            {
                neighborsB = new HashSet<Vector2Int>();
                _connections[b] = neighborsB;
            }

            neighborsA.Add(b);
            neighborsB.Add(a);
        }

        #endregion

        #region Spawn

        private void SpawnRoomsWithExits()
        {
            var exitMap = BuildExitMap();

            for (int i = 0; i < _allCells.Count; i++)
            {
                var cell = _allCells[i];
                var worldPos = new Vector3(
                    cell.x * _roomSize.x,
                    cell.y * _roomSize.y,
                    0f
                );

                var prefab = GetPrefabForCell(cell);
                var go = _resolver.Instantiate(
                    prefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

                go.name = GetNameForCell(cell, i);

                var roomController = go.GetComponent<RoomController>();
                if (roomController == null)
                {
                    Debug.LogError($"[DungeonGenerator] На префабе {prefab.name} нет RoomController.");
                    continue;
                }

                if (!exitMap.TryGetValue(cell, out var exits))
                {
                    Debug.LogWarning($"[DungeonGenerator] Для клетки {cell} нет записи в exitMap.");
                }

                roomController.ConfigureExits(
                    hasNorth: exits.HasNorth,
                    hasSouth: exits.HasSouth,
                    hasWest: exits.HasWest,
                    hasEast: exits.HasEast
                );
            }
        }

        private GameObject GetPrefabForCell(Vector2Int cell)
        {
            if (_mainPathCells.Count > 0)
            {
                if (cell == _mainPathCells[0])
                    return _startRoomPrefab;

                if (cell == _mainPathCells[_mainPathCells.Count - 1])
                    return _endRoomPrefab;
            }

            return _defaultRoomPrefab;
        }

        private string GetNameForCell(Vector2Int cell, int index)
        {
            if (_mainPathCells.Count > 0)
            {
                if (cell == _mainPathCells[0])
                    return "Room_Start";

                if (cell == _mainPathCells[_mainPathCells.Count - 1])
                    return "Room_End";
            }

            return $"Room_{index:00}";
        }

        private struct ExitFlags
        {
            public bool HasNorth;
            public bool HasSouth;
            public bool HasWest;
            public bool HasEast;
        }

        /// <summary>
        /// Строим карту выходов по графу соединений, а НЕ просто по соседству на сетке.
        /// </summary>
        private Dictionary<Vector2Int, ExitFlags> BuildExitMap()
        {
            var result = new Dictionary<Vector2Int, ExitFlags>(_allCells.Count);

            foreach (var cell in _allCells)
            {
                _connections.TryGetValue(cell, out var neighbors);

                var flags = new ExitFlags
                {
                    HasNorth = neighbors != null && neighbors.Contains(cell + North),
                    HasSouth = neighbors != null && neighbors.Contains(cell + South),
                    HasWest = neighbors != null && neighbors.Contains(cell + West),
                    HasEast = neighbors != null && neighbors.Contains(cell + East)
                };

                result[cell] = flags;
            }

            return result;
        }

        #endregion
    }
}
