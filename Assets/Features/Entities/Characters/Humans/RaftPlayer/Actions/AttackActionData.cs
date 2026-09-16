using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "AttackActionData", menuName = "Data/Entities/Characters/RaftPlayer/Actions/AttackActionData")]
    public class AttackActionData : ActionData
    {
        public override void Execute(GameplayContext context)
        {
            context.LocalPlayer.AttackLogic.Attack();   
        }
    }
}