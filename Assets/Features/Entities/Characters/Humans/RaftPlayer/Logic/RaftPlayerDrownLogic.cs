using FishFlingers.Networking;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerDrownLogic
    {
        private EntityManager _entityManager;
        private NetworkManager _networkManager;

        private RaftPlayer _player;
        
        private Drowning _drowning;

        public RaftPlayerDrownLogic(RaftPlayer player)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _player = player;
        }

        public void Tick()
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (_player.RaftPlayerPhysicsModule.TimeInWater > 0.5f)
            {
                if (_drowning == null)
                {
                    _drowning = (Drowning)_entityManager.Spawn(EntityId.Drowning, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
                    _drowning.SetTarget(_player);
                }
            }
            else
            {
                if (_drowning != null)
                {
                    _entityManager.Despawn(_drowning);
                    _drowning = null;
                }
            }
        }
    }
}