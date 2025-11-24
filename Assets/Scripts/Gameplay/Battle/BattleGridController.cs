// Manages battle grid slots for friendly and enemy squads, providing helpers to place and query occupants.
using System;
using System.Collections.Generic;
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

        [SerializeField]
        private Transform _defaultParent;

        private readonly List<BattleSlot> _friendlySlotData = new();
        private readonly List<BattleSlot> _enemySlotData = new();

        private void Awake()
        {
            InitializeSlots(_friendlySlots, _friendlySlotData);
            InitializeSlots(_enemySlots, _enemySlotData);
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

            var parent = slot.Root != null ? slot.Root : _defaultParent;
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
            return slot == null || slot.Squad == null;
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

        private void InitializeSlots(IReadOnlyList<Transform> slotTransforms, List<BattleSlot> slots)
        {
            slots.Clear();

            if (slotTransforms == null)
            {
                return;
            }

            foreach (var slotTransform in slotTransforms)
            {
                slots.Add(new BattleSlot(slotTransform));
            }
        }

        private BattleSlot GetSlot(int slotIndex, bool isEnemySide)
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

        private class BattleSlot
        {
            public BattleSlot(Transform root)
            {
                Root = root;
            }

            public Transform Root { get; }

            public SquadModel Squad { get; set; }

            public SquadController Controller { get; set; }

            public float LocalX => Root != null ? Root.localPosition.x : 0f;
        }
    }
}
