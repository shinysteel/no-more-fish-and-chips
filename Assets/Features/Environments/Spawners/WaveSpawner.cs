using FishFlingers.Entities;
using PurrNet;
using UnityEngine;

using NetworkManager = FishFlingers.Networking.NetworkManager;

namespace FishFlingers.Environments
{
    public class WaveSpawner : NetworkBehaviour
    {
        [SerializeField] private float _spawnInterval = 2.5f;

        [SerializeField] private FlyingFish _flyingFishPrefab;

        private NetworkManager _networkManager;

        private Raft _raft;

        private float _spawnTimer;

        protected override void OnInitializeModules()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        public void Initialise(Raft raft)
        {
            _raft = raft;
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

            _spawnTimer += Time.deltaTime;

            if (_spawnTimer < _spawnInterval)
            {
                return;
            }

            _spawnTimer -= _spawnInterval;

            Spawn();
        }

        private void Spawn()
        {
            IEntity entity = _networkManager.Spawn(_flyingFishPrefab, NetworkManager.HiddenSpawnPosition);
            entity.Initialise(_raft);
        }
    }
}