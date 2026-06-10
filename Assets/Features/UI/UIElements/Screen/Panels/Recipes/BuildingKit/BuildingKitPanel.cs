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

            _context.LocalPlayer.TileTargetLogic.OnTargetChanged += HandleRaftPlayerTileTargetChanged;

            _context.LocalPlayer.Hotbar.OnSelectedChanged += HandleHotbarSelectedChanged;
        }

        public override void Hide(Action onComplete)
        {
            base.Hide(onComplete);

            _context.LocalPlayer.TileTargetLogic.SetIsBuilding(false);
        }

        public override void Unload()
        {
            base.Unload();

            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.TileTargetLogic.OnTargetChanged -= HandleRaftPlayerTileTargetChanged;

                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleHotbarSelectedChanged;
            }
        }

        protected override IEnumerable<IBuildable> GetCreatables()
        {
            IEnumerable<IBuildable> buildables = Enumerable.Empty<IBuildable>();

            // We populate the entries with either tiles or structures depending on the target
            if (_context.LocalPlayer.TileTargetLogic.Target.CanBuildTile())
            {
                buildables = _entityManager.GetPrefabs<Tile>().Select(tile => tile.TileDefinitionData);
            }
            else if (_context.LocalPlayer.TileTargetLogic.Target.CanBuildStructure())
            {
                buildables = _entityManager.GetPrefabs<Structure>().Select(structure => structure.StructureDefinitionData);
            }

            return buildables;
        }

        protected override void CreatePressed(IBuildable buildable)
        {
            List<InventoryChangeParams> parameters = buildable.BuildRecipe.ToChangeParams();

            if (!_context.LocalPlayer.Inventory.CanRemoveItems(parameters, out _))
            {
                return;
            }

            if (!buildable.TryBuild(_context, _context.LocalPlayer.TileTargetLogic.Target))
            {
                return;
            }

            _context.LocalPlayer.Inventory.TryRemoveItems(parameters);
        }

        private void HandleRaftPlayerTileTargetChanged(RaftPlayerTileTarget target)
        {
            if (_isShowing)
            {
                RefreshEntries();
            }
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