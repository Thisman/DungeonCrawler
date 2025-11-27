using DungeonCrawler.Systems.Input;
using System;
using System.Collections.Generic;
using System.Text;
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
        }
    }
}
