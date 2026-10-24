using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftTileRepairInteraction : GameplayInteraction
    {
        private PoolManager _poolManager;
        private EnvironmentManager _environmentManager;

        private RaftTile _tile;

        private Prop _previewProp;

        public override ActionHotkey Hotkey => ActionHotkey.LeftClick;
        public override LocalisationTerm Term => LocalisationTerm.ActionsRepair;

        public RaftTileRepairInteraction(RaftTile tile, GameplayContext context) : base(context)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _tile = tile;
        }

        public override bool CanPrompt()
        {
            return base.CanPrompt() && _tile.EntityHealthLogic.CurrentHealth < _tile.EntityHealthLogic.MaxHealth && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Hammer;
        }

        public override InteractPrompt CreatePrompt(SpawnParams parameters)
        {
            RequirementPrompt prompt = _poolManager.GetTypedPoolable<RequirementPrompt>(parameters);
            prompt.SetupInteract(Hotkey, Term);
            prompt.SetupRequirement(_context, _tile.TileDefinitionData.RepairRecipe);
            return prompt;
        }

        public override bool CanInteract()
        {
            return _context.LocalPlayer.Inventory.CanRemoveItems(_tile.TileDefinitionData.RepairRecipe.ToChangeParams(), out _);
        }

        public override void Interact()
        {
            List<InventoryChangeParams> parameters = _tile.TileDefinitionData.RepairRecipe.ToChangeParams();

            if (!_context.LocalPlayer.Inventory.CanRemoveItems(parameters, out _))
            {
                return;
            }

            _context.LocalPlayer.RaftPlayerActLogic.StartChannel(_tile.TileDefinitionData.RepairRecipe.Duration, ChannelAnimation.Build, () =>
            {
                _context.LocalPlayer.Inventory.TryRemoveItems(parameters);
                _tile.EntityHealthLogic.ChangeHealth(1);
            });
        }

        public override void ShowPreview()
        {
            _previewProp = _environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Position = new Vector3(0f, -0.125f, 0f), Scale = new Vector3(1.01f, 0.26f, 1.01f), Parent = _tile.transform });
        }

        public override void SetPreviewColor(Color color)
        {
            _previewProp.SetColor(color);
        }

        public override void HidePreview()
        {
            // We need to check for null, some Interacts of RaftTile destroy it
            if (_previewProp == null)
            {
                return;
            }

            _environmentManager.ReturnProp(_previewProp);
            _previewProp = null;
        }
    }
}