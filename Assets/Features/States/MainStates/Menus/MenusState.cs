using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using NoMoreFishAndChips.UI;
using NoMoreFishAndChips.Networking;
using PurrLobby;
using ShinyOwl.Common;
using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.UI.Transitions;
using NoMoreFishAndChips.Scenes;
using System.Threading.Tasks;
using System;

namespace NoMoreFishAndChips.States
{
    public enum EMenusState { }

    public class MenusState : MainState<EMainState, ENone>
    {
        private UIManager _uiManager;
        private CameraManager _cameraManager;
        private TransitionManager _transitionManager;
        private SceneManager _sceneManager;

        private MenusStateConfig _config;

        private MainMenuScreen _mainMenuScreen;

        public MenusState(StateMachine<EMainState> parent) : base(parent)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
        }

        public override void Initialise(StateManagerConfig config)
        {
            _config = config.MenusStateConfig;
        }

        public override void Enter()
        {
            _cameraManager.SetMode(new OrbitCameraMode(Vector3.zero, 5f, 3f, 0.1f));
        }

        public override async Task EnterAsync()
        {
            try
            {
                await _sceneManager.LoadSceneAsync(EScene.EnvironmentMainMenu, LoadSceneMode.Additive, LoadSceneContext.Local);

                _mainMenuScreen = await _uiManager.CreateScreenUIAsync(_uiManager.Config.MainMenuScreenPrefab, UILayer.Screens);
                _mainMenuScreen.Show(null);

                _transitionManager.UncoverScreen(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public override void Exit()
        {
            _uiManager.DestroyScreenUI(_mainMenuScreen, UILayer.Screens);
            _mainMenuScreen = null;

            // Purrnet is unloading all the scenes as soon as we connect since Game
            // scene was loaded as single. This is just a dirty fix. The real solution is
            // covering the screen, unloading the environment, and only then connecting
            if (_sceneManager.IsSceneLoaded(EScene.EnvironmentMainMenu))
            {
                _sceneManager.UnloadSceneAsync(EScene.EnvironmentMainMenu, LoadSceneContext.Local);
            }
        }
    }
}