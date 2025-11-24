// Renders the upcoming unit order for the active battle queue with animated transitions.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.UI.Common;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    public class BattleQueuePanel : BaseUIController
    {
        private const float FadeDuration = 0.25f;

        private VisualElement _panelRootUI;
        private VisualElement _queueContainerUI;
        private IDisposable _battleStateChangedSubscription;
        private int _animationVersion;

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

            switch (stateChanged.ToState)
            {
                case BattleState.TurnEnd:
                case BattleState.RoundStart:
                case BattleState.RoundEnd:
                    UpdateQueue(stateChanged.Context);
                    break;
            }
        }

        private async void UpdateQueue(BattleContext context)
        {
            if (_queueContainerUI == null)
            {
                return;
            }

            var entries = BuildEntries(context);
            var currentKeys = _queueContainerUI.Children().Select(child => child.userData as string).ToList();
            var targetKeys = entries.Select(entry => entry.Key).ToList();

            if (currentKeys.SequenceEqual(targetKeys))
            {
                return;
            }

            CancelActiveAnimations();
            var localVersion = ++_animationVersion;

            await FadeOutExistingEntriesAsync();

            if (localVersion != _animationVersion)
            {
                return;
            }

            var createdEntries = new List<VisualElement>();

            _queueContainerUI.Clear();

            foreach (var entry in entries)
            {
                var element = CreateEntry(entry);
                element.style.opacity = 0f;
                createdEntries.Add(element);
                _queueContainerUI.Add(element);
            }

            await FadeInEntriesAsync(createdEntries);
        }

        private VisualElement CreateEntry(BattleQueueEntry entry)
        {
            var entryElement = new Label(entry.Label)
            {
                name = "battle-queue-entry"
            };

            entryElement.userData = entry.Key;
            entryElement.AddToClassList("battle-queue__entry");
            entryElement.AddToClassList("battle-queue__entry--transition");
            return entryElement;
        }

        private void Show()
        {
            _panelRootUI?.AddToClassList("panel--active");
        }

        private void Hide()
        {
            _panelRootUI?.RemoveFromClassList("panel--active");
        }

        private List<BattleQueueEntry> BuildEntries(BattleContext context)
        {
            var entries = new List<BattleQueueEntry>();

            if (context?.Queue == null || context.Squads == null)
            {
                return entries;
            }

            var availableQueue = context.Queue.GetAvailableQueue(context.Squads.Count);

            var upcomingRoundNumber = context.CurrentRoundNumber + 1;

            foreach (var squad in availableQueue)
            {
                if (squad == null)
                {
                    entries.Add(new BattleQueueEntry($"round-{upcomingRoundNumber}", upcomingRoundNumber.ToString()));
                    upcomingRoundNumber++;
                    continue;
                }

                entries.Add(new BattleQueueEntry($"squad-{squad.Unit.Id}", squad.Unit.Definition.Name));
            }

            return entries;
        }

        private Task FadeOutExistingEntriesAsync()
        {
            return FadeEntriesAsync(_queueContainerUI.Children(), 0f, removeAfterFade: true);
        }

        private Task FadeInEntriesAsync(IEnumerable<VisualElement> elements)
        {
            return FadeEntriesAsync(elements, 1f, removeAfterFade: false);
        }

        private Task FadeEntriesAsync(IEnumerable<VisualElement> elements, float endValue, bool removeAfterFade)
        {
            var targets = elements.ToList();

            if (targets.Count == 0)
            {
                return Task.CompletedTask;
            }

            var completionSource = new TaskCompletionSource<bool>();
            var sequence = DOTween.Sequence();

            foreach (var element in targets)
            {
                var tween = DOTween.To(() => element.resolvedStyle.opacity, value => element.style.opacity = value, endValue, FadeDuration)
                    .SetTarget(element);

                if (removeAfterFade)
                {
                    tween.OnComplete(() => element.RemoveFromHierarchy());
                }

                sequence.Join(tween);
            }

            sequence.OnComplete(() => completionSource.TrySetResult(true));
            sequence.Play();

            return completionSource.Task;
        }

        private void CancelActiveAnimations()
        {
            foreach (var child in _queueContainerUI.Children())
            {
                DOTween.Kill(child, complete: false);
            }
        }

        private sealed class BattleQueueEntry
        {
            public BattleQueueEntry(string key, string label)
            {
                Key = key;
                Label = label;
            }

            public string Key { get; }

            public string Label { get; }
        }
    }
}
