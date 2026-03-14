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

        private string _lobbyId;

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
            _lobbyId = lobby.LobbyId;
            _nameText.text = lobby.Name;
            _playerCountText.text = $"({lobby.Members.Count} / {lobby.MemberLimit})";
        }

        private void Pressed()
        {
            _ = _lobbyManager.JoinLobbyAsync(_lobbyId);
        }

        public void OnTakenFromPool() 
        { }

        public void OnReturnedToPool()
        { }
    }
}