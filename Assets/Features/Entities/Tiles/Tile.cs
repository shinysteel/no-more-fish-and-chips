using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Saving;
using Newtonsoft.Json;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.States;
using System.Collections;
using NoMoreFishAndChips.UI;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Tile : Entity, IInteractable
    {
        [SerializeField] private MeshRenderer _meshRenderer;

        [SerializeField] private Color _damagedColor;

        private Material _material;

        private Vector2Int _cell = Vector2Int.one * int.MinValue;
        public Vector2Int Cell => _cell;

        private int _rotations;
        public int Rotations => _rotations;
        
        public TileDefinitionData TileDefinitionData => (TileDefinitionData)_entityDefinitionData;

        private Structure _structure;
        public Structure Structure => _structure;

        public const float Size = 1f;

        IInteractableSettings IInteractable.Settings => TileDefinitionData.IInteractableSettings;

        protected override void Awake()
        {
            base.Awake();

            _material = _meshRenderer.material;
        }
        
        protected override void HealthModuleSetter(int health)
        {
            _context.Raft.SetNetTileHealth(_cell, health);
        }

        public override void OnTakenFromPool()
        {
            base.OnTakenFromPool();

            HandleHealthChanged(0, _entityHealthModule.Current);

            _entityHealthModule.OnChanged += HandleHealthChanged;
        }

        public override void OnReturnedToPool()
        {
            _entityHealthModule.OnChanged -= HandleHealthChanged;

            _cell = Vector2Int.one * int.MinValue;

            base.OnReturnedToPool();
        }

        private void HandleHealthChanged(int previous, int current)
        {
            // Since this event can also trigger a despawn, we need to account for that
            if (!_isSpawned)
            {
                return;
            }

            _material.color = Color.Lerp(Color.white, _damagedColor, 1f - ((float)_entityHealthModule.Current / _entityHealthModule.Max));
        }

        public void SetCell(Vector2Int cell)
        {
            if (_cell == cell)
            {
                return;
            }

            _cell = cell;

            transform.position = _context.Raft.Queries.CellToWorldPosition(_cell);
        }

        public void SetRotations(int rotations)
        {
            _rotations = rotations;

            transform.rotation = Quaternion.AngleAxis(_rotations * 90f, Vector3.up);
        }

        public void SetStructure(Structure structure)
        {
            _structure = structure;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            PositionFixedUpdate();
        }

        private void PositionFixedUpdate()
        {
            bool sink = Physics.CheckSphere(_entityPhysicsModule.Rigidbody.position, TileDefinitionData.SinkSettings.Radius, TileDefinitionData.SinkSettings.Mask);

            float targetY;

            if (sink)
            {
                // Sit just above the water
                targetY = 0f;
            }
            else
            {
                // Bob up and down
                targetY = TileDefinitionData.BobSettings.Amplitude * Mathf.PerlinNoise(
                    _cell.x * TileDefinitionData.BobSettings.NoiseScale + _networkManager.ServerTime * TileDefinitionData.BobSettings.TimeScale,
                    _cell.y * TileDefinitionData.BobSettings.NoiseScale + _networkManager.ServerTime * TileDefinitionData.BobSettings.TimeScale);
            }

            Vector3 targetPosition = new Vector3(_entityPhysicsModule.Rigidbody.position.x, targetY, _entityPhysicsModule.Rigidbody.position.z);
            _entityPhysicsModule.Rigidbody.MovePosition(Vector3.MoveTowards(_entityPhysicsModule.Rigidbody.position, targetPosition, TileDefinitionData.SinkSettings.Speed * Time.fixedDeltaTime));
        }

        /// <summary>
        /// Retrieves the y coord that sits on top of the tile
        /// </summary>
        public float GetSurfaceY()
        {
            float height = 0.25f;
            return transform.position.y + height * 0.5f;
        }

        bool IInteractable.CanPrompt()
        {
            return _isSpawned && _currentHealth < _entityHealthModule.Max && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Hammer;
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
            //if (_context.LocalPlayer.Inventory.TryRemoveItems(TileDefinitionData.RepairRecipe.ToChangeParams()))
            //{
            //    _entityHealthModule.ChangeHealth(1);
            //}
        }
    }
}