using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.Saving;
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
        [SerializeField] private Button _craftingKitButton;
        [SerializeField] private WaveMeter _waveMeter;
        
        private NetworkManager _networkManager;
        private UIManager _uiManager;
        private SaveManager _saveManager;

        private GameplayContext _context;

        private PanelInstance<SettingsPanel> _settingsPanelInstance;
        private PanelInstance<FishingBagPanel> _fishingBagPanelInstance;
        private PanelInstance<CraftingKitPanel> _craftingKitPanelInstance;

        public override void Load(Canvas canvas)
        {
            base.Load(canvas);

            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();

            _settingsPanelInstance = new PanelInstance<SettingsPanel>(_uiManager.Config.SettingsPanelPrefab);
            _fishingBagPanelInstance = new PanelInstance<FishingBagPanel>(_uiManager.Config.FishingBagPanelPrefab);
            _craftingKitPanelInstance = new PanelInstance<CraftingKitPanel>(_uiManager.Config.CraftingKitPanelPrefab);

            _settingsButton.onClick.AddListener(SettingsPressed);
            _fishingBagButton.onClick.AddListener(FishingBagPressed);
            _craftingKitButton.onClick.AddListener(CraftingKitPressed);
        }

        public void Setup(GameplayContext context)
        {
            _context = context;

            _hotbarView.Setup(context);
            _itemActionsView.Setup(context);
            _waveMeter.Setup(context);
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

            if (_context.LocalPlayer.InputLogic.ToggleCraftingKit)
            {
                Utils.UI.SimulatePressed(_craftingKitButton);
            }
        }

        private void SettingsPressed()
        {
            _settingsPanelInstance.Toggle(null);

            //if (_uiManager.IsLayerInUse(UILayer.Panels))
            //{
            //    return;
            //}

            //_ = SettingsPressedAsync();
        }

        //private async Task SettingsPressedAsync()
        //{
        //    if (_networkManager.IsServer)
        //    {
        //        await _saveManager.SaveGameAsync();
        //        _networkManager.StopServer();
        //    }

        //    _networkManager.StopClient();
        //}

        private void FishingBagPressed()
        {
            _fishingBagPanelInstance.Toggle((FishingBagPanel panel) => panel.Setup(_context));
        }

        private void CraftingKitPressed()
        {
            _craftingKitPanelInstance.Toggle((CraftingKitPanel panel) => panel.Setup(_context));
        }
    }
}