using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace NoMoreFishAndChips.Entities
{
    public interface IEntityManagerListener
    {
        void OnEntitySpawned(Entity entity) { }
        void OnEntityDespawned(Entity entity) { }
    }

    public class EntityManager : GameSystem<IEntityManagerListener>
    {
        private NetworkManager _networkManager;
        private PoolManager _poolManager;

        private EntityManagerConfig _config;

        private Dictionary<EntityId, Entity> _idPrefabMap = new();
        private Dictionary<Type, HashSet<Entity>> _typePrefabsMap = new();

        private Dictionary<EntityId, EntityModel> _idModelMap = new();
        private Dictionary<EntityId, Pool<EntityModel>> _modelPools = new();

        private List<Entity> _entities = new();
        public IReadOnlyList<Entity> Entities => _entities;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.EntityManagerConfig;

            // Entity prefab map
            foreach (Entity prefab in _config.EntityScanner.GetAssets())
            {
                _idPrefabMap.Add(prefab.EntityDefinitionData.Id, prefab);
            }

            // Type entities map
            Type[] types = new Type[] { typeof(RaftTile), typeof(Structure) };

            foreach (Type type in types)
            {
                _typePrefabsMap.Add(type, new());
            }

            foreach (Entity entity in _idPrefabMap.Values)
            {
                foreach (Type type in types)
                {
                    if (type.IsAssignableFrom(entity.GetType()))
                    {
                        _typePrefabsMap[type].Add(entity);
                    }
                }
            }

            // Entity model map
            foreach (EntityModel model in _config.EntityModelScanner.GetAssets())
            {
                _idModelMap.Add(model.Id, model);
            }

            base.InitialiseConfig(config);
        }

        /// <summary>
        /// Retrieves a single entity mapped to the type
        /// </summary>
        public Entity GetPrefab(EntityId id)
        {
            return _idPrefabMap[id];
        }

        /// <summary>
        /// Retrieves a registered collection of entities
        /// </summary>
        public IEnumerable<T> GetPrefabs<T>() where T : Entity
        {
            if (!_typePrefabsMap.TryGetValue(typeof(T), out HashSet<Entity> entities))
            {
                return Enumerable.Empty<T>();
            }

            return entities.OfType<T>();
        }

        // Some conditions may fail an enemy spawn request
        public bool TrySpawnEnemy(EntityId id, SpawnParams parameters, GameplayContext context, out Enemy enemy)
        {
            enemy = default;

            if (!_idPrefabMap.TryGetValue(id, out Entity entityPrefab))
            {   
                return false;
            }

            if (entityPrefab is not Enemy enemyPrefab)
            {
                return false;
            }

            return enemyPrefab.TrySpawn(out enemy);
        }

        // Centralised spawn method for entities, handling NetEntity, Entity + Poolable and Entity all in one
        public Entity Spawn(EntityId id, SpawnParams parameters)
        {
            if (!_idPrefabMap.TryGetValue(id, out Entity prefab))
            {
                Log.Error($"The entity {id} has not been mapped to a prefab");
                return default;
            }

            return _networkManager.Spawn(prefab, parameters);
        }

        public void Despawn(Entity entity)
        {
            _networkManager.Despawn(entity);
        }

        public EntityModel GetModel(EntityId id, SpawnParams parameters)
        {
            return _poolManager.GetPoolable(_modelPools, id, _idModelMap[id], parameters);
        }

        public void ReturnModel(EntityModel model)
        {
            _poolManager.ReturnPoolable(model, model.Id, _modelPools);
        }

        // Since Entity lifecycle is controlled by Purrnet, we need to manually raise these events
        public void RaiseEntitySpawned(Entity entity) => NotifyEntitySpawned(entity);
        public void RaiseEntityDespawned(Entity entity) => NotifyEntityDespawned(entity);

        private void NotifyEntitySpawned(Entity entity)
        {
            _entities.Add(entity);

            Listeners.Dispatch(listener => listener.OnEntitySpawned(entity));
        }

        private void NotifyEntityDespawned(Entity entity)
        {
            _entities.Remove(entity);

            Listeners.Dispatch(listener => listener.OnEntityDespawned(entity));
        }
    }
}