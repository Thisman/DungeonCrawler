// Displays the tactical phase UI with the start battle action button.
using System;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattlePreparationPanel : BaseUIController
    {
        private VisualElement _panelRootUI;
        private Button _startButtonUI;
        private IDisposable _battleStateChangedSubscription;

        protected override void RegisterUIElements()
        {
            _panelRootUI = _uiDocument.rootVisualElement.Q<VisualElement>(className: "battle-preparation");
            _startButtonUI = _uiDocument.rootVisualElement.Q<Button>(className: "battle-preparation__start");

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
            if (_startButtonUI != null)
            {
                _startButtonUI.clicked += HandleStartBattleClicked;
            }
        }

        protected override void UnsubscriveFromUIEvents()
        {
            if (_startButtonUI != null)
            {
                _startButtonUI.clicked -= HandleStartBattleClicked;
            }
        }

        private void HandleBattleStateChanged(BattleStateChanged stateChanged)
        {
            if (stateChanged.ToState == BattleState.Preparation)
            {
                Show();
            }

            if (stateChanged.FromState == BattleState.Preparation)
            {
                Hide();
            }
        }

        private void HandleStartBattleClicked()
        {
            _sceneEventBusService?.Publish(new RequestBattlePreparationFinish());
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
