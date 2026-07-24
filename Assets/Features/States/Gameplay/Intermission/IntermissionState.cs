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
        {
        }

        public virtual void InitialiseConfig(IntermissionStateConfig config)
        { }

        public virtual void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class ArriveState : IntermissionSubState
    {
        private NetworkManager _networkManager;

        private ArriveStateConfig _config;

        public ArriveState(StateMachine<EIntermissionState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

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

            if (_networkManager.IsServer)
            {
                Tween.Delay(Ocean.DefaultSetCurrentDuration, () => _parentStateMachine.ChangeState(EIntermissionState.Dock));
            }
        }
    }

    public class DockState : IntermissionSubState
    {
        private NetworkManager _networkManager;
        private UIManager _uiManager;
        private CameraManager _cameraManager;
        private SceneManager _sceneManager;

        private DockStateConfig _config;

        private float _startTimer;

        private FixedCameraMode _fixedCameraMode;

        private VoyageResultsScreen _voyageResultsScreen;

        public DockState(StateMachine<EIntermissionState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
        }

        public override void InitialiseConfig(IntermissionStateConfig config)
        {
            _config = config.DockStateConfig;
        }

        public override void Enter()
        {
            base.Enter();

            // Understand that clients can join at any point
            _context.References.Ocean.SetCurrent(false, 0f);

            _startTimer = 0f;

            _ = EnterVoyageResultsAsync();
        }

        private async Task EnterVoyageResultsAsync()
        {
            try
            {
                _context.LocalPlayer.RaftPlayerActLogic.SetInCutscene(true);

                await _sceneManager.LoadSceneAsync(EScene.EnvironmentVoyageResults, LoadSceneMode.Additive, LoadSceneContext.Local);

                _fixedCameraMode = new FixedCameraMode(_cameraManager.Config.VoyageResultsFixedCameraModeSettings);
                _cameraManager.AddMode(_fixedCameraMode);

                _cameraManager.CinemachineBrain.OutputCamera.cullingMask = _config.VoyageResultsMask;

                _voyageResultsScreen = await _uiManager.CreateScreenUIAsync(_uiManager.Config.VoyageResultsScreenPrefab, UILayer.Screens);
                _voyageResultsScreen.Setup(() => _ = ExitVoyageResultsAsync());

                _voyageResultsScreen.Show(null);
                _context.GameplayScreen.Hide(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async Task ExitVoyageResultsAsync()
        {
            try
            {
                await _sceneManager.UnloadSceneAsync(EScene.EnvironmentVoyageResults, LoadSceneContext.Local);

                _cameraManager.RemoveMode(_fixedCameraMode);
                _fixedCameraMode = null;

                _cameraManager.CinemachineBrain.OutputCamera.cullingMask = ~0;

                _uiManager.DestroyScreenUI(_voyageResultsScreen, UILayer.Screens);
                _context.GameplayScreen.Show(null);

                _voyageResultsScreen = null;

                _context.LocalPlayer.RaftPlayerActLogic.SetInCutscene(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        
        public override void Tick()
        {
            base.Tick();

            if (_networkManager.IsServer)
            {
                StartTick();
            }
        }

        // Start counting down once all players are on the raft
        private void StartTick()
        {
            //bool canStart = true;

            //foreach (RaftPlayer player in _context.Players)
            //{
            //    if (!player.RaftPlayerPhysicsModule.OnRaft)
            //    {
            //        canStart = false;
            //        break;
            //    }
            //}

            //if (!canStart)
            //{
            //    _startTimer = 0f;
            //    return;
            //}

            //_startTimer += Time.deltaTime;

            //if (_startTimer >= _config.StartDuration)
            //{
            //    _parentStateMachine.ChangeState(EIntermissionState.Depart);
            //}
        }
    }

    public class DepartState : IntermissionSubState
    {
        private NetworkManager _networkManager;

        private IntermissionState _intermissionState;

        private DepartStateConfig _config;

        public DepartState(StateMachine<EIntermissionState> parent, IntermissionState state) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _intermissionState = state;
        }

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

            if (!_networkManager.IsServer)
            {
                return;
            }

            if (_stateTimer >= _config.DepartDelay)
            {
                _intermissionState.GoToStageState();
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