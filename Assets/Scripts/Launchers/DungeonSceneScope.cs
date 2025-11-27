// Configures dungeon scene dependencies, including input and scene-level event bus.
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Dungeon;
using DungeonCrawler.Systems.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace DungeonCrawler.Launchers
{
    public class DungeonSceneScope : LifetimeScope
    {
        [SerializeField]
        private InputActionAsset _actions;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<InputActionAsset>(_actions).AsSelf();
            builder.Register<GameInputSystem>(Lifetime.Singleton);
            builder.Register<GameEventBus>(Lifetime.Singleton);

            builder.RegisterBuildCallback(resolver =>
            {
                var controllers = FindObjectsByType<ScenarioController>(FindObjectsSortMode.None);
                foreach (var c in controllers)
                {
                    resolver.Inject(c);
                }
            });
        }
    }
}
