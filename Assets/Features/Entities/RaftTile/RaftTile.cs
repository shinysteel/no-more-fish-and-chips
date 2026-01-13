using FishFlingers.Environments;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftTile : Entity
    {
        [SerializeField] private Transform _visualsContainer;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Material _material;

        private const string DamagedBlendName = "_DamagedBlend";

        private Vector2Int _cell;
        public Vector2Int Cell => _cell;

        public RaftTileData Data => (RaftTileData)_entityData;

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
            _cell = cell;

            transform.position = _context.Raft.CellToWorldPosition(cell);
        }

        private void Update()
        {
            PositionUpdate();
        }

        private void PositionUpdate()
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

            Vector3 targetPosition = new Vector3(0f, targetY, 0f);
            _visualsContainer.localPosition = Vector3.MoveTowards(_visualsContainer.localPosition, targetPosition, Data.SinkSettings.Speed * Time.deltaTime);
        }
    }
}