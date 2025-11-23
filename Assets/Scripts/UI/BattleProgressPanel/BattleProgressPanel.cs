// Shows in-battle action controls for skipping, waiting, or fleeing during combat.
using DungeonCrawler.Gameplay.Battle;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleProgressPanel : BattleUIPanel
    {
        private Button _skipButton;
        private Button _waitButton;
        private Button _fleeButton;

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
            if (_skipButton != null)
            {
                _skipButton.clicked -= OnSkipTurnClicked;
            }

            if (_waitButton != null)
            {
                _waitButton.clicked -= OnWaitClicked;
            }

            if (_fleeButton != null)
            {
                _fleeButton.clicked -= OnFleeClicked;
            }
        }

        private void RegisterUiCallbacks()
        {
            if (_skipButton != null)
            {
                _skipButton.clicked += OnSkipTurnClicked;
            }

            if (_waitButton != null)
            {
                _waitButton.clicked += OnWaitClicked;
            }

            if (_fleeButton != null)
            {
                _fleeButton.clicked += OnFleeClicked;
            }
        }

        private void ResolveElements()
        {
            _skipButton ??= Root?.Q<Button>("skip-turn-button");
            _waitButton ??= Root?.Q<Button>("wait-button");
            _fleeButton ??= Root?.Q<Button>("flee-button");
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

        private void OnSkipTurnClicked()
        {
            SceneEventBus?.Publish(new RequestSkipTurnAction());
        }

        private void OnWaitClicked()
        {
            SceneEventBus?.Publish(new RequestWaitAction());
        }

        private void OnFleeClicked()
        {
            SceneEventBus?.Publish(new RequestFleeFromBattle());
        }
    }
}
