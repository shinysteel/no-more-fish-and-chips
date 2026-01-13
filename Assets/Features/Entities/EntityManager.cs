using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.Scenes;
using ShinyOwl.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using Object = UnityEngine.Object;

namespace FishFlingers.Entities
{
    public enum EEntity
    {
        RaftTile    = 1   ,
        DroppedItem = 2   ,

        // Characters
        RaftPlayer  = 100 ,
        FlyingFish  = 101 ,

        // Structures
        WaveSign    = 200 ,
    }

    public interface IEntityManagerListener
    { }

    public class EntityManager : GameSystem<IEntityManagerListener>
    {
        private NetworkManager _networkManager;
        private PoolManager _poolManager;

        private EntityManagerConfig _config;

        private Dictionary<EEntity, IEntity> _entityPrefabMap = new();
        private Dictionary<Type, HashSet<IEntity>> _typeEntitiesMap = new();

        public override void Initialise(GameManagerConfig config)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.EntityManagerConfig;

            // Entity prefab map
            foreach (EntityMapping mapping in _config.EntityMappings)
            {
                _entityPrefabMap.Add(mapping.Entity, mapping.Prefab.GetComponent<IEntity>());
            }

            // Type entities map
            Type[] types = new Type[] { typeof(Structure) };

            foreach (Type type in types)
            {
                _typeEntitiesMap.Add(type, new());
            }

            foreach (EntityMapping mapping in _config.EntityMappings)
            {
                if (!mapping.Prefab.TryGetComponent(out IEntity entity))
                {
                    Debugger.LogError(this, $"Could not find a component that implements IEntity on {mapping.Prefab}");
                    continue;
                }

                foreach (Type type in types)
                {
                    if (type.IsAssignableFrom(entity.GetType()))
                    {
                        _typeEntitiesMap[type].Add(entity);
                    }
                }
            }

            base.Initialise(config);
        }

        /// <summary>
        /// Retrieves a registered collection of entities
        /// </summary>
        public IEnumerable<T> GetEntities<T>() where T : IEntity
        {
            if (!_typeEntitiesMap.TryGetValue(typeof(T), out HashSet<IEntity> entities))
            {
                return Enumerable.Empty<T>();
            }

            return entities.OfType<T>();
        }

        /// <summary>
        /// Centralised spawn method for entities, handling NetEntity, Entity + Poolable and Entity all in one
        /// </summary>
        public IEntity Spawn(EEntity type, SpawnParams parameters)
        {
            if (!_entityPrefabMap.TryGetValue(type, out IEntity prefab))
            {
                Debugger.LogError(this, $"The entity {type} has not been mapped to a prefab");
                return default;
            }

            // NetEntity
            if (prefab is NetEntity netEntity)
            {
                return _networkManager.Spawn(netEntity, parameters);
            }

            Entity entity = prefab as Entity;

            // Entity + Poolable
            if (entity is IPoolable)
            {
                return (IEntity)_poolManager.Get(entity.GetType(), parameters);
            }

            // Entity
            if (entity != null)
            {
                return parameters.Spawn(entity);
            }

            Debugger.LogError(this, $"Failed to cast {prefab} into a known entity class");
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
            if (obj is IPoolable)
            {
                _poolManager.Return(obj);
                return;
            }

            // Entity
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
                return;
            }
        }
    }
}