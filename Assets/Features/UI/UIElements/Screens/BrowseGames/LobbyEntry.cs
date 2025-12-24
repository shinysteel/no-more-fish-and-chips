using FishFlingers.Networking;
using FishFlingers.Pools;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class LobbyEntry : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _playerCountText;

        private NetworkManager _networkManager;

        private CSteamID _lobbyId;

        private void Start()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _button.onClick.AddListener(Pressed);
        }

        public void Setup(SteamLobby lobby)
        {
            _lobbyId = lobby.LobbyId;

            _nameText.text = lobby.Name;
            _playerCountText.text = $"{lobby.Members.Length} / {lobby.MemberLimit}";
        }

        private void Pressed()
        {
            _networkManager.JoinLobbyAsync(_lobbyId);
        }
    }
}