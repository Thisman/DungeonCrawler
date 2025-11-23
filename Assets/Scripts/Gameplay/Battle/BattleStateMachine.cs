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

        private void ConfigureTransitions()
        {
            ConfigureState(BattleState.Preparation)
                .Permit(Trigger.NextState, BattleState.RoundInit);

            ConfigureState(BattleState.RoundInit)
                .Permit(Trigger.NextState, BattleState.RoundStart);

            ConfigureState(BattleState.RoundStart)
                .Permit(Trigger.NextState, BattleState.TurnInit);

            ConfigureState(BattleState.TurnInit)
                .Permit(Trigger.NextState, BattleState.TurnStart)
                .Permit(Trigger.EndRound, BattleState.RoundEnd)
                .Permit(Trigger.Finish, BattleState.Result);

            ConfigureState(BattleState.TurnStart)
                .Permit(Trigger.NextState, BattleState.WaitForAction);

            ConfigureState(BattleState.WaitForAction)
                .Permit(Trigger.NextState, BattleState.TurnEnd);

            ConfigureState(BattleState.TurnEnd)
                .Permit(Trigger.NextState, BattleState.TurnInit)
                .Permit(Trigger.Finish, BattleState.Result);

            ConfigureState(BattleState.RoundEnd)
                .Permit(Trigger.NextState, BattleState.RoundInit)
                .Permit(Trigger.Finish, BattleState.Result);

            ConfigureState(BattleState.Result)
                .Ignore(Trigger.NextState)
                .Ignore(Trigger.Finish)
                .Ignore(Trigger.EndRound);
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
            _context.Queue = new BattleQueue(_context.Squads);
            _context.Queue.GetAvailableQueue(_context.Squads.Count);
        }

        protected virtual void EnterRoundInit() { }

        protected virtual void ExitRoundInit() { }

        protected virtual void EnterRoundStart() { }

        protected virtual void ExitRoundStart() { }

        protected virtual void EnterTurnInit()
        {
            _context.ActiveUnit = _context.Queue?.GetNext();

            if (_context.ActiveUnit == null)
            {
                _stateMachine.Fire(Trigger.EndRound);
            }
        }

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
            NextState,
            EndRound,
            Finish
        }
    }
}
