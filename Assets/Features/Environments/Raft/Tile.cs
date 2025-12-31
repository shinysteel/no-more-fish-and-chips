using FishFlingers.Pools;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Prediction;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Environments
{
    public class Tile : MonoBehaviour, IPoolable
    {
        [SerializeField] private BobSettings _bobSettings;
        [SerializeField] private SinkSettings _sinkSettings;
        [SerializeField] private MeshRenderer _renderer;

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

        private Vector2Int _cell;
        private int _health;

        private Material _material;

        private const float YCoord = 0f;

        private const string DamagedBlendName = "_DamagedBlend";

        public const int DefaultHealth = 3;

        private void Awake()
        {
            _material = _renderer.material;
        }

        public void SetCell(Vector2Int cell)
        {
            _cell = cell;

            transform.position = new Vector3(cell.x, YCoord, cell.y);
        }

        public void SetHealth(int health)
        {
            _health = health;

            _material.SetFloat(DamagedBlendName, 1f - ((float)_health / DefaultHealth));
        }

        private void Update()
        {
            PositionUpdate();
        }

        private void PositionUpdate()
        {
            LayerMask mask = LayerMask.GetMask("Player");
            bool sink = Physics.CheckSphere(new Vector3(_cell.x, YCoord, _cell.y), _sinkSettings.Radius, mask);

            float targetY;

            if (sink)
            {
                // Sit just above the water
                targetY = YCoord;
            }
            else
            {
                // Bob up and down
                targetY = YCoord + _bobSettings.Amplitude * Mathf.PerlinNoise(
                    _cell.x * _bobSettings.NoiseScale + Time.time * _bobSettings.TimeScale,
                    _cell.y * _bobSettings.NoiseScale + Time.time * _bobSettings.TimeScale);
            }

            Vector3 targetPosition = new Vector3(_cell.x, targetY, _cell.y);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _sinkSettings.Speed * Time.deltaTime);
        }

        public void OnTakenFromPool()
        { }

        public void OnReturnedToPool()
        { }
    }
}