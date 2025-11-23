// Provides battle flow logging for BattleStateMachine transitions and status changes.
using System;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleLogger
    {
        private readonly Action<string> _log;

        public BattleLogger(Action<string> log = null)
        {
            _log = log ?? Debug.Log;
        }

        public void LogTransition(BattleState from, BattleState to, string trigger)
        {
            _log?.Invoke($"[Battle] Transition: {from} --({trigger})--> {to}");
        }

        public void LogStateEnter(BattleState state)
        {
            _log?.Invoke($"[Battle] Enter state: {state}");
        }

        public void LogStateExit(BattleState state)
        {
            _log?.Invoke($"[Battle] Exit state: {state}");
        }

        public void LogStatusChange(BattleStatus status)
        {
            _log?.Invoke($"[Battle] Status changed to {status}");
        }

        public void LogUnhandledTrigger(BattleState state, string trigger)
        {
            _log?.Invoke($"[Battle] Unhandled trigger '{trigger}' in state {state}");
        }
    }
}
