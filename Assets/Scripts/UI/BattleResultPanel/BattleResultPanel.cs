// Presents final squad outcomes and the completion button once combat reaches the result state.
using System;
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleResultPanel : BaseUIController
    {
        private VisualElement _panelRootUI;
        private VisualElement _playerContainerUI;
        private VisualElement _enemyContainerUI;
        private Button _finishBattleButtonUI;
        private IDisposable _battleStateChangedSubscription;

        protected override void RegisterUIElements()
        {
            _panelRootUI = _uiDocument.rootVisualElement.Q<VisualElement>(className: "battle-result");
            _playerContainerUI = _uiDocument.rootVisualElement.Q<VisualElement>("player-result-container");
            _enemyContainerUI = _uiDocument.rootVisualElement.Q<VisualElement>("enemy-result-container");
            _finishBattleButtonUI = _uiDocument.rootVisualElement.Q<Button>("finish-battle-button");

            Hide();
        }

        protected override void SubscriveToGameEvents()
        {
            _battleStateChangedSubscription ??= _sceneEventBusService?.Subscribe<BattleStateChanged>(HandleBattleStateChanged);
        }

        protected override void UnsubscribeFromGameEvents()
        {
            _battleStateChangedSubscription?.Dispose();
            _battleStateChangedSubscription = null;
        }

        protected override void SubcribeToUIEvents()
        {
            if (_finishBattleButtonUI != null)
            {
                _finishBattleButtonUI.clicked += HandleFinishClicked;
            }
        }

        protected override void UnsubscriveFromUIEvents()
        {
            if (_finishBattleButtonUI != null)
            {
                _finishBattleButtonUI.clicked -= HandleFinishClicked;
            }
        }

        private void HandleBattleStateChanged(BattleStateChanged stateChanged)
        {
            if (stateChanged.ToState == BattleState.Result)
            {
                UpdateResults(stateChanged.Context);
                Show();
            }

            if (stateChanged.FromState == BattleState.Result)
            {
                ClearResults();
                Hide();
            }
        }

        private void HandleFinishClicked()
        {
            _sceneEventBusService?.Publish(new RequestFinishBattle());
        }

        private void UpdateResults(BattleContext context)
        {
            if (context?.Result == null)
            {
                return;
            }

            ClearResults();
            PopulateSquads(_playerContainerUI, context.Result.GetPlayerSquads());
            PopulateSquads(_enemyContainerUI, context.Result.GetEnemySquads());
        }

        private void PopulateSquads(VisualElement container, IEnumerable<BattleResult.BattleSquadResult> squads)
        {
            if (container == null)
            {
                return;
            }

            if (squads == null)
            {
                container.Clear();
                return;
            }

            foreach (var squadResult in squads)
            {
                container.Add(CreateSquadEntry(squadResult));
            }
        }

        private VisualElement CreateSquadEntry(BattleResult.BattleSquadResult squadResult)
        {
            var squadElement = new VisualElement();
            squadElement.AddToClassList("battle-result__squad");

            if (squadResult?.Squad?.IsDead == true)
            {
                squadElement.AddToClassList("battle-result__squad--dead");
            }

            var nameLabel = new Label(squadResult?.Squad?.Unit?.Definition?.Name ?? "-");
            nameLabel.AddToClassList("battle-result__name");
            squadElement.Add(nameLabel);

            if (squadResult?.Squad?.Unit?.Definition?.Icon != null)
            {
                var image = new Image
                {
                    sprite = squadResult.Squad.Unit.Definition.Icon,
                    scaleMode = ScaleMode.ScaleToFit
                };

                image.AddToClassList("battle-result__icon");
                squadElement.Add(image);
            }

            var counts = new VisualElement();
            counts.AddToClassList("battle-result__counts");

            var initialCountLabel = new Label(squadResult?.InitialCount.ToString() ?? "0");
            counts.Add(initialCountLabel);

            if (squadResult != null)
            {
                var delta = squadResult.Delta;
                var deltaLabel = new Label($"({(delta > 0 ? "+" : string.Empty)}{delta})");
                deltaLabel.AddToClassList("battle-result__delta");
                counts.Add(deltaLabel);
            }

            squadElement.Add(counts);
            return squadElement;
        }

        private void ClearResults()
        {
            _playerContainerUI?.Clear();
            _enemyContainerUI?.Clear();
        }

        private void Show()
        {
            _panelRootUI?.AddToClassList("panel--active");
        }

        private void Hide()
        {
            _panelRootUI?.RemoveFromClassList("panel--active");
        }
    }
}
