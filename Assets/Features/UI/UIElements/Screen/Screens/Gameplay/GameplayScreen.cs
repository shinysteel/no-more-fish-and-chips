using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.States;
using FishFlingers.UI.Transitions;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class GameplayScreen : ScreenUI
    {
        [SerializeField] private HotbarView _hotbarView;
        [SerializeField] private ItemActionsView _itemActionsView;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _fishingBagButton;
        
        private NetworkManager _networkManager;
        private UIManager _uiManager;

        private GameplayContext _context;

        private PanelInstance<FishingBagPanel> _fishingBagPanelInstance;

        public override void Load(Canvas canvas)
        {
            base.Load(canvas);

            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();

            _fishingBagPanelInstance = new PanelInstance<FishingBagPanel>(_uiManager.Config.FishingBagPanelPrefab);

            _settingsButton.onClick.AddListener(SettingsPressed);
            _fishingBagButton.onClick.AddListener(FishingBagPressed);
        }

        public void Setup(GameplayContext context)
        {
            _context = context;

            _hotbarView.Setup(context);
            _itemActionsView.Setup(context);
        }

        private void Update()
        {
            if (_context.LocalPlayer.InputLogic.ToggleSettings)
            {
                Utils.UI.SimulatePressed(_settingsButton);
            }

            if (_context.LocalPlayer.InputLogic.ToggleFishingBag)
            {
                Utils.UI.SimulatePressed(_fishingBagButton);
            }
        }

        private void SettingsPressed()
        {
            if (_uiManager.IsLayerInUse(UILayer.Panels))
            {
                return;
            }

            if (_networkManager.IsServer)
            {
                _networkManager.StopServer();
            }

            _networkManager.StopClient();
        }

        private void FishingBagPressed()
        {
            _fishingBagPanelInstance.Toggle((FishingBagPanel panel) => panel.Setup(_context));
        }
    }
}