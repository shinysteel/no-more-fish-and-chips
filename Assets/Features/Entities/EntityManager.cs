using FishFlingers.Cameras;
using FishFlingers.Instantiating;
using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.Scenes;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace FishFlingers.Entities
{
    public enum EntityId
    {
        None        = 0   ,
        
        DroppedItem = 1   ,
        
        // Characters
        RaftPlayer  = 100 ,
        FlyingFish  = 101 ,
        Shark       = 102 ,
        Seagull     = 103 ,
        Drowning    = 104 ,
        Crab        = 105 ,

        // Tiles
        GoopTile    = 201 ,
        WoodenTile  = 202 ,
        MetalTile   = 203 ,

        // Structures
        WaveSign    = 300 ,
        ClamChest   = 301 ,
        Planter     = 302 ,
    }

    public interface IEntityManagerListener
    {
        void OnEntitySpawned(IEntity entity) { }
        void OnEntityDespawned(IEntity entity) { }
    }

    public class EntityManager : GameSystem<IEntityManagerListener>
    {
        private NetworkManager _networkManager;
        private PoolManager _poolManager;

        private EntityManagerConfig _config;

        private Dictionary<EntityId, IEntity> _idPrefabMap = new();
        private Dictionary<EntityId, EntityModel> _idModelMap = new();

        private Dictionary<Type, HashSet<IEntity>> _typePrefabsMap = new();

        private List<IEntity> _entities = new();
        public IReadOnlyList<IEntity> Entities => _entities;

        public override void Initialise(GameManagerConfig config)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.EntityManagerConfig;

            // Entity prefab map
            foreach (IEntity prefab in _config.IEntityScanner.GetAssets())
            {
                _idPrefabMap.Add(prefab.EntityDefinitionData.Id, prefab);
            }

            // Entity model map
            foreach (EntityModel model in _config.EntityModelScanner.GetAssets())
            {
                _idModelMap.Add(model.Id, model);
            }

            // Type entities map
            Type[] types = new Type[] { typeof(Tile), typeof(Structure) };

            foreach (Type type in types)
            {
                _typePrefabsMap.Add(type, new());
            }

            foreach (IEntity entity in _idPrefabMap.Values)
            {
                foreach (Type type in types)
                {
                    if (type.IsAssignableFrom(entity.GetType()))
                    {
                        _typePrefabsMap[type].Add(entity);
                    }
                }
            }

            base.Initialise(config);
        }

        /// <summary>
        /// Retrieves a single entity mapped to the type
        /// </summary>
        public IEntity GetEntityPrefab(EntityId id)
        {
            return _idPrefabMap[id];
        }

        public EntityModel GetEntityModel(EntityId id)
        {
            return _idModelMap[id];
        }

        /// <summary>
        /// Retrieves a registered collection of entities
        /// </summary>
        public IEnumerable<T> GetEntityPrefabs<T>() where T : IEntity
        {
            if (!_typePrefabsMap.TryGetValue(typeof(T), out HashSet<IEntity> entities))
            {
                return Enumerable.Empty<T>();
            }

            return entities.OfType<T>();
        }

        /// <summary>
        /// Centralised spawn method for entities, handling NetEntity, Entity + Poolable and Entity all in one
        /// </summary>
        public IEntity Spawn(EntityId id, SpawnParams parameters)
        {
            if (!_idPrefabMap.TryGetValue(id, out IEntity prefab))
            {
                Log.Error($"The entity {id} has not been mapped to a prefab");
                return default;
            }

            // NetEntity
            if (prefab is NetEntity netEntity)
            {
                return _networkManager.Spawn(netEntity, parameters);
            }

            Entity entity = prefab as Entity;

            // Entity + Poolable
            if (entity is ITypedPoolable)
            {
                return (IEntity)_poolManager.GetTypedPoolable(entity.GetType(), parameters);
            }

            // Entity
            if (entity != null)
            {
                return parameters.Spawn(entity);
            }

            Log.Error($"Failed to cast {prefab} into a known entity class");
            return default;
        }

        public void Despawn(IEntity entity)
        {
            // NetEntity
            if (entity is NetEntity netEntity)
            {
                _networkManager.Despawn(netEntity);
                return;
            }

            Entity obj = entity as Entity;

            // Entity + Poolable
            if (obj is ITypedPoolable)
            {
                _poolManager.ReturnTypedPoolable(obj);
                return;
            }

            // Entity
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
                return;
            }

            Log.Error($"The top-level class of {entity} is unknown, and so it couldn't be despawned");
        }

        // Since NetEntity lifecycle is controlled by Purrnet, we need to manually raise these events
        public void RaiseNetEntitySpawned(IEntity entity) => NotifyNetEntitySpawned(entity);
        public void RaiseNetEntityDespawned(IEntity entity) => NotifyNetEntityDespawned(entity);

        private void NotifyNetEntitySpawned(IEntity entity)
        {
            _entities.Add(entity);

            Listeners.Dispatch(listener => listener.OnEntitySpawned(entity));
        }

        private void NotifyNetEntityDespawned(IEntity entity)
        {
            _entities.Remove(entity);

            Listeners.Dispatch(listener => listener.OnEntityDespawned(entity));
        }
    }
}