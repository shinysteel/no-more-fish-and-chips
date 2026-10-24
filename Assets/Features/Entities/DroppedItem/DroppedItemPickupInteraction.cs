using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class DroppedItemPickupInteraction : GameplayInteraction
    {
        private AudioManager _audioManager;

        private DroppedItem _item;

        public override ActionHotkey Hotkey => ActionHotkey.FKey;
        public override LocalisationTerm Term => LocalisationTerm.ActionsPickup;

        public DroppedItemPickupInteraction(DroppedItem item, GameplayContext context) : base(context)
        {
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _item = item;
        }

        public override bool CanInteract()
        {
            return _context.LocalPlayer.Inventory.CanAddItem(InventoryChangeParams.Create(_item.NetItemInstance), out _, out _, out _);
        }

        public override void Interact()
        {
            if (_context.LocalPlayer.Inventory.TryAddItem(InventoryChangeParams.Create(_item.NetItemInstance), false, out _, out _, out _))
            {
                _audioManager.PlaySound(SoundId.DroppedItemPickup, _item.transform.position, 0f);

                _item.DespawnRpc();
            }
        }
    }
}