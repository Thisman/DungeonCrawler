// Coordinates battle round and turn flow using Stateless for state transitions with entry/exit hooks for every state.
using System;
using Stateless;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleStateMachine
    {
        private readonly BattleContext _context;
        private readonly StateMachine<BattleState, Trigger> _stateMachine;
        private BattleState _currentState;

        public BattleStateMachine(BattleContext context, BattleState initialState = BattleState.Preparation)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentState = initialState;
            _stateMachine = new StateMachine<BattleState, Trigger>(() => _currentState, state => _currentState = state);

            ConfigureTransitions();
        }

        public BattleState CurrentState => _currentState;

        public BattleContext Context => _context;

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

            ConfigureState(BattleState.Result)
                .Ignore(Trigger.Advance)
                .Ignore(Trigger.Finish);
        }

        private void ConfigureFlowState(BattleState from, BattleState advanceTo)
        {
            ConfigureState(from)
                .Permit(Trigger.Advance, advanceTo)
                .Permit(Trigger.Finish, BattleState.Result);
        }

        private StateMachine<BattleState, Trigger>.StateConfiguration ConfigureState(BattleState state)
        {
            return _stateMachine.Configure(state)
                .OnEntry(() => EnterState(state))
                .OnExit(() => ExitState(state));
        }

        private void EnterState(BattleState state)
        {
            switch (state)
            {
                case BattleState.Preparation:
                    EnterPreparation();
                    break;
                case BattleState.RoundInit:
                    EnterRoundInit();
                    break;
                case BattleState.RoundStart:
                    EnterRoundStart();
                    break;
                case BattleState.TurnInit:
                    EnterTurnInit();
                    break;
                case BattleState.TurnStart:
                    EnterTurnStart();
                    break;
                case BattleState.WaitForAction:
                    EnterWaitForAction();
                    break;
                case BattleState.TurnEnd:
                    EnterTurnEnd();
                    break;
                case BattleState.RoundEnd:
                    EnterRoundEnd();
                    break;
                case BattleState.Result:
                    EnterResult();
                    break;
            }
        }

        private void ExitState(BattleState state)
        {
            switch (state)
            {
                case BattleState.Preparation:
                    ExitPreparation();
                    break;
                case BattleState.RoundInit:
                    ExitRoundInit();
                    break;
                case BattleState.RoundStart:
                    ExitRoundStart();
                    break;
                case BattleState.TurnInit:
                    ExitTurnInit();
                    break;
                case BattleState.TurnStart:
                    ExitTurnStart();
                    break;
                case BattleState.WaitForAction:
                    ExitWaitForAction();
                    break;
                case BattleState.TurnEnd:
                    ExitTurnEnd();
                    break;
                case BattleState.RoundEnd:
                    ExitRoundEnd();
                    break;
                case BattleState.Result:
                    ExitResult();
                    break;
            }
        }

        protected virtual void EnterPreparation()
        {
            _context.Status = BattleStatus.Preparation;
        }

        protected virtual void ExitPreparation()
        {
            _context.Status = BattleStatus.Progress;
        }

        protected virtual void EnterRoundInit() { }

        protected virtual void ExitRoundInit() { }

        protected virtual void EnterRoundStart() { }

        protected virtual void ExitRoundStart() { }

        protected virtual void EnterTurnInit() { }

        protected virtual void ExitTurnInit() { }

        protected virtual void EnterTurnStart() { }

        protected virtual void ExitTurnStart() { }

        protected virtual void EnterWaitForAction() { }

        protected virtual void ExitWaitForAction() { }

        protected virtual void EnterTurnEnd() { }

        protected virtual void ExitTurnEnd() { }

        protected virtual void EnterRoundEnd() { }

        protected virtual void ExitRoundEnd() { }

        protected virtual void EnterResult()
        {
            _context.Status = BattleStatus.Result;
        }

        protected virtual void ExitResult()
        {
            _context.Status = BattleStatus.Finished;
        }

        private enum Trigger
        {
            Advance,
            Finish
        }
    }
}
