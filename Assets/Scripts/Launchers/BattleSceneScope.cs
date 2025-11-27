using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Systems.Battle;
using DungeonCrawler.Systems.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace DungeonCrawler.Launchers
{
    public class BattleSceneScope : LifetimeScope
    {
        [SerializeField]
        private BattleGridController _battleGridController;

        [SerializeField]
        private BattleTargetPicker _battleTargetPicker;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UnitSystem>(Lifetime.Singleton);
            builder.Register<GameEventBus>(Lifetime.Singleton);
            builder.Register<BattleContext>(Lifetime.Singleton);
            builder.Register<BattleDamageSystem>(Lifetime.Singleton);
            builder.Register<BattleStateMachine>(Lifetime.Singleton);

            builder.RegisterInstance<BattleTargetPicker>(_battleTargetPicker).AsSelf();
            builder.RegisterInstance<BattleGridController>(_battleGridController).AsSelf();
        }
    }
}
