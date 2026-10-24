using LiteNetLib;
using Newtonsoft.Json;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Inventories;
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

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.Entities
{
    public abstract class RaftTile : Entity, IInteractable
    {
        // Manual reference for offline tiles
        [SerializeField] private Ocean _ocean;

        protected SyncVar<Vector2Int> _netCell = new SyncVar<Vector2Int>(ownerAuth: true);
        private SyncVar<int> _netRotations = new SyncVar<int>(ownerAuth: true);

        public Vector2Int Cell => _netCell.value;
        public int Rotations => _netRotations.value;

        public RaftTileDefinitionData TileDefinitionData => (RaftTileDefinitionData)_entityDefinitionData;

        public RaftTileDefeatLogic TileDefeatLogic => (RaftTileDefeatLogic)EntityDefeatLogic;

        Vector3 IInteractable.Position => transform.position;
        IInteractableSettings IInteractable.IInteractableSettings => TileDefinitionData.IInteractableSettings;
        private List<Interaction> _interactions = new();
        public virtual List<Interaction> Interactions => _interactions;

        protected override EntityLogicFactory CreateLogicFactory()
        {
            return new RaftTileLogicFactory();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleHealthChanged(0, EntityHealthLogic.CurrentHealth);
            HandleNetRotationsChanged(_netRotations.value);
            
            EntityHealthLogic.OnChanged += HandleHealthChanged;
            _netRotations.onChanged += HandleNetRotationsChanged;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            HandleNetCellChanged(_netCell.value);
            _netCell.onChanged += HandleNetCellChanged;

            _interactions.Add(new RaftTileRepairInteraction(this, _context));
        }

        protected override void OnDespawned()
        {   
            base.OnDespawned();

            foreach (Interaction interaction in _interactions)
            {
                interaction.HidePreview();
            }
            
            EntityHealthLogic.OnChanged -= HandleHealthChanged;
            _netRotations.onChanged -= HandleNetRotationsChanged;
            _netCell.onChanged -= HandleNetCellChanged;
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

        private void HandleNetRotationsChanged(int rotations)
        {
            transform.rotation = Quaternion.AngleAxis(rotations * 90f, Vector3.up);
        }

        private void HandleNetCellChanged(Vector2Int cell)
        {
            Vector3 position = _context.Raft.Queries.TileCellToWorldPosition(_netCell.value);
            position.y = _context.Ocean.SampleHeight(position) + 0.125f;
            transform.position = position;
        }

        public void SetNetRotations(int rotations)
        {
            _netRotations.value = rotations;
        }

        public void SetNetCell(Vector2Int cell)
        {
            _netCell.value = cell;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            PositionFixedUpdate();
        }

        private void PositionFixedUpdate()
        {
            void position(Ocean ocean)
            {
                Vector3 position = _rigidbody.position;
                position.y = ocean.SampleHeight(_rigidbody.position) + 0.125f;
                _rigidbody.MovePosition(Vector3.MoveTowards(_rigidbody.position, position, TileDefinitionData.BobSpeed * Time.fixedDeltaTime));
            }

            if (_ocean != null)
            {
                position(_ocean);
            }
            // TileDefeatModule takes over when defeated
            else
            {
                if (_context != null && !EntityDefeatLogic.IsDefeated)
                {
                    position(_context.Ocean);
                }
            }
        }
    }
}