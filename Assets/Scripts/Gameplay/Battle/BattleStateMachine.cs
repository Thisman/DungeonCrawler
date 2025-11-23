// Coordinates battle round and turn flow using Stateless for state transitions.
using Stateless;

namespace DungeonCrawler.Gameplay.Battle
{
    public enum BattleState
    {
        Preparation,
        RoundInit,
        RoundStart,
        TurnInit,
        TurnStart,
        WaitForAction,
        TurnEnd,
        RoundEnd,
        Result
    }

    public class BattleStateMachine
    {
        private readonly StateMachine<BattleState, Trigger> _stateMachine;
        private BattleState _currentState;

        public BattleStateMachine(BattleState initialState = BattleState.Preparation)
        {
            _currentState = initialState;
            _stateMachine = new StateMachine<BattleState, Trigger>(() => _currentState, state => _currentState = state);

            ConfigureTransitions();
        }

        public BattleState CurrentState => _currentState;

        public bool IsTerminal => CurrentState == BattleState.Result;

        public void MoveNext()
        {
            if (IsTerminal)
            {
                return;
            }

            _stateMachine.Fire(Trigger.Advance);
        }

        public void CompleteBattle()
        {
            if (IsTerminal)
            {
                return;
            }

            _stateMachine.Fire(Trigger.Finish);
        }

        public void SetState(BattleState state)
        {
            _currentState = state;
        }

        private void ConfigureTransitions()
        {
            ConfigureFlowState(BattleState.Preparation, BattleState.RoundInit);
            ConfigureFlowState(BattleState.RoundInit, BattleState.RoundStart);
            ConfigureFlowState(BattleState.RoundStart, BattleState.TurnInit);
            ConfigureFlowState(BattleState.TurnInit, BattleState.TurnStart);
            ConfigureFlowState(BattleState.TurnStart, BattleState.WaitForAction);
            ConfigureFlowState(BattleState.WaitForAction, BattleState.TurnEnd);
            ConfigureFlowState(BattleState.TurnEnd, BattleState.TurnInit);
            ConfigureFlowState(BattleState.RoundEnd, BattleState.RoundInit);

            _stateMachine.Configure(BattleState.Result)
                .Ignore(Trigger.Advance)
                .Ignore(Trigger.Finish);
        }

        private void ConfigureFlowState(BattleState from, BattleState advanceTo)
        {
            _stateMachine.Configure(from)
                .Permit(Trigger.Advance, advanceTo)
                .Permit(Trigger.Finish, BattleState.Result);
        }

        private enum Trigger
        {
            Advance,
            Finish
        }
    }
}
