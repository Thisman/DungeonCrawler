// Displays the tactical phase UI with the start battle action button.
using DungeonCrawler.Gameplay.Battle;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattlePreparationPanel : BattleUIPanel
    {
        private Button _startButton;

        protected override void OnPanelAttachedToPanel()
        {
            ResolveElements();
            RegisterUiCallbacks();
            base.OnPanelAttachedToPanel();
            UpdateInitialVisibility();
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
            if (_startButton != null)
            {
                _startButton.clicked -= OnStartBattleClicked;
            }
        }

        private void RegisterUiCallbacks()
        {
            if (_startButton != null)
            {
                _startButton.clicked += OnStartBattleClicked;
            }
        }

        private void ResolveElements()
        {
            _startButton ??= Root?.Q<Button>("start-battle-button");
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

        private void UpdateInitialVisibility()
        {
            if (Launcher != null && Launcher.CurrentBattleState == BattleState.Preparation)
            {
                Show();
            }
        }

        private void OnStartBattleClicked()
        {
            SceneEventBus?.Publish(new RequestBattlePreparationFinish());
        }
    }
}
