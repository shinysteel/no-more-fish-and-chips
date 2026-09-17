using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RotateActionData", menuName = "Data/Entities/Characters/RaftPlayer/Actions/RotateActionData")]
    public class RotateActionData : ActionData
    {
        public override void Execute(GameplayContext context)
        {
            context.LocalPlayer.BuildLogic.Rotate();
        }
    }
}