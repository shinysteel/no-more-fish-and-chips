using FishFlingers.Entities;
using PurrNet;
using UnityEngine;
using FishFlingers.Networking;
using FishFlingers.Scenes;

using NetworkManager = FishFlingers.Networking.NetworkManager;
using FishFlingers.States;

namespace FishFlingers.Environments
{
    public class WaveSpawner : NetBehaviour
    {
        [SerializeField] private float _spawnInterval = 2.5f;

        [SerializeField] private FlyingFish _flyingFishPrefab;

        private GameplayContext _context;

        private float _spawnTimer;

        public void Initialise(GameplayContext context)
        {
            _context = context;
        }
        
        private void Update()
        {
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
            IEntity entity = _networkManager.Spawn(_flyingFishPrefab, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
            entity.Initialise(_context);
        }
    }
}