using FishFlingers.Networking;
using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class BrowseGamesScreen : UIElement
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _lobbyEntryContainer;
        [SerializeField] private LobbyEntry _lobbyEntryPrefab;

        private NetworkManager _networkManager;

        private List<LobbyEntry> _lobbyEntries = new();
        private float _searchInterval = 3f;
        private float _searchTimer;

        public override void Load()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _closeButton.onClick.AddListener(CloseClicked);
        }

        public override void Show(Action onComplete)
        {
            base.Show(onComplete);

            SearchAsync();
        }

        private void Update()
        {
            AutoSearchUpdate();
        }

        private void AutoSearchUpdate()
        {
            if (!_isVisible)
            {
                return;
            }

            _searchTimer += Time.deltaTime;
            if (_searchTimer < _searchInterval)
            {
                return;
            }

            _ = SearchAsync();
        }

        // Use pooling once we allow the scroll rect to display only what is on screen
        private async Task SearchAsync()
        {
            _searchTimer = 0f;
            SteamLobby[] lobbies = await _networkManager.SearchLobbies();

            if (!_isVisible)
            {
                return;
            }

            for (int i = _lobbyEntries.Count; i < lobbies.Length; i++)
            {
                _lobbyEntries.Add(Instantiate(_lobbyEntryPrefab, _lobbyEntryContainer));
            }

            for (int i = 0; i < lobbies.Length; i++)
            {
                _lobbyEntries[i].Setup(lobbies[i]);
            }

            for (int i = _lobbyEntries.Count - 1; i >= lobbies.Length; i--)
            {
                _lobbyEntries.RemoveAt(i);
            }
        }

        private void CloseClicked()
        {
            Hide(null);
        }
    }
}