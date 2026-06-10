using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    [CreateAssetMenu(fileName = "BuildActionData", menuName = "Data/Items/Actions/BuildActionData")]
    public class BuildActionData : ItemActionData
    {
        public override void Execute(GameplayContext context)
        {
            context.LocalPlayer.TileTargetLogic.SetIsBuilding(true);

            UIManager uiManager = GameManager.Instance.Get<UIManager>();

            uiManager.CreateScreenUIAsync(uiManager.Config.BuildingKitPanelPrefab, UILayer.Panels).completed += (BuildingKitPanel panel) =>
            {
                panel.Setup(context);
                panel.Show(null);
            };
        }
    }
}