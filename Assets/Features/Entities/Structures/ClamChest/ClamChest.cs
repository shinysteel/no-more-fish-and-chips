using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.States;
using FishFlingers.UI;
using Newtonsoft.Json;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class ClamChest : Structure<ClamChestData>, IInteractable
    {
        [SerializeField] private Inventory _inventory;
        [SerializeField] private BoolGrid _inventoryLayout;

        private PanelInstance<ClamChestPanel> _clamChestPanelInstance;

        public Vector3 Position => transform.position;

        protected override void Awake()
        {
            base.Awake();

            _inventory.SetLayout(_inventoryLayout);
        }

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            _clamChestPanelInstance = new PanelInstance<ClamChestPanel>(_uiManager.Config.ClamChestPanel);
        }

        public override string GetJsonData()
        {
            return JsonConvert.SerializeObject(new InventorySave(_inventory));
        }

        public override void LoadJsonData(string json)
        {
            _ = JsonConvert.DeserializeObject<InventorySave>(json).LoadToAsync(_inventory);
        }

        public void Interact()
        {
            _context.LocalPlayer.SetNetOpenObjectNetworkId(id.Value);

            _clamChestPanelInstance.Toggle((ClamChestPanel panel) => panel.Setup(_context, _inventory));
        }
    }
}