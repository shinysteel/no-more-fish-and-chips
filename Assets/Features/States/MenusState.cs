using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using FishFlingers.UI;
using FishFlingers.Networking;
using PurrLobby;
using ShinyOwl.Common;
using UnityEngine.SceneManagement;
using FishFlingers.Cameras;
using FishFlingers.UI.Transitions;

namespace FishFlingers.States
{
    public enum EMenusState { }

    public class MenusState : State<MainState, EMenusState>
    {
        private UIManager _uiManager;
        private CameraManager _cameraManager;
        private TransitionManager _transitionManager;

        private MainMenuScreen _mainMenuScreen;
        private BrowseGamesScreen _browseGamesScreen;

        public MenusState(StateMachine<MainState> parent) : base(parent)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
        }

        public override void Enter()
        {
            _browseGamesScreen = _uiManager.CreateUIElementInLayer(_uiManager.Config.BrowseGamesScreen, UILayer.Screens);

            _mainMenuScreen = _uiManager.CreateUIElementInLayer(_uiManager.Config.MainMenuScreen, UILayer.Screens, UILayerInsertMode.FirstSibling);
            _mainMenuScreen.Configure(_browseGamesScreen);
            _mainMenuScreen.Show(null);

            AsyncOperation op = SceneManager.LoadSceneAsync(SceneRegistry.GetSceneName(EScene.EnvironmentMainMenu), LoadSceneMode.Additive);
            op.completed += _ =>
            {
                SceneManager.SetActiveScene(SceneRegistry.GetScene(EScene.EnvironmentMainMenu));
                _transitionManager.UncoverScreen(null);
            };

            _cameraManager.SetMode(new OrbitCameraMode(Vector3.zero, 5f, 3f, 0.1f));
        }

        public override void Exit()
        {
            _uiManager.DestroyUIElementInLayer(_mainMenuScreen, UILayer.Screens);
            _mainMenuScreen = null;

            _uiManager.DestroyUIElementInLayer(_browseGamesScreen, UILayer.Screens);
            _browseGamesScreen = null;

            SceneManager.UnloadSceneAsync(SceneRegistry.GetSceneName(EScene.EnvironmentMainMenu));
        }
    }
}