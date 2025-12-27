using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common;
using FishFlingers.Scenes;

namespace FishFlingers.States
{
    public interface IStateManagerListener
    { }

    public enum MainState
    {
        None,
        Menus,
        Gameplay
    }

    public class StateManager : GameSystem<IStateManagerListener>, ISceneManagerListener
    {
        private StateManagerConfig _config;

        private SceneManager _sceneManager;

        private StateMachine<MainState> _stateMachine;
        private MenusState _menusState;
        private GameplayState _gameplayState;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.StateManagerConfig;

            _sceneManager = GameManager.Instance.Get<SceneManager>();

            _sceneManager.AddListener(this);

            _stateMachine = new();
            _menusState = new MenusState(_stateMachine);
            _gameplayState = new GameplayState(_stateMachine);

            _stateMachine.AddState(MainState.Menus, _menusState);
            _stateMachine.AddState(MainState.Gameplay, _gameplayState);

            base.Initialise(gameManagerConfig);
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

        public void ChangeState(MainState state)
        {
            _stateMachine.ChangeState(state);
        }

        public void OnSceneUnloaded(EScene scene)
        { 
            // Only once do we listen for the startup scene to unload before starting the state machine
            if (scene == EScene.Startup)
            {
                _sceneManager.RemoveListener(this);
                _stateMachine.ChangeState(MainState.Menus);
            }
        }

        public void OnSceneLoaded(EScene scene, LoadSceneMode mode) { }
    }
}