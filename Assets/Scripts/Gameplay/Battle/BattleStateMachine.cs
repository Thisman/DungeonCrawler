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
        private bool _isStopped;

        public Action<BattleState, BattleState, string, BattleContext> OnTransitioned;

        public BattleState CurrentState => _currentState;

        public BattleContext Context => _context;

        public BattleStateMachine(BattleContext context, BattleState initialState = BattleState.Preparation, BattleLogger logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? new BattleLogger();
            _currentState = initialState;
            _stateMachine = new StateMachine<BattleState, Trigger>(() => _currentState, state => _currentState = state);

            ConfigureTransitions();
            _stateMachine.OnTransitioned(transition => {
                _logger.LogTransition(transition.Source, transition.Destination, transition.Trigger.ToString());
                OnTransitioned?.Invoke(transition.Source, transition.Destination, transition.Trigger.ToString(), _context);
            });
            _stateMachine.OnUnhandledTrigger((state, trigger) => {
                _logger.LogUnhandledTrigger(state, trigger.ToString());
            });
        }

        public void Start()
        {
            EnterState(_currentState);
        }

        public void Stop()
        {
            _isStopped = true;
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

        private void EnterPreparation()
        {
            SetStatus(BattleStatus.Preparation);
            Fire(Trigger.NextState);
        }

        private void ExitPreparation()
        {
            SetStatus(BattleStatus.Progress);
            _context.Queue = new BattleQueue(_context.Squads);
            _context.Queue.GetAvailableQueue(_context.Squads.Count);
        }

        private void EnterRoundInit() {
            Fire(Trigger.NextState);
        }

        private void ExitRoundInit() { }

        private void EnterRoundStart() {
            Fire(Trigger.NextState);
        }

        private void ExitRoundStart() { }

        private void EnterTurnInit()
        {
            _context.ActiveUnit = _context.Queue?.GetNext();

            if (_context.ActiveUnit == null)
            {
                Fire(Trigger.EndRound);
                return;
            }

            Fire(Trigger.NextState);
        }

        private void ExitTurnInit() { }

        private void EnterTurnStart() {
            Fire(Trigger.NextState);
        }

        private void ExitTurnStart() { }

        private async void EnterWaitForAction() {
            await Task.Delay(TimeSpan.FromSeconds(5));
            Fire(Trigger.NextState);
        }

        private void ExitWaitForAction() { }

        private void EnterTurnEnd() {
            Fire(Trigger.NextState);
        }

        private void ExitTurnEnd() { }

        private void EnterRoundEnd() {
            Fire(Trigger.NextState);
        }

        private void ExitRoundEnd() { }

        private void EnterResult()
        {
            SetStatus(BattleStatus.Result);
            Fire(Trigger.NextState);
        }

        private void ExitResult()
        {
            SetStatus(BattleStatus.Finished);
        }

        private void SetStatus(BattleStatus status)
        {
            _context.Status = status;
            _logger.LogStatusChange(status);
        }

        private void Fire(Trigger trigger)
        {
            if (_isStopped)
            {
                return;
            }

            _stateMachine.Fire(trigger);
        }

        private enum Trigger
        {
            NextState,
            EndRound,
            Finish
        }
    }
}
