// Shows in-battle action controls for skipping, waiting, or fleeing during combat.
using System;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.UI.Common;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleProgressPanel : BaseUIController
    {
        private VisualElement _panelRootUI;
        private Button _skipButtonUI;
        private Button _waitButtonUI;
        private Button _fleeButtonUI;
        private IDisposable _battleStateChangedSubscription;

        protected override void RegisterUIElements()
        {
            _panelRootUI = _uiDocument.rootVisualElement.Q<VisualElement>(className: "battle-progress");
            _skipButtonUI = _uiDocument.rootVisualElement.Q<Button>("skip-turn-button");
            _waitButtonUI = _uiDocument.rootVisualElement.Q<Button>("wait-button");
            _fleeButtonUI = _uiDocument.rootVisualElement.Q<Button>("flee-button");

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
            if (_skipButtonUI != null)
            {
                _skipButtonUI.clicked += HandleSkipTurnClicked;
            }

            if (_waitButtonUI != null)
            {
                _waitButtonUI.clicked += HandleWaitClicked;
            }

            if (_fleeButtonUI != null)
            {
                _fleeButtonUI.clicked += HandleFleeClicked;
            }
        }

        protected override void UnsubscriveFromUIEvents()
        {
            if (_skipButtonUI != null)
            {
                _skipButtonUI.clicked -= HandleSkipTurnClicked;
            }

            if (_waitButtonUI != null)
            {
                _waitButtonUI.clicked -= HandleWaitClicked;
            }

            if (_fleeButtonUI != null)
            {
                _fleeButtonUI.clicked -= HandleFleeClicked;
            }
        }

        private void HandleBattleStateChanged(BattleStateChanged stateChanged)
        {
            if (stateChanged.FromState == BattleState.Preparation)
            {
                Show();
            }

            if (stateChanged.ToState == BattleState.Result)
            {
                Hide();
            }
        }

        private void HandleSkipTurnClicked()
        {
            _sceneEventBusService?.Publish(new RequestSkipTurnAction());
        }

        private void HandleWaitClicked()
        {
            _sceneEventBusService?.Publish(new RequestWaitAction());
        }

        private void HandleFleeClicked()
        {
            _sceneEventBusService?.Publish(new RequestFleeFromBattle());
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
