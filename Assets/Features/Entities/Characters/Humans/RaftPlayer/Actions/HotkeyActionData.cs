using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "HotkeyActionData", menuName = "Data/Entities/Characters/RaftPlayer/Actions/HotkeyActionData")]
    public class HotkeyActionData : ActionData
    {
        public override void Execute(GameplayContext context)
        {
            context.LocalPlayer.InteractLogic.Interact(_hotkey);
        }
    }
}