using FishFlingers.Entities;
using UnityEngine;
using FishFlingers.Networking;
using ShinyOwl.Common;

using NetworkManager = FishFlingers.Networking.NetworkManager;
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Environments
{
    public class WaveSpawner : GameplayBehaviour
    {
        [SerializeField] private EntityId _entityId;
        [SerializeField] private float _spawnInterval = 2.5f;
        [SerializeField] private float _initialDelay = 1f;
        [SerializeField] private bool _prewarm;

        private float _spawnTimer;

        protected override void OnSpawned()
        {
            if (isServer)
            {
                if (_prewarm)
                {
                    _spawnTimer = _spawnInterval;
                }

                _spawnTimer -= _initialDelay;
            }

            base.OnSpawned();
        }

        private void Update()
        {
            if (!isSpawned)
            {
                return;
            }

            SpawnUpdate();
        }

        private void SpawnUpdate()
        {
            if (!isServer)
            {
                return;
            }

            if (_spawnTimer < _spawnInterval)
            {
                _spawnTimer += Time.deltaTime;
                return;
            }

            _spawnTimer -= _spawnInterval;

            Spawn();
        }

        private void Spawn()
        {
            _entityManager.Spawn(_entityId, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
        }
    }
}