using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Interactions
{
    public class PlantInteraction : GameplayInteraction
    {
        private PoolManager _poolManager;
        private EnvironmentManager _environmentManager;

        private Planter _planter;

        private Prop _previewProp;

        public override ActionHotkey Hotkey => ActionHotkey.LeftClick;
        public override LocalisationTerm Term => LocalisationTerm.ActionsPlant;

        public PlantInteraction(Planter planter, GameplayContext context) : base(context)
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

        public override bool CanInteract()
        {
            return _context.LocalPlayer.Inventory.CanRemoveItems(_planter.DefinitionData.PlantRecipe.ToChangeParams(), out _);
        }

        public override void Interact()
        {
            _context.LocalPlayer.Inventory.TryRemoveItems(_planter.DefinitionData.PlantRecipe.ToChangeParams());
        }

        public override void ShowPreview()
        {
            _previewProp = _environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Position = new Vector3(0f, 0.1875f, 0f), Parent = _planter.transform });
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