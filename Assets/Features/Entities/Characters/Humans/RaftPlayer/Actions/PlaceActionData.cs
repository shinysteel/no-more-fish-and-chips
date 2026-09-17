using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "PlaceActionData", menuName = "Data/Entities/Characters/RaftPlayer/Actions/PlaceActionData")]
    public class PlaceActionData : ActionData
    {
        public override void Execute(GameplayContext context)
        {
            context.LocalPlayer.BuildLogic.Place();
        }
    }
}