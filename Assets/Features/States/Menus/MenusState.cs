using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.UI;
using NoMoreFishAndChips.UI.Transitions;
using PurrLobby;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Object = UnityEngine.Object;

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

        private OrbitCameraMode _orbitCameraMode;

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
            base.Enter();

            _ = EnterAsync();
        }

        private async Task EnterAsync()
        {
            try
            {
                await _sceneManager.LoadSceneAsync(EScene.EnvironmentMenus, LoadSceneMode.Additive, LoadSceneContext.Local);

                EnvironmentMenusReferences references = Object.FindFirstObjectByType<EnvironmentMenusReferences>();

                _orbitCameraMode = new OrbitCameraMode(_cameraManager.Config.EnvironmentMenusOrbitCameraModeSettings, references.RaftTransform);
                _cameraManager.AddMode(_orbitCameraMode);

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
            base.Exit();

            _uiManager.DestroyScreenUI(_mainMenuScreen, UILayer.Screens);
            _mainMenuScreen = null;

            // Purrnet is unloading all the scenes as soon as we connect since Game
            // scene was loaded as single. This is just a dirty fix. The real solution is
            // covering the screen, unloading the environment, and only then connecting
            if (_sceneManager.IsSceneLoaded(EScene.EnvironmentMenus))
            {
                _sceneManager.UnloadSceneAsync(EScene.EnvironmentMenus, LoadSceneContext.Local);
            }

            _cameraManager.RemoveMode(_orbitCameraMode);
            _orbitCameraMode = null;
        }
    }
}