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
        void OnMainStateChanged(EMainState previous, EMainState current) { }
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

    public class StateManager : GameSystem<IStateManagerListener>, ISceneManagerListener
    {
        private StateManagerConfig _config;

        private SceneManager _sceneManager;

        private StateMachine<EMainState> _stateMachine;

        public override void Initialise(GameManagerConfig config)
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

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _stateMachine.OnStateChanged -= HandleMainStateChanged;

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
            Listeners.Dispatch(listener => listener.OnMainStateChanged(previous, current));
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