using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Interactions
{
    public class FeedGiantClamInteraction : GameplayInteraction
    {
        private GiantClam _clam;

        public override ActionHotkey Hotkey => ActionHotkey.FKey;
        public override LocalisationTerm Term => LocalisationTerm.ActionsFeed;

        public FeedGiantClamInteraction(GiantClam clam, GameplayContext context) : base(context)
        {
            _clam = clam;
        }

        public override bool CanPrompt()
        {
            return base.CanPrompt() && _clam.CanOpenInventory;
        }

        public override bool CanInteract()
        {
            return true;
        }

        public override void Interact()
        {
            _clam.GiantClamsMouthPanelInstance.Toggle((GiantClamsMouthPanel panel) => panel.Setup(_context, _clam, _clam.Inventory));
        }
    }
}