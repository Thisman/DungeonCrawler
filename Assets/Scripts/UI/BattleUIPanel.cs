// Provides a shared UI Toolkit panel base with event bus wiring and visibility helpers for battle UI documents.
using System;
using System.Collections.Generic;
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Battle;
using UnityEngine;
using UnityEngine.UIElements;

namespace DungeonCrawler.UI.Battle
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class BattleUIPanel : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _document;

        private readonly List<IDisposable> _subscriptions = new();

        protected BattleSceneLauncher Launcher { get; private set; }

        protected GameEventBus SceneEventBus { get; private set; }

        protected VisualElement Root => _document?.rootVisualElement;

        protected virtual void Awake()
        {
            _document ??= GetComponent<UIDocument>();
            Hide();
        }

        protected virtual void OnEnable()
        {
            EnsureEventBus();
            RegisterSubscriptions();
        }

        protected virtual void OnDisable()
        {
            UnregisterUiCallbacks();
            DisposeSubscriptions();
        }

        protected abstract void RegisterSubscriptions();

        protected virtual void UnregisterUiCallbacks()
        {
        }

        protected void AddSubscription(IDisposable subscription)
        {
            if (subscription != null)
            {
                _subscriptions.Add(subscription);
            }
        }

        protected void Show()
        {
            if (Root != null)
            {
                Root.style.display = DisplayStyle.Flex;
            }
        }

        protected void Hide()
        {
            if (Root != null)
            {
                Root.style.display = DisplayStyle.None;
            }
        }

        private void EnsureEventBus()
        {
            if (SceneEventBus != null)
            {
                return;
            }

            Launcher = FindObjectOfType<BattleSceneLauncher>();
            SceneEventBus = Launcher?.SceneEventBus;
        }

        private void DisposeSubscriptions()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
        }
    }
}
