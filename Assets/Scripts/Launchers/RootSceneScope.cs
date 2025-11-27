// Configures root scene-level services such as session and scene loading systems.
using DungeonCrawler.Systems.Input;
using DungeonCrawler.Systems.SceneManagement;
using DungeonCrawler.Systems.Session;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace DungeonCrawler.Launchers
{
    public class RootSceneScope : LifetimeScope
    {
        [SerializeField]
        private InputActionAsset _actions;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameSessionSystem>(Lifetime.Singleton);
            builder.Register<SceneLoaderSystem>(Lifetime.Singleton);
            builder.Register<GameInputSystem>(Lifetime.Singleton);
            builder.RegisterInstance<InputActionAsset>(_actions).AsSelf();
        }
    }
}
