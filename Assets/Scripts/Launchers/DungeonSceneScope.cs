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
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameEventBus>(Lifetime.Singleton);
            builder.RegisterBuildCallback(resolver =>
            {
                var sceneControllers = FindObjectsByType<ScenarioController>(FindObjectsSortMode.None);
                foreach (var c in sceneControllers)
                {
                    resolver.Inject(c);
                }
            });
        }
    }
}
