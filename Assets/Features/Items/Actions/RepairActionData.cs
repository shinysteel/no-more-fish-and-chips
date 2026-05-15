using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Items
{
    [CreateAssetMenu(fileName = "RepairActionData", menuName = "Data/Items/Actions/RepairActionData")]
    public class RepairActionData : ItemActionData
    {
        public override void Execute(GameplayContext context)
        {
            //if (!context.LocalPlayer.TileTargetLogic.Target.CanRepair())
            //{
            //    return;
            //}

            //context.LocalPlayer.TileTargetLogic.Target.Tile.EntityHealthModule.ChangeHealth(1);
        }
    }
}