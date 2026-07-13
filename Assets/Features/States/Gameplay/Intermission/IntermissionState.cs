using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Networking;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    public enum EIntermissionState
    {
        None,
        Arrive,
        Dock,
        Depart
    }

    public interface IIntermissionSubState
    {
        void InitialiseContext(GameplayContext context);
    }

    public class IntermissionSubState : State<EIntermissionState, ENone>, IIntermissionSubState
    {
        protected GameplayContext _context;

        public IntermissionSubState(StateMachine<EIntermissionState> parent) : base(parent)
        { }

        public virtual void InitialiseConfig(IntermissionStateConfig config)
        { }

        public virtual void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class ArriveState : IntermissionSubState
    {
        private ArriveStateConfig _config;

        public ArriveState(StateMachine<EIntermissionState> parent) : base(parent)
        { }

        public override void InitialiseConfig(IntermissionStateConfig config)
        {
            _config = config.ArriveStateConfig;
        }

        public override void Enter()
        {
            base.Enter();

            Tween.Delay(_config.ArriveDelay, Arrive);
        }

        private void Arrive()
        {
            _context.References.Ocean.SetCurrent(false, Ocean.DefaultSetCurrentDuration);

            Tween.Delay(Ocean.DefaultSetCurrentDuration, () => _parentStateMachine.ChangeState(EIntermissionState.Dock));
        }
    }

    public class DockState : IntermissionSubState
    {
        private DockStateConfig _config;

        private float _startTimer;

        public DockState(StateMachine<EIntermissionState> parent) : base(parent)
        { }

        public override void InitialiseConfig(IntermissionStateConfig config)
        {
            _config = config.DockStateConfig;
        }

        public override void Enter()
        {
            base.Enter();

            _startTimer = 0f;
        }
        
        public override void Tick()
        {
            base.Tick();

            StartTick();
        }

        // Start counting down once all players are on the raft
        private void StartTick()
        {
            bool canStart = true;

            foreach (RaftPlayer player in _context.Players)
            {
                if (!player.RaftPlayerPhysicsModule.OnRaft)
                {
                    canStart = false;
                    break;
                }
            }

            if (!canStart)
            {
                _startTimer = 0f;
                return;
            }

            _startTimer += Time.deltaTime;

            if (_startTimer >= _config.StartDuration)
            {
                _parentStateMachine.ChangeState(EIntermissionState.Depart);
            }
        }
    }

    public class DepartState : IntermissionSubState
    {
        private DepartStateConfig _config;

        public DepartState(StateMachine<EIntermissionState> parent) : base(parent)
        { }

        public override void InitialiseConfig(IntermissionStateConfig config)
        {
            _config = config.DepartStateConfig;
        }

        public override void Enter()
        {
            base.Enter();

            _context.References.Ocean.SetCurrent(true, Ocean.DefaultSetCurrentDuration);
        }

        public override void Tick()
        {
            base.Tick();

            if (_stateTimer >= _config.DepartDelay)
            {
                _parentStateMachine.ChangeState(EIntermissionState.None);
            }
        }
    }

    public class IntermissionState : GameplaySubState<EIntermissionState>
    {
        private NetworkManager _networkManager;
        private LobbyManager _lobbyManager;

        private IntermissionStateConfig _config;

        private Island _island;

        public IntermissionState(StateMachine<EGameplayState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
        }

        public override void InitialiseConfig(GameplayStateConfig config)
        {
            _config = config.IntermissionStageConfig;

            _subStateMachine = new();

            ArriveState arriveState = new ArriveState(_subStateMachine);
            DockState dockState = new DockState(_subStateMachine);
            DepartState departState = new DepartState(_subStateMachine);

            arriveState.InitialiseConfig(_config);
            dockState.InitialiseConfig(_config);
            departState.InitialiseConfig(_config);

            _subStateMachine.AddState(EIntermissionState.Arrive, arriveState);
            _subStateMachine.AddState(EIntermissionState.Dock, dockState);
            _subStateMachine.AddState(EIntermissionState.Depart, departState);

            _subStateMachine.OnStateChanged += HandleSubStateChanged;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (IIntermissionSubState state in _subStateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        ~IntermissionState()
        {
            _subStateMachine.OnStateChanged -= HandleSubStateChanged;
        }

        public override void Enter()
        {
            base.Enter();

            if (!_networkManager.IsServer)
            {
                return;
            }
            
            Vector2Int cell = _context.Raft.Queries.Axes[Axis.Vertical].MinLine.MinEdge.Node.Cell;
            Vector3 position = _context.Raft.Queries.CellToWorldPosition(cell) + Vector3.left * _config.IslandOffset;
            _island = _networkManager.Spawn(_config.IslandPrefab, new SpawnParams() { Position = position });

            if (!_lobbyManager.CurrentLobby.GetBool(Lobby.StartedKey))
            {
                _context.References.Ocean.SetCurrent(false, 0f);

                _subStateMachine.ChangeState(EIntermissionState.Dock);
            }
            else
            {
                _island.transform.position += Vector3.forward * (5f + Ocean.DefaultSetCurrentDuration * 0.5f);

                _subStateMachine.ChangeState(EIntermissionState.Arrive);
            }
        }

        private void HandleSubStateChanged(EIntermissionState previous, EIntermissionState current)
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (current == EIntermissionState.None)
            {
                _networkManager.Despawn(_island);
                _island = null;   

                _lobbyManager.StartLobby();

                _parentStateMachine.ChangeState(EGameplayState.Stage);
            }
        }
    }
}