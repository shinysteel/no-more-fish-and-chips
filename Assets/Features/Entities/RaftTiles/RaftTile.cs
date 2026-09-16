using LiteNetLib;
using Newtonsoft.Json;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class RaftTile : Entity, IInteractable
    {
        private SyncVar<Vector2Int> _netCell = new SyncVar<Vector2Int>(ownerAuth: true);
        private SyncVar<int> _netRotations = new SyncVar<int>(ownerAuth: true);

        public Vector2Int Cell => _netCell.value;
        public int Rotations => _netRotations.value;

        public RaftTileDefinitionData TileDefinitionData => (RaftTileDefinitionData)_entityDefinitionData;


        public const float Size = 1f;

        public RaftTileDefeatLogic TileDefeatLogic => (RaftTileDefeatLogic)EntityDefeatLogic;

        IInteractableSettings IInteractable.IInteractableSettings => TileDefinitionData.IInteractableSettings;

        protected override EntityLogicFactory CreateLogicFactory()
        {
            return new RaftTileLogicFactory();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleHealthChanged(0, EntityHealthLogic.CurrentHealth);
            
            EntityHealthLogic.OnChanged += HandleHealthChanged;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            HandleNetCellChanged(_netCell.value);

            _netCell.onChanged += HandleNetCellChanged;
        }

        protected override void OnDespawned()
        {
            EntityHealthLogic.OnChanged -= HandleHealthChanged;

            _netCell.onChanged -= HandleNetCellChanged;
            
            base.OnDespawned();
        }

        private void HandleHealthChanged(int previous, int current)
        {
            // Since this event can also trigger a despawn, we need to account for that
            if (!isSpawned)
            {
                return;
            }

            _entityModel.SetColor(Color.Lerp(Color.white, TileDefinitionData.DamagedColor, 1f - ((float)current / EntityHealthLogic.MaxHealth)));
        }

        private void HandleNetCellChanged(Vector2Int cell)
        {
            Vector3 position = _context.Raft.Queries.TileCellToWorldPosition(_netCell.value);
            position.y = 0.125f;

            transform.position = position;
        }

        public void SetNetCell(Vector2Int cell)
        {
            _netCell.value = cell;
        }

        public void SetNetRotations(int rotations)
        {
            _netRotations.value = rotations;

            transform.rotation = Quaternion.AngleAxis(_netRotations.value * 90f, Vector3.up);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!isSpawned)
            {
                return;
            }
            
            PositionFixedUpdate();
        }

        private void PositionFixedUpdate()
        {
            // TileDefeatModule takes over when defeated
            if (EntityDefeatLogic.IsDefeated)
            {
                return;
            }

            bool dip = Physics.CheckSphere(_rigidbody.position, TileDefinitionData.DipSettings.Radius, TileDefinitionData.DipSettings.Mask);

            // Sit just above the water
            float targetY = 0.125f;

            if (!dip)
            {
                // Bob up and down
                targetY = 0.125f + TileDefinitionData.BobSettings.Amplitude * Mathf.PerlinNoise(
                    _netCell.value.x * TileDefinitionData.BobSettings.NoiseScale + _networkManager.ServerTime * TileDefinitionData.BobSettings.TimeScale,
                    _netCell.value.y * TileDefinitionData.BobSettings.NoiseScale + _networkManager.ServerTime * TileDefinitionData.BobSettings.TimeScale);
            }

            Vector3 targetPosition = new Vector3(_rigidbody.position.x, targetY, _rigidbody.position.z);
            _rigidbody.MovePosition(Vector3.MoveTowards(_rigidbody.position, targetPosition, TileDefinitionData.DipSettings.Speed * Time.fixedDeltaTime));
        }

        bool IInteractable.CanPrompt()
        {
            return isSpawned && _context != null && EntityHealthLogic.CurrentHealth < EntityHealthLogic.MaxHealth && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Hammer;
        }

        WorldUI IInteractable.CreatePromptUI()
        {
            RequirementPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.RequirementPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(TileDefinitionData.IInteractableSettings.Hotkey);
            ui.SetupRequirement(_context, TileDefinitionData.RepairRecipe);
            return ui;
        }

        bool IInteractable.CanInteract()
        {
            return _context.LocalPlayer.Inventory.CanRemoveItems(TileDefinitionData.RepairRecipe.ToChangeParams(), out _);
        }

        void IInteractable.Interact()
        {
            if (_context.LocalPlayer.Inventory.TryRemoveItems(TileDefinitionData.RepairRecipe.ToChangeParams()))
            {
                EntityHealthLogic.ChangeHealth(1);
            }
        }
    }
}