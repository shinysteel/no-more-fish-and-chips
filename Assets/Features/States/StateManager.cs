using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common;
using FishFlingers.Scenes;
using PurrNet;

namespace FishFlingers.States
{
    public interface IStateManagerListener
    {
        void OnStateChanged(EMainState previous, EMainState current);
    }

    public enum EMainState
    {
        None,
        Menus,
        Gameplay
    }

    public class StateManager : GameSystem<IStateManagerListener>, ISceneManagerListener
    {
        private StateManagerConfig _config;

        private SceneManager _sceneManager;

        private StateMachine<EMainState> _stateMachine;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.StateManagerConfig;

            _sceneManager = GameManager.Instance.Get<SceneManager>();

            _sceneManager.AddListener(this);

            _stateMachine = new();
            MenusState menusState = new MenusState(_stateMachine);
            GameplayState gameplayState = new GameplayState(_stateMachine);

            IMainState[] states = new IMainState[] { menusState, gameplayState };
            foreach (IMainState state in states)
            {
                state.Initialise(_config);
            }

            _stateMachine.AddState(EMainState.Menus, menusState);
            _stateMachine.AddState(EMainState.Gameplay, gameplayState);

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _sceneManager?.RemoveListener(this);

            base.Shutdown();
        }

        public override void Update()
        {
            _stateMachine.Update();
        }

        public void ChangeState(EMainState state)
        {
            EMainState previous = _stateMachine.CurrentEnum;
            _stateMachine.ChangeState(state);
            Listeners.Dispatch(NotifyOnStateChanged, previous, state);
        }

        private void NotifyOnStateChanged(IStateManagerListener listener, EMainState previous, EMainState current)
        {
            listener.OnStateChanged(previous, current);
        }

        public void OnSceneUnloaded(EScene scene)
        { 
            // Only once do we listen for the startup scene to unload before starting the state machine
            if (scene == EScene.Startup)
            {
                _sceneManager.RemoveListener(this);
                _stateMachine.ChangeState(EMainState.Menus);
            }
        }

        public void OnSceneLoaded(EScene scene, LoadSceneMode mode) { }
        public void OnActiveSceneChanged(EScene previous, EScene current) { }
        public void OnNetworkedSceneLoaded(EScene scene, bool asServer) { }
        public void OnNetworkedSceneUnloaded(EScene scene, bool asServer) { }
        public void OnPlayerLoadedScene(PlayerID playerId, EScene scene, bool asServer) { }
        public void OnPlayerUnloadedScene(PlayerID playerId, EScene scene, bool asServer) { }
    }
}