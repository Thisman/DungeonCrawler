// Renders the upcoming unit order for the active battle queue.
using System.Collections.Generic;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleQueuePanel : BattleUIPanel
    {
        private VisualElement _queueContainer;

        protected override void OnPanelAttachedToPanel()
        {
            ResolveElements();
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

        private void HandleBattleStateChanged(BattleStateChanged stateChanged)
        {
            UpdateQueue(stateChanged.Context);

            if (stateChanged.FromState == BattleState.Preparation)
            {
                Show();
            }

            if (stateChanged.ToState == BattleState.Result)
            {
                Hide();
            }
        }

        private void UpdateQueue(BattleContext context)
        {
            if (_queueContainer == null)
            {
                return;
            }

            var items = context?.Queue?.GetAvailableQueue(context.Squads.Count) ?? new List<SquadModel?>();

            _queueContainer.Clear();

            foreach (var entry in items)
            {
                var label = new Label(entry?.Unit.Definition.Name ?? ">>");
                label.AddToClassList("battle-queue__entry");

                _queueContainer.Add(label);
            }
        }

        private void ResolveElements()
        {
            _queueContainer ??= Root?.Q<VisualElement>("queue-container");
        }
    }
}
