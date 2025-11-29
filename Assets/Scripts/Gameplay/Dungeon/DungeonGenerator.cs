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

        [Header("Grid Bounds (in rooms)")]
        [SerializeField]
        private Vector2Int _gridSize = new(5, 10); // width (X), height (Y)

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

        // Границы по сетке
        private int _minX;
        private int _maxX;
        private int _minY;
        private int _maxY;

        // Стартовая клетка в координатах сетки
        private Vector2Int _startCell;

        private void Start()
        {
            if (_generateOnStart)
            {
                Generate();
            }
        }

        public void Generate()
        {
            // Настраиваем границы
            SetupBounds();

            _occupied.Clear();
            _mainPathCells.Clear();
            _allCells.Clear();
            _connections.Clear();

            GenerateMainPath();
            GenerateBranches();
            SpawnRoomsWithExits();
        }

        private void SetupBounds()
        {
            // Минимум 1x1
            int width = Mathf.Max(1, _gridSize.x);
            int height = Mathf.Max(1, _gridSize.y);

            _minX = 0;
            _maxX = width - 1;
            _minY = 0;
            _maxY = height - 1;

            // Стартовая клетка — центр прямоугольника
            _startCell = new Vector2Int(width / 2, height / 2);
        }

        private bool InBounds(Vector2Int cell)
        {
            return cell.x >= _minX && cell.x <= _maxX &&
                   cell.y >= _minY && cell.y <= _maxY;
        }

        #region Main Path

        private void GenerateMainPath()
        {
            var current = _startCell;

            _mainPathCells.Add(current);
            _allCells.Add(current);
            _occupied.Add(current);

            var currentDirection = East; // стартовое направление

            while (_mainPathCells.Count < _mainPathLength)
            {
                // Сначала собираем все допустимые направления (внутри границ и в свободные клетки)
                var possibleDirs = new List<Vector2Int>();
                foreach (var dir in Directions)
                {
                    var candidate = current + dir;
                    if (!InBounds(candidate))
                        continue;
                    if (_occupied.Contains(candidate))
                        continue;

                    possibleDirs.Add(dir);
                }

                if (possibleDirs.Count == 0)
                {
                    Debug.LogWarning("[DungeonGenerator] Главный путь упёрся в границы/сам себя раньше заданной длины.");
                    break;
                }

                Vector2Int nextDirection;

                // Если можем двигаться дальше в текущем направлении и сработала вероятность — продолжаем
                if (possibleDirs.Contains(currentDirection) &&
                    Random.value < _keepDirectionProbability)
                {
                    nextDirection = currentDirection;
                }
                else
                {
                    // Иначе выбираем случайное допустимое направление
                    var index = Random.Range(0, possibleDirs.Count);
                    nextDirection = possibleDirs[index];
                }

                var nextCell = current + nextDirection;

                _mainPathCells.Add(nextCell);
                _allCells.Add(nextCell);
                _occupied.Add(nextCell);

                ConnectCells(current, nextCell);

                current = nextCell;
                currentDirection = nextDirection;
            }
        }

        #endregion

        #region Branches

        private void GenerateBranches()
        {
            if (_maxBranches <= 0)
                return;

            int branchesCreated = 0;

            // Ветки можем создавать из ЛЮБОЙ существующей комнаты (кроме старт/финиш),
            // _allCells будет расти по мере генерации веток → подветвление.
            for (int i = 0; i < _allCells.Count; i++)
            {
                if (branchesCreated >= _maxBranches)
                    break;

                var startCell = _allCells[i];

                // Не ветвимся от стартовой и конечной комнаты (по желанию)
                if (_mainPathCells.Count > 0)
                {
                    if (startCell == _mainPathCells[0] ||
                        startCell == _mainPathCells[_mainPathCells.Count - 1])
                        continue;
                }

                if (Random.value > _branchChancePerMainRoom)
                    continue;

                // Выбираем возможные направления для начала ветки
                var availableDirs = new List<Vector2Int>();
                foreach (var dir in Directions)
                {
                    var candidate = startCell + dir;
                    if (!InBounds(candidate))
                        continue;
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

                    // Если вышли за границы или упёрлись в занятую клетку — заканчиваем ветку
                    if (!InBounds(next) || _occupied.Contains(next))
                    {
                        break;
                    }

                    _occupied.Add(next);
                    _allCells.Add(next);
                    ConnectCells(current, next);

                    current = next;

                    // Попытка сделать петлю из этой комнаты
                    if (Random.value < _loopConnectionChance)
                    {
                        TryCreateLoopFrom(current);
                    }

                    // Здесь можно добавить смену направления внутри ветки, если нужно:
                    // например, с маленькой вероятностью выбрать новое допустимое направление.
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

                if (!InBounds(candidate))
                    continue;

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

                // Привязываем стартовую комнату к (0,0) в мире,
                // остальные — относительно неё.
                var offsetFromStart = cell - _startCell;
                var worldPos = new Vector3(
                    offsetFromStart.x * _roomSize.x,
                    offsetFromStart.y * _roomSize.y,
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
