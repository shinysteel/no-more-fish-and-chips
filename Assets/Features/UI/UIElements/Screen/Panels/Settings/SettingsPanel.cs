using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Saving;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.UI
{
    public class SettingsPanel : Panel
    {
        [SerializeField] private Button _saveAndQuitButton;

        private NetworkManager _networkManager;
        private SaveManager _saveManager;

        private void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();

            _saveAndQuitButton.onClick.AddListener(SaveAndQuitPressed);
        }

        private void SaveAndQuitPressed()
        {
            _ = SaveAndQuitPressedAsync();
        }

        private async Task SaveAndQuitPressedAsync()
        {
            if (_networkManager.IsServer)
            {
                await _saveManager.SaveGameAsync();
                _networkManager.StopServer();
            }

            _networkManager.StopClient();
        }
    }
}