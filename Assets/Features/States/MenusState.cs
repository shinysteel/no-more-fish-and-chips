using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using FishFlingers.UI;
using FishFlingers.Networking;
using PurrLobby;
using ShinyOwl.Common;
using FishFlingers.Cameras;
using FishFlingers.UI.Transitions;
using FishFlingers.Scenes;
using System.Threading.Tasks;

namespace FishFlingers.States
{
    public enum EMenusState { }

    public class MenusState : State<MainState, EMenusState>
    {
        private UIManager _uiManager;
        private CameraManager _cameraManager;
        private TransitionManager _transitionManager;
        private SceneManager _sceneManager;

        private MainMenuScreen _mainMenuScreen;
        private BrowseGamesScreen _browseGamesScreen;

        public MenusState(StateMachine<MainState> parent) : base(parent)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
        }

        public override void Enter()
        {
            _cameraManager.SetMode(new OrbitCameraMode(Vector3.zero, 5f, 3f, 0.1f));
        }

        public override async Task EnterAsync()
        {
            await _sceneManager.LoadSceneAsync(EScene.EnvironmentMainMenu, LoadSceneMode.Additive);

            _browseGamesScreen = (BrowseGamesScreen)await _uiManager.CreateUIElementAsync(_uiManager.Config.BrowseGamesScreen, UILayer.Screens);
            _mainMenuScreen = (MainMenuScreen)await _uiManager.CreateUIElementAsync(_uiManager.Config.MainMenuScreen, UILayer.Screens);

            _mainMenuScreen.Configure(_browseGamesScreen);
            _mainMenuScreen.Show(null);

            _transitionManager.UncoverScreen(null);
        }

        public override void Exit()
        {
            _uiManager.DestroyUIElement(_mainMenuScreen, UILayer.Screens);
            _mainMenuScreen = null;

            _uiManager.DestroyUIElement(_browseGamesScreen, UILayer.Screens);
            _browseGamesScreen = null;

            _sceneManager.UnloadSceneAsync(EScene.EnvironmentMainMenu, LoadSceneContext.Local);
        }
    }
}