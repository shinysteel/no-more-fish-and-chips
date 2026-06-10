using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class MainMenuScreen : ScreenUI
    {
        [SerializeField] private Button _browseGamesButton;
        [SerializeField] private Button _hostGameButton;
        [SerializeField] private Button _quitButton;

        private UIManager _uiManager;

        private PanelInstance<BrowseGamesPanel> _browseGamesPanelInstance;
        private PanelInstance<HostGamePanel> _hostGamesPanelInstance;

        public override void Load(Canvas canvas)
        {
            base.Load(canvas);

            _uiManager = GameManager.Instance.Get<UIManager>();

            _browseGamesPanelInstance = new PanelInstance<BrowseGamesPanel>(_uiManager.Config.BrowseGamesPanelPrefab);
            _hostGamesPanelInstance = new PanelInstance<HostGamePanel>(_uiManager.Config.HostGamePanelPrefab);

            _browseGamesButton.onClick.AddListener(BrowseGamesPressed);
            _hostGameButton.onClick.AddListener(HostGamePressed);
            _quitButton.onClick.AddListener(QuitPressed);
        }

        private void BrowseGamesPressed()
        {
            _browseGamesPanelInstance.Toggle(null);
        }

        private void HostGamePressed()
        {
            _hostGamesPanelInstance.Toggle(null);
        }

        private void QuitPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}