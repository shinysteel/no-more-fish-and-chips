using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common;
using NoMoreFishAndChips.Scenes;
using PurrNet;

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

    public class StatePath
    {
        private List<Enum> _path;

        public StatePath(List<Enum> path)
        {
            _path = path;
        }

        public bool Contains(Enum stateEnum)
        {
            return _path.Contains(stateEnum);
        }
    }

    public class StateManager : GameSystem<IStateManagerListener>, ISceneManagerListener
    {
        private StateManagerConfig _config;

        private SceneManager _sceneManager;

        private StateMachine<EMainState> _stateMachine;

        private StatePath _currentStatePath;
        public StatePath CurrentStatePath => _currentStatePath;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _sceneManager = GameManager.Instance.Get<SceneManager>();

            _sceneManager.AddListener(this);

            _config = config.StateManagerConfig;

            _stateMachine = new();

            MenusState menusState = new MenusState(_stateMachine);
            GameplayState gameplayState = new GameplayState(_stateMachine);

            menusState.Initialise(_config);
            gameplayState.Initialise(_config);

            _stateMachine.AddState(EMainState.Menus, menusState);
            _stateMachine.AddState(EMainState.Gameplay, gameplayState);

            _stateMachine.OnStateChanged += HandleMainStateChanged;
            gameplayState.SubStateMachine.OnStateChanged += HandleGameplayStateChanged;

            base.InitialiseConfig(config);
        }

        public override void Shutdown()
        {
            _stateMachine.OnStateChanged -= HandleMainStateChanged;
            ((GameplayState)_stateMachine[EMainState.Gameplay]).SubStateMachine.OnStateChanged -= HandleGameplayStateChanged;

            _sceneManager?.RemoveListener(this);

            base.Shutdown();
        }

        public override void Tick()
        {
            _stateMachine.Tick();
        }

        public void ChangeMainState(EMainState state)
        {
            _stateMachine.ChangeState(state);
        }

        private void HandleMainStateChanged(EMainState previous, EMainState current)
        {
            HandleStateChanged();
        }

        private void HandleGameplayStateChanged(EGameplayState previous, EGameplayState current)
        {
            HandleStateChanged();
        }

        private void HandleStateChanged()
        {
            StatePath previous = _currentStatePath;

            List<Enum> path = new();

            path.Add(_stateMachine.CurrentEnum);

            if (_stateMachine.CurrentState is GameplayState gameplayState)
            {
                path.Add(gameplayState.SubStateMachine.CurrentEnum);
            }

            _currentStatePath = new StatePath(path);

            Listeners.Dispatch(listener => listener.OnStatePathChanged(previous, _currentStatePath));
        }

        void ISceneManagerListener.OnSceneUnloaded(EScene scene)
        { 
            // Only once do we listen for the startup scene to unload before starting the state machine
            if (scene == EScene.Startup)
            {
                _sceneManager.RemoveListener(this);
                _stateMachine.ChangeState(EMainState.Menus);
            }
        }
    }
}