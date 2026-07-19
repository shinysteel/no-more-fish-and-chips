using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common;
using NoMoreFishAndChips.Scenes;
using PurrNet;
using NoMoreFishAndChips.Networking;
using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.States
{
    public interface IStateManagerListener
    {
        void OnStatePathChanged(StatePath previous, StatePath current) { }
    }

    public enum EMainState
    {
        // While the game is initialising, a none state is useful
        None     , 
        Menus    ,
        Gameplay ,
    }

    public abstract class MainState<TParentStateEnum, TSubStateEnum> : State<TParentStateEnum, TSubStateEnum>
        where TParentStateEnum : Enum
        where TSubStateEnum : Enum
    {
        public MainState(StateMachine<TParentStateEnum> parent) : base(parent)
        { }

        public virtual void Initialise(StateManagerConfig config)
        { }
    }

    public class StatePath : IEquatable<StatePath>, IEnumerable<Enum>
    {
        private List<Enum> _enums;

        public StatePath(List<Enum> enums)
        {
            _enums = enums;
        }

        public bool Contains(Enum enumValue)
        {
            return _enums.Contains(enumValue);
        }

        // We want equality to reflect having the same path
        public bool Equals(StatePath other)
        {
            if (other == null)
            {
                return false;
            }

            if (_enums.Count != other._enums.Count)
            {
                return false;
            }

            for (int i = 0; i < _enums.Count; i++)
            {
                if (!_enums[i].Equals(other._enums[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // It's best practice to override GetHashCode when implementing IEquatable
        public override int GetHashCode()
        {
            HashCode code = new();

            foreach (Enum stateEnum in _enums)
            {
                code.Add(stateEnum);
            }

            return code.ToHashCode();
        }

        public IEnumerator<Enum> GetEnumerator()
        {
            return _enums.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class StateManager : GameSystem<IStateManagerListener>, ISceneManagerListener
    {
        private StateManagerConfig _config;

        private SceneManager _sceneManager;
        private NetworkManager _networkManager;

        private StateMachine<EMainState> _stateMachine;

        private StatePath _currentStatePath;
        public StatePath CurrentStatePath => _currentStatePath;

        private StateSynchroniser _stateSynchroniser;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _sceneManager.AddListener(this);

            _config = config.StateManagerConfig;

            _stateMachine = new();

            MenusState menusState = new MenusState(_stateMachine);
            GameplayState gameplayState = new GameplayState(_stateMachine);

            menusState.Initialise(_config);
            gameplayState.Initialise(_config);

            _stateMachine.AddState(EMainState.Menus, menusState);
            _stateMachine.AddState(EMainState.Gameplay, gameplayState);

            TraverseStateMachines((IStateMachine machine) => machine.OnStateChanged += HandleStateChanged);

            base.InitialiseConfig(config);
        }

        public override void Shutdown()
        {
            TraverseStateMachines((IStateMachine machine) => machine.OnStateChanged -= HandleStateChanged);

            _sceneManager?.RemoveListener(this);

            base.Shutdown();
        }

        private void TraverseStateMachines(Action<IStateMachine> action)
        {
            void recurse(IStateMachine machine)
            {
                action(machine);

                foreach (IState state in machine)
                {
                    if (state.SubStateMachine != null)
                    {
                        recurse(state.SubStateMachine);
                    }
                }
            }

            recurse(_stateMachine);
        }

        public override void Tick()
        {
            _stateMachine.Tick();
        }

        public void ChangeMainState(EMainState state)
        {   
            _stateMachine.ChangeState(state);
        }

        private void HandleStateChanged(Enum previous, Enum current)
        {
            RefreshCurrentStatePath();
        }

        private void RefreshCurrentStatePath()
        {
            StatePath previous = _currentStatePath;

            List<Enum> enums = new();
            IStateMachine machine = _stateMachine;

            while (machine != null)
            {
                enums.Add(machine.CurrentEnum);
                machine = machine.CurrentState?.SubStateMachine;
            }

            StatePath current = new StatePath(enums);

            if (current.Equals(previous))
            {
                return;
            }

            _currentStatePath = current;

            Listeners.Dispatch(listener => listener.OnStatePathChanged(previous, _currentStatePath));
        }

        void ISceneManagerListener.OnSceneUnloaded(EScene scene)
        {
            // Only once do we listen for the startup scene to unload before starting the state machine
            if (scene == EScene.Startup)
            {
                _stateMachine.ChangeState(EMainState.Menus);
            }   
        }

        void ISceneManagerListener.OnNetworkedSceneLoaded(EScene scene, bool asServer)
        {
            if (asServer && scene == EScene.Game)
            {
                 _stateSynchroniser = _networkManager.Spawn(_config.StateSynchroniserPrefab);
            }
        }

        public void ReadStatePathEnumValues(List<int> enumValues)
        {
            IStateMachine machine = _stateMachine;

            for (int i = 0; i < enumValues.Count; i++)
            {
                if (Convert.ToInt32(machine.CurrentEnum) != enumValues[i])
                {
                    machine.ChangeState(enumValues[i]);
                }
                
                machine = machine.CurrentState?.SubStateMachine;
            }
        }
    }
}