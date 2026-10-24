using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CannonLoadInteraction : CannonInteraction
    {
        private PoolManager _poolManager;

        public override ActionHotkey Hotkey => ActionHotkey.LeftClick;
        public override LocalisationTerm Term => LocalisationTerm.ActionsLoad;

        public CannonLoadInteraction(Cannon cannon, GameplayContext context) : base(cannon, context)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public override bool CanPrompt()
        {
            return base.CanPrompt() && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Cannonball;
        }

        public override InteractPrompt CreatePrompt(SpawnParams parameters)
        {
            RequirementPrompt prompt = _poolManager.GetTypedPoolable<RequirementPrompt>(parameters);
            prompt.SetupInteract(Hotkey, Term);
            prompt.SetupRequirement(_context, _cannon.DefinitionData.LoadRecipe);
            return prompt;
        }

        public override void Interact()
        {
            _context.LocalPlayer.Inventory.TryRemoveItems(_cannon.DefinitionData.LoadRecipe.ToChangeParams());
        }
    }
}