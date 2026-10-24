using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Localisation;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class VoyageBoardViewInteraction : Interaction
    {
        private VoyageBoard _board;

        public override ActionHotkey Hotkey => ActionHotkey.FKey;
        public override LocalisationTerm Term => LocalisationTerm.ActionsView;

        public VoyageBoardViewInteraction(VoyageBoard board) : base()
        {
            _board = board;
        }
        public override void Interact()
        {
            _board.VoyageBoardPanelInstance.Toggle(null);
        }
    }
}