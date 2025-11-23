// Coordinates battle round and turn flow using Stateless for state transitions with entry/exit hooks for every state.
using System;
using System.Threading.Tasks;
using Stateless;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleStateMachine
    {
        private readonly BattleContext _context;
        private readonly StateMachine<BattleState, Trigger> _stateMachine;
        private readonly BattleLogger _logger;
        private BattleState _currentState;

        public BattleState CurrentState => _currentState;

        public BattleContext Context => _context;

        public BattleStateMachine(BattleContext context, BattleState initialState = BattleState.Preparation, BattleLogger logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? new BattleLogger();
            _currentState = initialState;
            _stateMachine = new StateMachine<BattleState, Trigger>(() => _currentState, state => _currentState = state);

            ConfigureTransitions();
            _stateMachine.OnTransitioned(transition => _logger.LogTransition(transition.Source, transition.Destination, transition.Trigger.ToString()));
            _stateMachine.OnUnhandledTrigger((state, trigger) => _logger.LogUnhandledTrigger(state, trigger.ToString()));
        }

        public void Start()
        {
            EnterState(_currentState);
        }

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
            _logger.LogStateEnter(state);

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
            _logger.LogStateExit(state);

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
            SetStatus(BattleStatus.Preparation);
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitPreparation()
        {
            SetStatus(BattleStatus.Progress);
            _context.Queue = new BattleQueue(_context.Squads);
            _context.Queue.GetAvailableQueue(_context.Squads.Count);
        }

        protected virtual void EnterRoundInit() {
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitRoundInit() { }

        protected virtual void EnterRoundStart() {
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitRoundStart() { }

        protected virtual void EnterTurnInit()
        {
            _context.ActiveUnit = _context.Queue?.GetNext();

            if (_context.ActiveUnit == null)
            {
                _stateMachine.Fire(Trigger.EndRound);
                return;
            }

            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitTurnInit() { }

        protected virtual void EnterTurnStart() {
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitTurnStart() { }

        protected virtual async void EnterWaitForAction() {
            await Task.Delay(TimeSpan.FromSeconds(5));
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitWaitForAction() { }

        protected virtual void EnterTurnEnd() {
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitTurnEnd() { }

        protected virtual void EnterRoundEnd() {
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitRoundEnd() { }

        protected virtual void EnterResult()
        {
            SetStatus(BattleStatus.Result);
            _stateMachine.Fire(Trigger.NextState);
        }

        protected virtual void ExitResult()
        {
            SetStatus(BattleStatus.Finished);
        }

        private void SetStatus(BattleStatus status)
        {
            _context.Status = status;
            _logger.LogStatusChange(status);
        }

        private enum Trigger
        {
            NextState,
            EndRound,
            Finish
        }
    }
}
