// Presents the battle completion button once combat reaches the result state.
using DungeonCrawler.Gameplay.Battle;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleResultPanel : BattleUIPanel
    {
        private Button _finishButton;

        protected override void OnPanelAttachedToPanel()
        {
            ResolveElements();
            RegisterUiCallbacks();
            base.OnPanelAttachedToPanel();
        }

        protected override void RegisterSubscriptions()
        {
            if (SceneEventBus == null)
            {
                return;
            }

            AddSubscription(SceneEventBus.Subscribe<BattleStateChanged>(HandleBattleStateChanged));
        }

        protected override void UnregisterUiCallbacks()
        {
            if (_finishButton != null)
            {
                _finishButton.clicked -= OnFinishClicked;
            }
        }

        private void RegisterUiCallbacks()
        {
            if (_finishButton != null)
            {
                _finishButton.clicked += OnFinishClicked;
            }
        }

        private void ResolveElements()
        {
            _finishButton ??= Root?.Q<Button>("finish-battle-button");
        }

        private void HandleBattleStateChanged(BattleStateChanged stateChanged)
        {
            if (stateChanged.ToState == BattleState.Result)
            {
                Show();
                return;
            }

            if (stateChanged.FromState == BattleState.Result)
            {
                Hide();
            }
        }

        private void OnFinishClicked()
        {
            SceneEventBus?.Publish(new RequestFinishBattle());
        }
    }
}
