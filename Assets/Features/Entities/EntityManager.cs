using FishFlingers.Networking;
using FishFlingers.Scenes;
using UnityEngine;

namespace FishFlingers.Entities
{
    public enum EEntity
    {
        RaftPlayer  ,
        RaftTile    ,
        DroppedItem , 
        FlyingFish  ,
    }

    public interface IEntityManagerListener
    { }

    public class EntityManager : GameSystem<IEntityManagerListener>
    {
        private NetworkManager _networkManager;

        private EntityManagerConfig _config;

        public override void Initialise(GameManagerConfig config)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _config = config.EntityManagerConfig;

            base.Initialise(config);
        }

        public T Spawn<T>(T prefab, SpawnParams parameters) where T : IEntity
        {
            if (prefab is NetEntity netEntity)
            {
                // _networkManager.Spawn(netEntity, )
            }
            else if (prefab is Entity entity)
            {

            }

            return default;
        }
    }
}