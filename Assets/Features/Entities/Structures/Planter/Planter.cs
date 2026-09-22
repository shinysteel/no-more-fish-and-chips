using NoMoreFishAndChips.UI;
using UnityEngine;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Environments;

namespace NoMoreFishAndChips.Entities
{
    public class Planter : Structure<PlanterDefinitionData>, IInteractable
    {
        private Prop _previewProp;

        Vector3 IInteractable.Position => transform.position;
        IInteractableSettings IInteractable.IInteractableSettings => DefinitionData.IInteractableSettings;

        bool IInteractable.CanPrompt()
        {
            return _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.PalmSeed;
        }

        WorldUI IInteractable.CreatePromptUI()
        {
            RequirementPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.RequirementPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(DefinitionData.IInteractableSettings.Hotkey);
            ui.SetupRequirement(_context, DefinitionData.PlantRecipe);
            return ui;
        }

        bool IInteractable.CanInteract()
        {
            return _context.LocalPlayer.Inventory.CanRemoveItems(DefinitionData.PlantRecipe.ToChangeParams(), out _);
        }

        void IInteractable.Interact()
        {
            _context.LocalPlayer.Inventory.TryRemoveItems(DefinitionData.PlantRecipe.ToChangeParams());
        }

        void IInteractable.ShowPreview()
        {
            _previewProp = _environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Position = new Vector3(0f, 0.1875f, 0f), Parent = transform });
        }

        void IInteractable.SetPreviewColor(Color color)
        {
            _previewProp.SetColor(color);
        }

        void IInteractable.HidePreview()
        {
            _environmentManager.ReturnProp(_previewProp);
            _previewProp = null;
        }
    }
}