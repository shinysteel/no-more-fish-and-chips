using PurrNet;
using UnityEngine;
using FishFlingers.Entities;

using NetworkManager = FishFlingers.Networking.NetworkManager;

namespace FishFlingers.Environments
{
    public class SalvageSpawner : NetworkBehaviour
    {
        [SerializeField] private Item _driftwoodPrefab;

        [SerializeField] private float _spawnInterval = 5f;

        private NetworkManager _networkManager;

        private Raft _raft;

        private float _spawnTimer;

        public void Initialise(Raft raft)   
        {
            _raft = raft;
        }

        protected override void OnInitializeModules()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
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
            int minSpread = 3;
            float x = Random.Range((float)Mathf.Min(-minSpread, _raft.LeftmostColumn), Mathf.Max(minSpread, _raft.RightmostColumn));
            int forwardDist = 10;
            int y = _raft.ForwardmostRow + forwardDist;
            Vector3 position = _raft.CellToWorldPosition(new Vector2(x, y));

            Item item = _networkManager.Spawn(_driftwoodPrefab, position);
            item.Initialise(_raft);
        }
    }
}