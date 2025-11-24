// Manages squad controller instances for battle, including spawning and lookup by model.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Gameplay.Unit;
using UnityEngine;
using DungeonCrawler.Core.EventBus;

namespace DungeonCrawler.Systems.Battle
{
    public class UnitSystem : IDisposable
    {
        private readonly GameEventBus _sceneEventBus;
        private readonly SquadController _squadPrefab;
        private readonly Transform _defaultParent;
        private readonly Transform _friendlySquadsRoot;
        private readonly Transform _enemySquadsRoot;
        private readonly int _unitsPerRow;
        private readonly Vector2 _squadSpacing;
        private readonly BattleContext _context;
        private readonly List<IDisposable> _subscriptions = new();
        private readonly Dictionary<SquadModel, SquadController> _squadControllers = new();
        private readonly Dictionary<UnitModel, SquadController> _unitControllers = new();
        private readonly List<SquadController> _highlightedControllers = new();

        public UnitSystem(
            GameEventBus sceneEventBus,
            SquadController squadPrefab,
            Transform defaultParent,
            Transform friendlySquadsRoot,
            Transform enemySquadsRoot,
            int unitsPerRow,
            Vector2 squadSpacing,
            BattleContext context)
        {
            _sceneEventBus = sceneEventBus;
            _squadPrefab = squadPrefab;
            _defaultParent = defaultParent;
            _friendlySquadsRoot = friendlySquadsRoot;
            _enemySquadsRoot = enemySquadsRoot;
            _unitsPerRow = Mathf.Max(1, unitsPerRow);
            _squadSpacing = squadSpacing;
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
                var parent = isEnemy ? _enemySquadsRoot : _friendlySquadsRoot;
                var squadInstance = UnityEngine.Object.Instantiate(_squadPrefab, parent ? parent : _defaultParent);
                var slotIndex = isEnemy ? enemyIndex++ : friendlyIndex++;

                squadInstance.transform.localPosition = CalculateLocalPosition(slotIndex, isEnemy);
                squadInstance.Initalize(squad);

                _squadControllers[squad] = squadInstance;
                _unitControllers[squad.Unit] = squadInstance;
            }
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

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
        }

        private Vector3 CalculateLocalPosition(int index, bool isEnemy)
        {
            var row = index / _unitsPerRow;
            var column = index % _unitsPerRow;
            var direction = isEnemy ? 1f : -1f;

            return new Vector3(direction * column * _squadSpacing.x, -row * _squadSpacing.y, 0f);
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

                controller.AnimationController?.HighlightAsTarget();
                _highlightedControllers.Add(controller);
            }
        }

        private void ClearHighlights()
        {
            foreach (var controller in _highlightedControllers)
            {
                controller?.AnimationController?.ResetColor();
            }

            _highlightedControllers.Clear();
        }
    }
}
