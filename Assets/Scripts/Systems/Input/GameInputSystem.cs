using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonCrawler.Systems.Input
{
    using UnityEngine.InputSystem;

    public enum GameMode
    {
        Dungeon,
        Dialog,
        Battle,
        Menu,
    }

    public class GameInputSystem
    {
        private readonly InputActionAsset _actions;

        public InputActionAsset Actions => _actions;

        public GameInputSystem(InputActionAsset actions)
        {
            _actions = actions;
        }

        public void EnableOnly(params string[] maps)
        {
            foreach (var m in _actions.actionMaps) m.Disable();
            foreach (var name in maps)
                _actions.FindActionMap(name, throwIfNotFound: true).Enable();
        }

        public void ClearBindingMask() => _actions.bindingMask = null;

        public void EnterBattle() => SetMode(GameMode.Battle);

        public void EnterDungeon() => SetMode(GameMode.Dungeon);

        public void EnterMenu() => SetMode(GameMode.Menu);

        public void EnterDialog() => SetMode(GameMode.Dialog);

        private void SetMode(GameMode mode)
        {
            ClearBindingMask();

            switch (mode)
            {
                case GameMode.Dungeon:
                    EnableOnly("Player");
                    break;
                case GameMode.Battle:
                    EnableOnly("Battle");
                    break;
                case GameMode.Menu:
                    EnableOnly("Menu");
                    break;
                case GameMode.Dialog:
                    EnableOnly("Dialog");
                    break;
            }
        }
    }
}
