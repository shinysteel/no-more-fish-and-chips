using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.UI;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class ClamChest : Structure<ClamChestData>, IInteractable
    {
        private Inventory _inventory;

        private PanelInstance<ClamChestPanel> _clamChestPanelInstance;

        public Vector3 Position => transform.position;
        
        private void Start()
        {
            _clamChestPanelInstance = new PanelInstance<ClamChestPanel>(_uiManager.Config.ClamChestPanel);
        }

        public void Interact()
        {
            _clamChestPanelInstance.Toggle(null);
        }
    }
}