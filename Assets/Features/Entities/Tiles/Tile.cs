using FishFlingers.Environments;
using FishFlingers.Items;
using FishFlingers.Saving;
using Newtonsoft.Json;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;
using FishFlingers.States;

namespace FishFlingers.Entities
{
    public abstract class Tile : Entity
    {
        [SerializeField] private MeshRenderer _meshRenderer;

        [SerializeField] private Color _damagedColor;

        private Material _material;

        private Vector2Int _cell = Vector2Int.one * int.MinValue;
        public Vector2Int Cell => _cell;

        private int _rotations;
        public int Rotations => _rotations;
        
        public TileData Data => (TileData)_entityData;

        private Structure _structure;
        public Structure Structure => _structure;

        public const float Size = 1f;

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

            HandleHealthChanged(0, _healthModule.Current);

            _healthModule.OnChanged += HandleHealthChanged;
        }

        public override void OnReturnedToPool()
        {
            _healthModule.OnChanged -= HandleHealthChanged;

            _cell = Vector2Int.one * int.MinValue;

            base.OnReturnedToPool();
        }

        private void HandleHealthChanged(int previous, int current)
        {
            _material.color = Color.Lerp(Color.white, _damagedColor, 1f - ((float)_healthModule.Current / _healthModule.Max));

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

            transform.position = _context.Raft.Queries.CellToWorldPosition(cell);
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

        private void FixedUpdate()
        {
            PositionFixedUpdate();
        }

        private void PositionFixedUpdate()
        {
            bool sink = Physics.CheckSphere(_rigidbody.position, Data.SinkSettings.Radius, Data.SinkSettings.Mask);

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