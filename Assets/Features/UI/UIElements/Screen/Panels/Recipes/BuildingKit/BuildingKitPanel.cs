using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.UI
{
    public class BuildingKitPanel : RecipesPanel<IBuildable>
    {
        public override void Setup(GameplayContext context)
        {
            base.Setup(context);

            _context.LocalPlayer.Hotbar.OnSelectedChanged += HandleHotbarSelectedChanged;
        }

        private void OnDestroy()
        {
            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleHotbarSelectedChanged;
            }
        }

        protected override IEnumerable<IBuildable> GetCreatables()
        {
            return _entityManager.GetPrefabs<RaftTile>().Where(tile => tile.TileDefinitionData.BuildRecipe.Requirements.Length > 0).Select(tile => (IBuildable)tile.TileDefinitionData)
                .Concat(_entityManager.GetPrefabs<Structure>().Where(structure => structure.StructureDefinitionData.BuildRecipe.Requirements.Length > 0).Select(structure => structure.StructureDefinitionData));
        }

        protected override void CreatePressed(IBuildable buildable)
        {
            _context.LocalPlayer.BuildLogic.SetBuildTarget(buildable.EntityDefinitionData.Id);

            ClosePressed();
        }

        private void HandleHotbarSelectedChanged(HotbarSlot slot)
        {
            // There's a scenario where you aren't holding a hammer anymore while this is open
            if (slot.InventoryItem?.ItemInstance.Data.ItemId != ItemId.Hammer)
            {
                ClosePressed();
            }
        }
    }
}