using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class BuildInteraction : GameplayInteraction
    {
        private IBuildable _buildable;

        public override ActionHotkey Hotkey => ActionHotkey.LeftClick;
        public override LocalisationTerm Term => LocalisationTerm.ActionsBuild;

        public BuildInteraction(IBuildable buildable, GameplayContext context) : base(context)
        {
            _buildable = buildable;
        }

        public override bool CanPrompt()
        {
            return base.CanPrompt() && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Hammer;
        }

        public override void Interact()
        {
            _buildable.Build();
        }
    }
}