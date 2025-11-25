// Presents the battle completion button once combat reaches the result state.
using System;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.UI.Common;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleResultPanel : BaseUIController
    {
        private VisualElement _panelRootUI;
        private Button _finishBattleButtonUI;
        private IDisposable _battleStateChangedSubscription;

        protected override void RegisterUIElements()
        {
            _panelRootUI = _uiDocument.rootVisualElement.Q<VisualElement>(className: "battle-result");
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
                Show();
            }

            if (stateChanged.FromState == BattleState.Result)
            {
                Hide();
            }
        }

        private void HandleFinishClicked()
        {
            _sceneEventBusService?.Publish(new RequestFinishBattle());
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
