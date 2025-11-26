// Coordinates battle round and turn flow using Stateless for state transitions with entry/exit hooks for every state.
using DungeonCrawler.Core.EventBus;
using DungeonCrawler.Gameplay.Squad;
using DungeonCrawler.Systems.Battle;
using Stateless;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DungeonCrawler.Gameplay.Battle
{
    public class BattleStateMachine
    {
        private bool _isStopped;
        private BattleState _currentState;
        private readonly BattleLogger _logger;
        private readonly BattleContext _context;
        private readonly GameEventBus _sceneEventBus;
        private readonly List<IDisposable> _subscribtions = new();
        private readonly BattleActionExecutor _battleActionExecutor;
        private readonly StateMachine<BattleState, Trigger> _stateMachine;
        private readonly Dictionary<string, IBattleController> _unitControllers = new();

        public BattleState CurrentState => _currentState;

        public BattleContext Context => _context;

        public BattleStateMachine(BattleContext context, GameEventBus sceneEventBus, UnitSystem unitSystem, BattleDamageSystem battleDamageSystem)
        {
            _context = context;
            _logger = new BattleLogger();
            _sceneEventBus = sceneEventBus;
            _currentState = BattleState.None;
            _stateMachine = new StateMachine<BattleState, Trigger>(() => _currentState, state => _currentState = state);
            _battleActionExecutor = new BattleActionExecutor(_sceneEventBus, unitSystem, battleDamageSystem);

            _unitControllers.Add("Player", new PlayerController(_sceneEventBus));
            var availableActionForEnemies = new List<UnitAction>()
            {
                new UnitAttackAction(),
                new UnitWaitAction(),
                new UnitAbilityAction(),
                new UnitSkipTurnAction(),
            };
            _unitControllers.Add("Enemy", new AIController(availableActionForEnemies, _sceneEventBus));

            ConfigureTransitions();
            _stateMachine.OnTransitioned(transition => {
                _logger.LogTransition(transition.Source, transition.Destination, transition.Trigger.ToString());
                _sceneEventBus.Publish(new BattleStateChanged(transition.Source, transition.Destination, _context));
            });
            _stateMachine.OnUnhandledTrigger((state, trigger) => {
                _logger.LogUnhandledTrigger(state, trigger.ToString());
            });
        }

        public void Start()
        {
            Fire(Trigger.NextState);
        }

        public void Stop()
        {
            _isStopped = true;
            UnsubscribeFromSceneEvents();
        }

        private void ConfigureTransitions()
        {
            ConfigureState(BattleState.None)
                .Permit(Trigger.NextState, BattleState.Preparation);

            ConfigureState(BattleState.Preparation)
                .Permit(Trigger.NextState, BattleState.RoundInit);

            ConfigureState(BattleState.RoundInit)
                .Permit(Trigger.NextState, BattleState.RoundStart)
                .Permit(Trigger.Result, BattleState.Result);

            ConfigureState(BattleState.RoundStart)
                .Permit(Trigger.NextState, BattleState.TurnInit)
                .Permit(Trigger.Result, BattleState.Result);

            ConfigureState(BattleState.TurnInit)
                .Permit(Trigger.NextState, BattleState.TurnStart)
                .Permit(Trigger.EndRound, BattleState.RoundEnd)
                .Permit(Trigger.Result, BattleState.Result);

            ConfigureState(BattleState.TurnStart)
                .Permit(Trigger.NextState, BattleState.WaitForAction)
                .Permit(Trigger.Result, BattleState.Result);

            ConfigureState(BattleState.WaitForAction)
                .Permit(Trigger.NextState, BattleState.TurnEnd)
                .Permit(Trigger.Result, BattleState.Result);

            ConfigureState(BattleState.TurnEnd)
                .Permit(Trigger.NextState, BattleState.TurnInit)
                .Permit(Trigger.Result, BattleState.Result);

            ConfigureState(BattleState.RoundEnd)
                .Permit(Trigger.NextState, BattleState.RoundInit)
                .Permit(Trigger.Result, BattleState.Result);

            ConfigureState(BattleState.Result)
                .Permit(Trigger.Finish, BattleState.Finish)
                .Ignore(Trigger.NextState)
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
            SubscribeToSceneEvents();
        }

        private void ExitPreparation()
        {
            SetStatus(BattleStatus.Progress);
            _context.Queue = new BattleQueue(_context.Squads);
        }

        private void EnterRoundInit() {
            _context.CurrentRoundNumber++;
            _logger.LogRoundCount(_context.CurrentRoundNumber);
            Fire(Trigger.NextState);
        }

        private void ExitRoundInit() { }

        private void EnterRoundStart() {
            Fire(Trigger.NextState);
        }

        private void ExitRoundStart() { }

        private void EnterTurnInit()
        {
            if (TryResolveBattle())
            {
                return;
            }

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
            SquadModel actor = _context.ActiveUnit;
            if (actor == null)
            {
                TryFire(Trigger.NextState);
                return;
            }

            IBattleController controller = actor.Unit.Definition.IsFriendly() ?
                _unitControllers.GetValueOrDefault("Player") :
                _unitControllers.GetValueOrDefault("Enemy");

            try
            {
                using var cts = new CancellationTokenSource();
                if (_isStopped)
                    cts.Cancel();

                var planned = await controller.DecideActionAsync(actor, _context, cts.Token);
                _context.PlannedActiion = planned;
                _sceneEventBus.Publish<UnitPlanSelected>(new UnitPlanSelected(planned));

                if (_isStopped)
                    cts.Cancel();

                await _battleActionExecutor.ExecuteAsync(planned, _context);
                if (TryResolveBattle())
                {
                    return;
                }

                Fire(Trigger.NextState);
            }
            catch (OperationCanceledException)
            {
                //
            }
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
            UnsubscribeFromSceneEvents();
        }

        private void SetStatus(BattleStatus status)
        {
            _context.Status = status;
            _logger.LogStatusChange(status);
        }

        private bool TryResolveBattle(bool playerRequestedFlee = false)
        {
            if (_context?.Result == null)
            {
                return false;
            }

            if (_context.Result.TryEvaluate(playerRequestedFlee))
            {
                TryFire(Trigger.Result);
                return true;
            }

            return false;
        }

        private void Fire(Trigger trigger)
        {
            if (_isStopped)
            {
                return;
            }

            _stateMachine.Fire(trigger);
        }

        private void TryFire(Trigger trigger)
        {
            if (_isStopped)
            {
                return;
            }

            if (_stateMachine.CanFire(trigger))
            {
                Fire(trigger);
            }
        }

        private void SubscribeToSceneEvents()
        {
            _subscribtions.Add(_sceneEventBus.Subscribe<RequestBattlePreparationFinish>((RequestBattlePreparationFinish _) => TryFire(Trigger.NextState)));
            _subscribtions.Add(_sceneEventBus.Subscribe<RequestFleeFromBattle>(HandleFleeFromBattle));
            _subscribtions.Add(_sceneEventBus.Subscribe<RequestFinishBattle>((RequestFinishBattle _) => TryFire(Trigger.Finish)));
        }

        private void UnsubscribeFromSceneEvents()
        {
            _subscribtions.ForEach(subscribtion => subscribtion.Dispose());
            _subscribtions.Clear();
        }

        private void HandleFleeFromBattle(RequestFleeFromBattle _)
        {
            if (TryResolveBattle(playerRequestedFlee: true))
            {
                return;
            }

            TryFire(Trigger.Result);
        }

        private enum Trigger
        {
            NextState,
            EndRound,
            Result,
            Finish
        }
    }
}
