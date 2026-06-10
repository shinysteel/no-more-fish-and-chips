using NoMoreFishAndChips.UI;
using UnityEngine;
using NoMoreFishAndChips.Items;

namespace NoMoreFishAndChips.Entities
{
    public class Planter : Structure<PlanterDefinitionData>, IInteractable
    {
        IInteractableSettings IInteractable.Settings => DefinitionData.IInteractableSettings;

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
    }
}