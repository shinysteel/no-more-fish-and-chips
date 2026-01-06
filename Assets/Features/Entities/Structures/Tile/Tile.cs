using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class Tile : Entity
    {
        [SerializeField] private Transform _visualsContainer;
        [SerializeField] private MeshRenderer _meshRenderer;

        [SerializeField] private BobSettings _bobSettings;
        [SerializeField] private SinkSettings _sinkSettings;

        [Serializable]
        private class BobSettings
        {
            [SerializeField] private float _amplitude = 0.125f;
            [SerializeField] private float _noiseScale = 0.5f;
            [SerializeField] private float _timeScale = 0.25f;

            public float Amplitude => _amplitude;
            public float NoiseScale => _noiseScale;
            public float TimeScale => _timeScale;
        }

        [Serializable]
        private class SinkSettings
        {
            [SerializeField] private float _radius = 1.333f;
            [SerializeField] private float _speed = 0.25f;

            public float Radius => _radius;
            public float Speed => _speed;
        }

        private Material _material;

        private const string DamagedBlendName = "_DamagedBlend";

        private Vector2Int _cell;
        public Vector2Int Cell => _cell;

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

            transform.position = _raft.CellToWorldPosition(cell);
        }

        private void Update()
        {
            PositionUpdate();
        }

        private void PositionUpdate()
        {
            LayerMask mask = LayerMask.GetMask(LayerNames.Player);
            bool sink = Physics.CheckSphere(transform.position, _sinkSettings.Radius, mask);

            float targetY;

            if (sink)
            {
                // Sit just above the water
                targetY = 0f;
            }
            else
            {
                // Bob up and down
                targetY = _bobSettings.Amplitude * Mathf.PerlinNoise(
                    _cell.x * _bobSettings.NoiseScale + Time.time * _bobSettings.TimeScale,
                    _cell.y * _bobSettings.NoiseScale + Time.time * _bobSettings.TimeScale);
            }

            Vector3 targetPosition = new Vector3(0f, targetY, 0f);
            _visualsContainer.localPosition = Vector3.MoveTowards(_visualsContainer.localPosition, targetPosition, _sinkSettings.Speed * Time.deltaTime);
        }
    }
}