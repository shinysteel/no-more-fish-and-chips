using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Localisation;
using UnityEngine;

namespace NoMoreFishAndChips.Interactions
{
    public class ViewVoyageBoardInteraction : Interaction
    {
        private VoyageBoard _board;

        public override ActionHotkey Hotkey => ActionHotkey.FKey;
        public override LocalisationTerm Term => LocalisationTerm.ActionsView;

        public ViewVoyageBoardInteraction(VoyageBoard board) : base()
        {
            _board = board;
        }
        public override void Interact()
        {
            _board.VoyageBoardPanelInstance.Toggle(null);
        }
    }
}