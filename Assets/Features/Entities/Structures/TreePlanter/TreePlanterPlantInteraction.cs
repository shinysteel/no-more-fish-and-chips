using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class TreePlanterPlantInteraction : GameplayInteraction
    {
        private PoolManager _poolManager;
        private EnvironmentManager _environmentManager;

        private TreePlanter _planter;

        private Prop _previewProp;

        public override ActionHotkey Hotkey => ActionHotkey.LeftClick;
        public override LocalisationTerm Term => LocalisationTerm.ActionsPlant;

        public TreePlanterPlantInteraction(TreePlanter planter, GameplayContext context) : base(context)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _planter = planter;
        }

        public override bool CanPrompt()
        {
            return base.CanPrompt() && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.PalmSeed;
        }

        public override InteractPrompt CreatePrompt(SpawnParams parameters)
        {
            RequirementPrompt prompt = _poolManager.GetTypedPoolable<RequirementPrompt>(parameters);
            prompt.SetupInteract(Hotkey, Term);
            prompt.SetupRequirement(_context, _planter.DefinitionData.PlantRecipe);
            return prompt;
        }

        public override void Interact()
        {
            _context.LocalPlayer.Inventory.TryRemoveItems(_planter.DefinitionData.PlantRecipe.ToChangeParams());

            _planter.PlantRpc();
        }

        public override void ShowPreview()
        {
            _previewProp = _environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Position = new Vector3(0f, 0.275f, 0f), Scale = new Vector3(0.1f, 0.2f, 0.1f), Parent = _planter.transform });
        }

        public override void SetPreviewColor(Color color)
        {
            _previewProp.SetColor(color);
        }

        public override void HidePreview()
        {
            _environmentManager.ReturnProp(_previewProp);
            _previewProp = null;
        }
    }
}