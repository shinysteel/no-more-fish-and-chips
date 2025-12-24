using FishFlingers.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class GameplayScreen : UIElement
    {
        [SerializeField] private Button _leaveButton;

        private NetworkManager _networkManager;

        public override void Load()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _leaveButton.onClick.AddListener(LeavePressed);
        }

        private void LeavePressed()
        {
            if (_networkManager.IsServer)
            {
                _networkManager.StopServer();
            }

            _networkManager.StopClient();
        }
    }
}