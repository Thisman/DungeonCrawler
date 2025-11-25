// Manages squad controller instances for battle, including spawning, lookup by model, and interactions limited to living squads.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using UnityEngine;
using DungeonCrawler.Core.EventBus;
using System.Threading.Tasks;

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
        private readonly Dictionary<UnitModel, SquadController> _unitControllers = new();
        private readonly List<SquadController> _highlightedControllers = new();

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
                _unitControllers[squad.Unit] = squadInstance;
            }
        }

        public SquadController GetController(SquadModel squad)
        {
            if (squad?.IsDead == true)
            {
                return null;
            }

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

            if (damage.Attacker != null && _unitControllers.TryGetValue(damage.Attacker, out var attackerController))
            {
                if (attackerController.Model?.IsDead == false)
                {
                    await attackerController.ResolveAttack(damage);
                }
            }

            if (damage.Target != null && _unitControllers.TryGetValue(damage.Target, out var targetController))
            {
                if (targetController.Model?.IsDead == false)
                {
                    await targetController.TakeDamage(damage);
                }
            }
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
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

            if (evt?.Action == null || _context?.ActiveUnit == null)
            {
                return;
            }

            var validTargets = evt.Action.GetValidTargets(_context.ActiveUnit.Unit, _context);
            if (validTargets == null)
            {
                return;
            }

            foreach (var target in validTargets)
            {
                if (target == null || !_unitControllers.TryGetValue(target, out var controller))
                {
                    continue;
                }

                if (controller.Model?.IsDead == true)
                {
                    continue;
                }

                controller.SetAsTarget(true);
                _highlightedControllers.Add(controller);
            }
        }

        private void ClearHighlights()
        {
            foreach (var controller in _highlightedControllers)
            {
                if (controller?.Model?.IsDead == false)
                {
                    controller.SetAsTarget(false);
                }
            }

            _highlightedControllers.Clear();
        }
    }
}
