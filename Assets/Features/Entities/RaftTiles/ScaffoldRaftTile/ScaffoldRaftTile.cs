using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.UI;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class ScaffoldRaftTile : RaftTile
    {
        private SyncVar<EntityId> _netBuildId = new SyncVar<EntityId>(ownerAuth: true);
        private SyncVar<int> _netBuildRotations = new SyncVar<int>(ownerAuth: true);

        public void SetNetBuildId(EntityId id)
        {
            _netBuildId.value = id;
        }

        public void SetNetBuildRotations(int rotations)
        {
            _netBuildRotations.value = rotations;
        }

        protected override bool CanPrompt()
        {
            return isSpawned && _context != null && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Hammer;
        }

        protected override WorldUI CreatePromptUI()
        {
            InteractPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(TileDefinitionData.IInteractableSettings.Hotkey);
            return ui;
        }

        protected override bool CanInteract()
        {
            return true;
        }

        protected override void Interact()
        {
            RaftTile prefab = (RaftTile)_entityManager.GetPrefab(_netBuildId.value);

            _context.Raft.SetTile(_netCell.value, _netBuildId.value, prefab.EntityDefinitionData.Health, _netBuildRotations.value);
        }
    }
}