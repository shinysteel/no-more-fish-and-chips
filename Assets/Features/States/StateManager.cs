using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using UnityEngine.SceneManagement;
using ShinyOwl.Common;

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

    public class StateManager : GameSystem<IStateManagerListener>
    {
        private StateManagerConfig _config;

        private StateMachine<MainState> _stateMachine;
        private MenusState _menusState;
        private GameplayState _gameplayState;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.StateManagerConfig;

            _stateMachine = new();
            _menusState = new MenusState(_stateMachine);
            _gameplayState = new GameplayState(_stateMachine);

            _stateMachine.AddState(MainState.Menus, _menusState);
            _stateMachine.AddState(MainState.Gameplay, _gameplayState);

            base.Initialise(gameManagerConfig);

            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            void HandleSceneUnloaded(Scene scene)
            {
                if (scene.name != SceneRegistry.GetSceneName(EScene.Startup))
                {
                    return;
                }

                _stateMachine.ChangeState(MainState.Menus);
                SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            }
        }

        public override void Update()
        {
            _stateMachine.Update();
        }

        public void ChangeState(MainState state)
        {
            _stateMachine.ChangeState(state);
        }
    }
}