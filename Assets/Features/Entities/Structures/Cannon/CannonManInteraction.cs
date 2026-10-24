using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CannonManInteraction : CannonInteraction
    {
        public override ActionHotkey Hotkey => ActionHotkey.FKey;
        public override LocalisationTerm Term => LocalisationTerm.ActionsMan;

        public CannonManInteraction(Cannon cannon, GameplayContext context) : base(cannon, context)
        { }

        public override void Interact()
        { }
    }
}