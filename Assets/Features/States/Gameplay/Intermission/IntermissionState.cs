using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.UI;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System.Threading.Tasks;
using UnityEngine;
using System;
using NoMoreFishAndChips.Items;

using Object = UnityEngine.Object;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

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

    public abstract class IntermissionSubState : State<EIntermissionState, ENone>, IIntermissionSubState
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
            DepartState departState = new DepartState(_subStateMachine, this);

            arriveState.InitialiseConfig(_config);
            dockState.InitialiseConfig(_config);
            departState.InitialiseConfig(_config);

            _subStateMachine.AddState(EIntermissionState.Arrive, arriveState);
            _subStateMachine.AddState(EIntermissionState.Dock, dockState);
            _subStateMachine.AddState(EIntermissionState.Depart, departState);
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (IIntermissionSubState state in _subStateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        public override void Enter()
        {
            base.Enter();

            _context.LocalPlayer.ReadyLogic.SetNetIsReady(false);
            
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

        public override void Exit()
        {
            // Only needs to be called on the server due to StateSynchroniser
            if (_networkManager.IsServer)
            {
                _subStateMachine.ChangeState(EIntermissionState.None);
            }
        }
        
        public void GoToStageState()
        {
            _networkManager.Despawn(_island);
            _island = null;

            _lobbyManager.StartLobby();

            _parentStateMachine.ChangeState(EGameplayState.Stage);
        }
    }
}