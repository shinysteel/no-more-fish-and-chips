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
        [SerializeField] private MeshRenderer _meshRenderer;

        [SerializeField] private Color _damagedColor;

        private SyncVar<Vector2Int> _netCell = new SyncVar<Vector2Int>(ownerAuth: true);
        private SyncVar<int> _netRotations = new SyncVar<int>(ownerAuth: true);
        private SyncVar<Structure> _netStructure = new SyncVar<Structure>(ownerAuth: true);

        public Vector2Int Cell => _netCell.value;
        public int Rotations => _netRotations.value;
        public Structure Structure => _netStructure.value;


        private Material _material;

        public RaftTileDefinitionData TileDefinitionData => (RaftTileDefinitionData)_entityDefinitionData;


        public const float Size = 1f;

        public RaftTileDefeatLogic TileDefeatModule => (RaftTileDefeatLogic)EntityDefeatLogic;

        IInteractableSettings IInteractable.IInteractableSettings => TileDefinitionData.IInteractableSettings;

        protected override void Awake()
        {
            base.Awake();

            _material = _meshRenderer.material;
        }

        protected override EntityLogicFactory CreateLogicFactory()
        {
            return new RaftTileLogicFactory();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            EntityHealthLogic.OnChanged += HandleHealthChanged;
        }

        protected override void OnDespawned()
        {
            EntityHealthLogic.OnChanged -= HandleHealthChanged;

            base.OnDespawned();
        }

        private void HandleHealthChanged(int previous, int current)
        {
            // Since this event can also trigger a despawn, we need to account for that
            if (!isSpawned)
            {
                return;
            }

            _material.color = Color.Lerp(Color.white, _damagedColor, 1f - ((float)current / EntityHealthLogic.Max));
        }

        [ServerRpc(requireOwnership: false)]
        public void AddStructureRpc(EntityId structureId)
        {
            if (_netStructure.value != null)
            {
                return;
            }

            _netStructure.value = (Structure)_entityManager.Spawn(structureId, new SpawnParams() { Parent = transform, Position = new Vector3(transform.position.x, GetSurfaceY(), transform.position.z) });
            _netStructure.value.SetCell(_netCell.value);
        }

        public void SetNetCell(Vector2Int cell)
        {
            if (_netCell.value == cell)
            {
                return;
            }

            _netCell.value = cell;

            transform.position = _context.Raft.Queries.CellToWorldPosition(_netCell.value);
        }

        public void SetNetRotations(int rotations)
        {
            _netRotations.value = rotations;

            transform.rotation = Quaternion.AngleAxis(_netRotations.value * 90f, Vector3.up);
        }

        public void SetNetStructure(Structure structure)
        {
            _netStructure.value = structure;
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

            float targetY;

            if (dip)
            {
                // Sit just above the water
                targetY = 0f;
            }
            else
            {
                // Bob up and down
                targetY = TileDefinitionData.BobSettings.Amplitude * Mathf.PerlinNoise(
                    _netCell.value.x * TileDefinitionData.BobSettings.NoiseScale + _networkManager.ServerTime * TileDefinitionData.BobSettings.TimeScale,
                    _netCell.value.y * TileDefinitionData.BobSettings.NoiseScale + _networkManager.ServerTime * TileDefinitionData.BobSettings.TimeScale);
            }

            Vector3 targetPosition = new Vector3(_rigidbody.position.x, targetY, _rigidbody.position.z);
            _rigidbody.MovePosition(Vector3.MoveTowards(_rigidbody.position, targetPosition, TileDefinitionData.DipSettings.Speed * Time.fixedDeltaTime));
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
            return isSpawned && EntityHealthLogic.Current < EntityHealthLogic.Max && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Hammer;
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