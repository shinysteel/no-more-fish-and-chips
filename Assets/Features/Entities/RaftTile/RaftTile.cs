using FishFlingers.Environments;
using FishFlingers.Items;
using FishFlingers.Saving;
using Newtonsoft.Json;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;

namespace FishFlingers.Entities
{
    public class TileSave
    {
        [JsonProperty] public SimpleVector2Int Cell { get; private set; }
        [JsonProperty] public int Health { get; private set; }

        public TileSave(Vector2Int cell, int health)
        {
            Cell = new SimpleVector2Int(cell);
            Health = health;
        }
    }

    public class RaftTile : Entity
    {
        [SerializeField] private Transform _visualsContainer;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Material _material;

        private const string DamagedBlendName = "_DamagedBlend";

        private Vector2Int _cell = Vector2Int.one * int.MinValue;
        public Vector2Int Cell => _cell;
        
        public RaftTileData Data => (RaftTileData)_entityData;

        private Structure _structure;
        public Structure Structure => _structure;

        protected override void Awake()
        {
            base.Awake();

            _material = _meshRenderer.material;
        }
        
        public override void SetHealth(int health)
        {
            base.SetHealth(health);
        }

        protected override void OnHealthChanged(int previous, int current)
        {
            _material.SetFloat(DamagedBlendName, 1f - ((float)_healthModule.Current / _healthModule.Max));

            if (current < previous)
            {
                // Oscillate our scale for some feedback
            }
        }

        public void SetCell(Vector2Int cell)
        {
            if (_cell == cell)
            {
                return;
            }

            _cell = cell;

            transform.position = _context.Raft.CellToWorldPosition(cell);
        }

        public void SetStructure(Structure structure)
        {
            _structure = structure;
        }

        private void FixedUpdate()
        {
            PositionFixedUpdate();
        }

        private void PositionFixedUpdate()
        {
            bool sink = Physics.CheckSphere(transform.position, Data.SinkSettings.Radius, Data.SinkSettings.Mask);

            float targetY;

            if (sink)
            {
                // Sit just above the water
                targetY = 0f;
            }
            else
            {
                // Bob up and down
                targetY = Data.BobSettings.Amplitude * Mathf.PerlinNoise(
                    _cell.x * Data.BobSettings.NoiseScale + Time.time * Data.BobSettings.TimeScale,
                    _cell.y * Data.BobSettings.NoiseScale + Time.time * Data.BobSettings.TimeScale);
            }

            Vector3 targetPosition = new Vector3(_rigidbody.position.x, targetY, _rigidbody.position.z);
            _rigidbody.MovePosition(Vector3.MoveTowards(_rigidbody.position, targetPosition, Data.SinkSettings.Speed * Time.fixedDeltaTime));
        }

        /// <summary>
        /// Retrieves the y coord that sits on top of the tile
        /// </summary>
        public float GetSurfaceY()
        {
            float height = 0.25f;
            return transform.position.y + height * 0.5f;
        }
    }
}