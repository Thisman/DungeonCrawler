// Renders the upcoming unit order for the active battle queue.
using System;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.UI.Common;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleQueuePanel : BaseUIController
    {
        private VisualElement _panelRootUI;
        private VisualElement _queueContainerUI;
        private IDisposable _battleStateChangedSubscription;

        protected override void RegisterUIElements()
        {
            _panelRootUI = _uiDocument.rootVisualElement.Q<VisualElement>(className: "battle-queue");
            _queueContainerUI = _uiDocument.rootVisualElement.Q<VisualElement>("queue-container");

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
        }

        protected override void UnsubscriveFromUIEvents()
        {
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

            switch(stateChanged.ToState)
            {
                case BattleState.TurnEnd:
                case BattleState.RoundStart:
                case BattleState.RoundEnd:
                    UpdateQueue(stateChanged.Context);
                    break;
            }
        }

        private void UpdateQueue(BattleContext context)
        {
            if (_queueContainerUI == null)
            {
                return;
            }

            _queueContainerUI.Clear();

            if (context?.Queue == null)
            {
                return;
            }

            var availableQueue = context.Queue.GetAvailableQueue(10);
            var nextRoundNumber = context.CurrentRoundNumber + 1;

            foreach (var squad in availableQueue)
            {
                if (squad == null)
                {
                    _queueContainerUI.Add(CreateRoundSeparator(nextRoundNumber));
                    nextRoundNumber++;
                    continue;
                }

                _queueContainerUI.Add(CreateEntry(squad));
            }
        }

        private VisualElement CreateEntry(SquadModel squad)
        {
            var entry = new Label(squad?.Unit.Definition.Name ?? ">>")
            {
                name = "battle-queue-entry"
            };

            entry.AddToClassList("battle-queue__entry");
            return entry;
        }

        private VisualElement CreateRoundSeparator(int roundNumber)
        {
            var entry = new Label(roundNumber.ToString())
            {
                name = "battle-queue-entry"
            };

            entry.AddToClassList("battle-queue__entry");
            entry.AddToClassList("battle-queue__separator");
            return entry;
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
