using FishFlingers.Networking;
using FishFlingers.UI.Transitions;
using ShinyOwl.Common.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class GameplayScreen : UIElement
    {
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _fishingBagButton;

        private NetworkManager _networkManager;
        private UIManager _uiManager;

        public override void Load()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();

            _settingsButton.onClick.AddListener(SettingsPressed);
            _fishingBagButton.onClick.AddListener(FishingBagPressed);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Utils.UI.SimulatePressed(_settingsButton);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Utils.UI.SimulatePressed(_fishingBagButton);
            }
        }

        private void SettingsPressed()
        {
            if (_networkManager.IsServer)
            {
                _networkManager.StopServer();
            }

            _networkManager.StopClient();
        }

        private void FishingBagPressed()
        {
            _uiManager.CreateUIElementAsync(_uiManager.Config.FishingBagPanel, UILayer.Panels).completed += (UIElement element) =>
            {
                element.Show(null);
            };
        }
    }
}