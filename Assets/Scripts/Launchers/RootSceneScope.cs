// Configures root scene-level services such as session and scene loading systems.
using DungeonCrawler.Systems.SceneManagement;
using DungeonCrawler.Systems.Session;
using VContainer;
using VContainer.Unity;

namespace DungeonCrawler.Launchers
{
    public class RootSceneScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameSessionSystem>(Lifetime.Singleton);
            builder.Register<SceneLoaderSystem>(Lifetime.Singleton);
        }
    }
}
