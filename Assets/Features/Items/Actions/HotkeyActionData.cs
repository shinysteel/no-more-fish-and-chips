using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    [CreateAssetMenu(fileName = "HotkeyActionData", menuName = "Data/Items/Actions/HotkeyActionData")]
    public class HotkeyActionData : ItemActionData
    {
        public override void Execute(GameplayContext context)
        {
            context.LocalPlayer.InteractLogic.Interact(_hotkey);
        }
    }
}