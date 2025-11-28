// Manages battle grid slots for friendly and enemy squads, providing helpers to place, query, and reassign occupants.
using System;
using System.Collections.Generic;
using Assets.Scripts.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class BattleGridController : MonoBehaviour
    {
        private const float RowTolerance = 0.01f;

        [SerializeField]
        private Transform[] _friendlySlots = Array.Empty<Transform>();

        [SerializeField]
        private Transform[] _enemySlots = Array.Empty<Transform>();

        private readonly List<BattleGridSlot> _friendlySlotData = new();
        private readonly List<BattleGridSlot> _enemySlotData = new();
        private readonly List<BattleGridSlot> _allGridSlots = new();

        public IReadOnlyList<BattleGridSlot> GridSlots => _allGridSlots;

        private void Awake()
        {
            InitializeSlots(_friendlySlots, _friendlySlotData, BattleGridSlotSide.Ally);
            InitializeSlots(_enemySlots, _enemySlotData, BattleGridSlotSide.Enemy);
            _allGridSlots.AddRange(_friendlySlotData);
            _allGridSlots.AddRange(_enemySlotData);
        }

        public SquadController AddToSlot(int slotIndex, bool isEnemySide, SquadModel squadModel, SquadController prefab)
        {
            var slot = GetSlot(slotIndex, isEnemySide);
            if (slot == null)
            {
                Debug.LogWarning($"Cannot place squad, slot {slotIndex} on {(isEnemySide ? "enemy" : "friendly")} side is missing.");
                return null;
            }

            if (prefab == null)
            {
                Debug.LogWarning("Squad prefab is not assigned; placement skipped.");
                return null;
            }

            RemoveFromSlot(slotIndex, isEnemySide);

            // TODO: перенести инициализацию в UnitSystem
            var parent = slot.Root;
            var instance = Instantiate(prefab, parent);
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;
            instance.Initalize(squadModel);

            slot.Controller = instance;
            slot.Squad = squadModel;

            return instance;
        }

        public void RemoveFromSlot(int slotIndex, bool isEnemySide)
        {
            var slot = GetSlot(slotIndex, isEnemySide);
            if (slot == null)
            {
                return;
            }

            if (slot.Controller != null)
            {
                Destroy(slot.Controller.gameObject);
            }

            slot.Controller = null;
            slot.Squad = null;
        }

        public bool IsSlotEmpty(int slotIndex, bool isEnemySide)
        {
            var slot = GetSlot(slotIndex, isEnemySide);
            return slot == null || slot.IsEmpty;
        }

        public SquadModel GetSquad(int slotIndex, bool isEnemySide)
        {
            return GetSlot(slotIndex, isEnemySide)?.Squad;
        }

        public IReadOnlyList<SquadModel> GetFrontRow(bool isEnemySide)
        {
            return GetRow(isEnemySide, isFrontRow: true);
        }

        public IReadOnlyList<SquadModel> GetBackRow(bool isEnemySide)
        {
            return GetRow(isEnemySide, isFrontRow: false);
        }

        private void InitializeSlots(IReadOnlyList<Transform> slotTransforms, List<BattleGridSlot> slots, BattleGridSlotSide side)
        {
            slots.Clear();

            if (slotTransforms == null)
            {
                return;
            }

            for (int i = 0; i < slotTransforms.Count; i++)
            {
                var slot = new BattleGridSlot(slotTransforms[i])
                {
                    Index = i,
                    Side = side,
                    Type = i < 3 ? BattleGridSlotType.Back : BattleGridSlotType.Front,
                };

                slots.Add(slot);
            }
        }

        private BattleGridSlot GetSlot(int slotIndex, bool isEnemySide)
        {
            var slots = isEnemySide ? _enemySlotData : _friendlySlotData;
            if (slotIndex < 0 || slotIndex >= slots.Count)
            {
                return null;
            }

            return slots[slotIndex];
        }

        private IReadOnlyList<SquadModel> GetRow(bool isEnemySide, bool isFrontRow)
        {
            var slots = isEnemySide ? _enemySlotData : _friendlySlotData;
            if (slots.Count == 0)
            {
                return Array.Empty<SquadModel>();
            }

            var edgePosition = slots[0].LocalX;
            var useMax = isEnemySide ^ isFrontRow;

            foreach (var slot in slots)
            {
                edgePosition = useMax
                    ? Mathf.Max(edgePosition, slot.LocalX)
                    : Mathf.Min(edgePosition, slot.LocalX);
            }

            var rowSquads = new List<SquadModel>();
            foreach (var slot in slots)
            {
                if (slot.Squad == null)
                {
                    continue;
                }

                if (Mathf.Abs(slot.LocalX - edgePosition) <= RowTolerance)
                {
                    rowSquads.Add(slot.Squad);
                }
            }

            return rowSquads;
        }

        public class BattleGridSlot
        {
            public BattleGridSlot(Transform root)
            {
                Root = root;
            }

            public int Index { get; set; }

            public bool IsEmpty => Squad == null || Squad.IsDead;

            public Transform Root { get; }

            public SquadModel Squad { get; set; }

            public SquadController Controller { get; set; }

            public BattleGridSlotSide Side { get; set; }

            public BattleGridSlotType Type { get; set; }

            public float LocalX => Root != null ? Root.localPosition.x : 0f;
        }

        public bool TryResolveSlot(Transform transform, out Transform slotRoot)
        {
            var slot = FindSlotByTransform(transform);
            slotRoot = slot?.Root;
            return slotRoot != null;
        }

        public bool TryGetSlotSide(Transform slotRoot, out BattleGridSlotSide side)
        {
            var slot = FindSlotByTransform(slotRoot);
            side = slot?.Side ?? default;
            return slot != null;
        }

        public bool IsSlotEmpty(Transform slotRoot)
        {
            var slot = FindSlotByTransform(slotRoot);
            return slot == null || slot.IsEmpty;
        }

        public bool TryGetSlotOccupant(Transform slotRoot, out Transform occupant)
        {
            var slot = FindSlotByTransform(slotRoot);
            occupant = slot?.Controller != null ? slot.Controller.transform : null;
            return occupant != null;
        }

        public bool TryAttachToSlot(Transform slotRoot, Transform occupant)
        {
            var slot = FindSlotByTransform(slotRoot);
            if (slot == null || occupant == null)
            {
                return false;
            }

            var controller = occupant.GetComponent<SquadController>();
            if (controller == null || controller.Model == null)
            {
                return false;
            }

            slot.Controller = controller;
            slot.Squad = controller.Model;

            occupant.SetParent(slot.Root, false);
            occupant.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            occupant.localScale = Vector3.one;

            return true;
        }

        public bool TryRemoveOccupant(Transform occupant, out Transform slotRoot)
        {
            slotRoot = null;

            if (occupant == null)
            {
                return false;
            }

            var controller = occupant.GetComponentInParent<SquadController>();
            if (controller == null)
            {
                return false;
            }

            var slot = FindSlotByController(controller);
            if (slot == null)
            {
                return false;
            }

            slotRoot = slot.Root;
            slot.Controller = null;
            slot.Squad = null;
            return true;
        }

        private BattleGridSlot FindSlotByTransform(Transform transform)
        {
            if (transform == null)
            {
                return null;
            }

            foreach (var slot in _allGridSlots)
            {
                for (var current = transform; current != null; current = current.parent)
                {
                    if (slot.Root == current)
                    {
                        return slot;
                    }
                }
            }

            return null;
        }

        private BattleGridSlot FindSlotByController(SquadController controller)
        {
            if (controller == null)
            {
                return null;
            }

            foreach (var slot in _allGridSlots)
            {
                if (slot.Controller == controller)
                {
                    return slot;
                }
            }

            return null;
        }
    }
}
