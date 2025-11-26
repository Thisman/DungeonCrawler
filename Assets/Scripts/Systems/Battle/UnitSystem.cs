// Manages squad controller instances for battle, including spawning and lookup by model.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using UnityEngine;
using DungeonCrawler.Core.EventBus;
using System.Threading.Tasks;
using System.Linq;

namespace DungeonCrawler.Systems.Battle
{
    public class UnitSystem : IDisposable
    {
        private readonly GameEventBus _sceneEventBus;
        private readonly SquadController _squadPrefab;
        private readonly Transform _defaultParent;
        private readonly BattleGridController _gridController;
        private readonly BattleContext _context;
        private readonly List<IDisposable> _subscriptions = new();
        private readonly Dictionary<SquadModel, SquadController> _squadControllers = new();
        private readonly List<SquadController> _highlightedControllers = new();
        private readonly List<SquadModel> _trackedSquads = new();

        public UnitSystem(
            GameEventBus sceneEventBus,
            SquadController squadPrefab,
            Transform defaultParent,
            BattleGridController gridController,
            BattleContext context)
        {
            _sceneEventBus = sceneEventBus;
            _squadPrefab = squadPrefab;
            _defaultParent = defaultParent;
            _gridController = gridController;
            _context = context;

            _subscriptions.Add(_sceneEventBus.Subscribe<RequestSelectAction>(HandleRequestSelectAction));
            _subscriptions.Add(_sceneEventBus.Subscribe<BattleStateChanged>(HandleStateChanged));
        }

        public void InitializeSquads(IReadOnlyList<SquadModel> squads)
        {
            if (_squadPrefab == null)
            {
                Debug.LogWarning("Squad prefab is not assigned; squad layout will be skipped.");
                return;
            }

            if (squads == null)
            {
                return;
            }

            var friendlyIndex = 0;
            var enemyIndex = 0;

            foreach (var squad in squads)
            {
                if (squad == null)
                {
                    continue;
                }

                var isEnemy = squad.Unit.Definition.IsEnemy();
                var slotIndex = isEnemy ? enemyIndex++ : friendlyIndex++;
                var squadInstance = _gridController != null
                    ? _gridController.AddToSlot(slotIndex, isEnemy, squad, _squadPrefab)
                    : UnityEngine.Object.Instantiate(_squadPrefab, _defaultParent);

                if (squadInstance == null)
                {
                    continue;
                }

                if (_gridController == null)
                {
                    squadInstance.transform.localPosition = Vector3.zero;
                    squadInstance.Initalize(squad);
                }

                _squadControllers[squad] = squadInstance;
                squad.Changed += HandleSquadChanged;
                _trackedSquads.Add(squad);
            }

            UpdateSquadPlacements();
        }

        public SquadController GetController(SquadModel squad)
        {
            _squadControllers.TryGetValue(squad, out var controller);
            return controller;
        }

        public IReadOnlyList<SquadController> GetControllers(IEnumerable<SquadModel> squads)
        {
            if (squads == null)
            {
                return Array.Empty<SquadController>();
            }

            var result = new List<SquadController>();
            foreach (var squad in squads)
            {
                if (squad != null && _squadControllers.TryGetValue(squad, out var controller))
                {
                    result.Add(controller);
                }
            }

            return result;
        }

        public async Task ApplyDamage(DamageInstance damage)
        {
            if (damage == null)
            {
                return;
            }

            if (TryGetLiveController(damage.Attacker, out var attackerController))
            {
                await attackerController.ResolveAttack(damage);
            }

            if (TryGetLiveController(damage.Target, out var targetController))
            {
                await targetController.TakeDamage(damage);
            }
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();

            foreach (var squad in _trackedSquads.Where(s => s != null))
            {
                squad.Changed -= HandleSquadChanged;
            }

            _trackedSquads.Clear();
        }

        private void UpdateSquadPlacements()
        {
            if (_context == null || _gridController == null)
            {
                return;
            }

            RegisterRowPlacements(_gridController.GetFrontRow(false), isEnemySide: false, BattleRow.Front);
            RegisterRowPlacements(_gridController.GetBackRow(false), isEnemySide: false, BattleRow.Back);
            RegisterRowPlacements(_gridController.GetFrontRow(true), isEnemySide: true, BattleRow.Front);
            RegisterRowPlacements(_gridController.GetBackRow(true), isEnemySide: true, BattleRow.Back);
        }

        private void RegisterRowPlacements(IEnumerable<SquadModel> squads, bool isEnemySide, BattleRow row)
        {
            if (squads == null)
            {
                return;
            }

            foreach (var squad in squads)
            {
                _context.SetSquadPlacement(squad, isEnemySide, row);
            }
        }

        private void HandleStateChanged(BattleStateChanged evt)
        {
            if (evt.ToState == BattleState.TurnEnd)
            {
                ClearHighlights();
            }
        }

        private void HandleRequestSelectAction(RequestSelectAction evt)
        {
            ClearHighlights();

            if (evt?.Action == null || _context?.ActiveUnit == null || _context.ActiveUnit.IsDead)
            {
                return;
            }

            var validTargets = evt.Action.GetValidTargets(_context.ActiveUnit, _context);
            if (validTargets == null)
            {
                return;
            }

            foreach (var target in validTargets)
            {
                if (target == null || !_squadControllers.TryGetValue(target, out var controller) || controller.Model?.IsDead == true)
                {
                    continue;
                }

                controller.SetAsTarget(true);
                _highlightedControllers.Add(controller);
            }
        }

        private void HandleSquadChanged(SquadModel squad, int newCount, int oldCount)
        {
            if (squad == null || !_squadControllers.TryGetValue(squad, out var controller))
            {
                return;
            }

            if (squad.IsDead)
            {
                controller.SetAsTarget(false);
                _highlightedControllers.Remove(controller);
            }
        }

        private void ClearHighlights()
        {
            foreach (var controller in _highlightedControllers)
            {
                controller?.SetAsTarget(false);
            }

            _highlightedControllers.Clear();
        }

        private bool TryGetLiveController(SquadModel squad, out SquadController controller)
        {
            controller = null;

            if (squad == null || !_squadControllers.TryGetValue(squad, out var foundController))
            {
                return false;
            }

            if (foundController.Model?.IsDead == true)
            {
                return false;
            }

            controller = foundController;
            return true;
        }
    }
}
