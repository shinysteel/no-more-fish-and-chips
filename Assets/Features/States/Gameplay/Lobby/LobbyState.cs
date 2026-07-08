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
    public enum ELobbyState
    {
        None,
        Dock,
        Depart
    }

    public interface ILobbySubState
    {
        void InitialiseContext(GameplayContext context);
    }

    public class LobbySubState : State<ELobbyState, ENone>, ILobbySubState
    {
        protected GameplayContext _context;

        public LobbySubState(StateMachine<ELobbyState> parent) : base(parent)
        { }

        public virtual void InitialiseConfig(LobbyStateConfig config)
        { }

        public virtual void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class DockState : LobbySubState
    {
        private DockStateConfig _config;

        private float _startTimer;
        private float _startDuration = 3f;

        public DockState(StateMachine<ELobbyState> parent) : base(parent)
        { }

        public override void InitialiseConfig(LobbyStateConfig config)
        {
            _config = config.DockStateConfig;
        }

        public override void Enter()
        {
            _context.References.Ocean.SetCurrent(false, true);
        }

        public override void Tick()
        {
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

            if (_startTimer >= _startDuration)
            {
                _parentStateMachine.ChangeState(ELobbyState.Depart);
            }
        }
    }

    public class DepartState : LobbySubState
    {
        private DepartStateConfig _config;

        public DepartState(StateMachine<ELobbyState> parent) : base(parent)
        { }

        public override void InitialiseConfig(LobbyStateConfig config)
        {
            _config = config.DepartStateConfig;
        }

        public override void Enter()
        {
            _context.References.Ocean.SetCurrent(true, false);

            float delay = 5f;
            Tween.Delay(delay, onComplete: () => _parentStateMachine.ChangeState(ELobbyState.None));
        }
    }

    public class LobbyState : GameplaySubState<ELobbyState>
    {
        private NetworkManager _networkManager;

        private LobbyStateConfig _config;

        private Island _island;

        public LobbyState(StateMachine<EGameplayState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>(); 
        }

        public override void InitialiseConfig(GameplayStateConfig config)
        {
            _config = config.LobbyStateConfig;

            _subStateMachine = new();

            DockState dockState = new DockState(_subStateMachine);
            DepartState departState = new DepartState(_subStateMachine);

            dockState.InitialiseConfig(_config);
            departState.InitialiseConfig(_config);

            _subStateMachine.AddState(ELobbyState.Dock, dockState);
            _subStateMachine.AddState(ELobbyState.Depart, departState);

            _subStateMachine.OnStateChanged += HandleSubStateChanged;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (ILobbySubState state in _subStateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        ~LobbyState()
        {
            _subStateMachine.OnStateChanged -= HandleSubStateChanged;
        }

        public override void Enter()
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            Vector2Int cell = _context.Raft.Queries.Axes[Axis.Vertical].MinLine.MinEdge.Node.Cell;
            Vector3 position = _context.Raft.Queries.CellToWorldPosition(cell) + Vector3.left * 6f;
            _island = _networkManager.Spawn(_config.IslandPrefab, new SpawnParams() { Position = position });

            _subStateMachine.ChangeState(ELobbyState.Dock);
        }

        private void HandleSubStateChanged(ELobbyState previous, ELobbyState current)
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (current == ELobbyState.None)
            {
                float delay = 5f;
                Tween.Delay(delay, onComplete: () =>
                {
                    _networkManager.Despawn(_island);
                    _island = null;
                });

                _parentStateMachine.ChangeState(EGameplayState.Stage);
            }
        }
    }
}