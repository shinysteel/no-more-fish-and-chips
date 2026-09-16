using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "BuildingKitActionData", menuName = "Data/Entities/Characters/RaftPlayer/Actions/BuildingKitActionData")]
    public class BuildingKitActionData : ActionData
    {
        public override void Execute(GameplayContext context)
        {
            UIManager uiManager = GameManager.Instance.Get<UIManager>();

            uiManager.CreateScreenUIAsync(uiManager.Config.BuildingKitPanelPrefab, UILayer.Panels).completed += (BuildingKitPanel panel) =>
            {
                panel.Setup(context);
                panel.Show(null);
            };
        }
    }
}