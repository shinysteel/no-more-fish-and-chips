using FishFlingers.Networking;
using FishFlingers.States;
using PurrLobby;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class MainMenuScreen : UIElement
    {
        [SerializeField] private Button _browseGamesButton;
        [SerializeField] private Button _hostGameButton;
        [SerializeField] private Button _quitButton;

        private NetworkManager _networkManager;

        private BrowseGamesScreen _browseGamesScreen;

        public void Configure(BrowseGamesScreen browseGamesScreen)
        {
            _browseGamesScreen = browseGamesScreen;
        }

        public override void Load()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _browseGamesButton.onClick.AddListener(BrowseGamesPressed);
            _hostGameButton.onClick.AddListener(HostGamePressed);
            _quitButton.onClick.AddListener(QuitPressed);
        }

        private void BrowseGamesPressed()
        {
            _browseGamesScreen.Show(null);
        }

        private void HostGamePressed()
        {
            _ = _networkManager.CreateLobbyAsync();
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