using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Interactions
{
    public class OpenClamChestInteraction : GameplayInteraction
    {
        private ClamChest _chest;

        public override ActionHotkey Hotkey => ActionHotkey.FKey;
        public override LocalisationTerm Term => LocalisationTerm.ActionsOpen;

        public OpenClamChestInteraction(ClamChest chest, GameplayContext context) : base(context)
        {
            _chest = chest;
        }

        public override void Interact()
        {
            _chest.ClamChestPanelInstance.Toggle((ClamChestPanel panel) => panel.Setup(_context, _chest, _chest.Inventory));
        }
    }
}