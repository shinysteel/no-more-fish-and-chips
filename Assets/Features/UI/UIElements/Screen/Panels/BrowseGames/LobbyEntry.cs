using FishFlingers.Networking;
using FishFlingers.Pools;
using ShinyOwl.Common;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class LobbyEntry : MonoBehaviour, IPoolable
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _playerCountText;

        private LobbyManager _lobbyManager;

        private Lobby _lobby;

        private void Awake()
        {
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
        }

        private void Start()
        {
            _button.onClick.AddListener(Pressed);
        }

        public void Setup(Lobby lobby)
        {
            _lobby = lobby;
            _nameText.text = _lobby.Name;
            _playerCountText.text = $"({_lobby.Members.Count} / {_lobby.MemberLimit})";
        }

        private void Pressed()
        {
            _lobbyManager.SetLobbyService(_lobby.Service);

            _ = _lobbyManager.JoinLobbyAsync(_lobby.LobbyId);
        }

        public void OnTakenFromPool() 
        { }

        public void OnReturnedToPool()
        { }
    }
}