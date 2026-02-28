using FishFlingers.Environments;
using FishFlingers.Scenes;
using FishFlingers.UI;
using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishFlingers.Pools
{
    public interface IPoolable
    {
        void OnTakenFromPool();
        void OnReturnedToPool();
    }

    public interface IPoolManagerListener
    { }

    public class PoolManager : GameSystem<IPoolManagerListener>
    {
        private PoolManagerConfig _config;

        private Dictionary<Type, IPoolable> _prefabRegistry = new();
        private Dictionary<Type, IPool> _pools = new();
        private Transform _container;
        
        private const string ContainerName = "Pools";

        private interface IPool
        {
            IPoolable Get(SpawnParams parameters);
            void Return(IPoolable poolable);
        }

        private class Pool<T> : IPool where T : Component, IPoolable
        {
            private SceneManager _sceneManager;

            private T _prefab;
            private Transform _container;
            private Vector3 _prefabScale;

            private Stack<T> _available = new();
            private HashSet<T> _inUse = new();

            public Pool(T prefab, Transform container)
            {
                _sceneManager = GameManager.Instance.Get<SceneManager>();

                _prefab = prefab;
                _container = container;
                _prefabScale = prefab.transform.localScale;
            }

            public IPoolable Get(SpawnParams parameters)
            {
                T obj = _available.Count > 0
                    ? _available.Pop()
                    : Object.Instantiate(_prefab, _container);

                _inUse.Add(obj);

                // When worldPositionStays is false, the child's transform does not get modified
                // when parenting to something. For pooling, this is relevant for UI where we generally
                // don't want to touch the rect transform. A side effect from this is that the child's 
                // scale is now derived from parent's scale * child's local scale, which will compound
                // whenever the parent's scale is not 1

                obj.transform.SetParent(parameters.Parent, false);
                obj.transform.localScale = _prefabScale;

                obj.transform.localPosition = parameters.Position;
                obj.transform.localRotation = parameters.Rotation;

                if (parameters.Parent == null)
                {
                    _sceneManager.MoveGameObjectToScene(obj.gameObject, parameters.SpawnScene.Get());
                }

                obj.gameObject.SetActive(true);
                obj.OnTakenFromPool();

                return obj;
            }

            public void Return(IPoolable poolable)
            {
                if (poolable is not T obj)
                {
                    return;
                }

                if (!_inUse.Contains(obj))
                {
                    return;
                }

                _inUse.Remove(obj);
                _available.Push(obj);

                obj.transform.SetParent(_container);
                obj.gameObject.SetActive(false);
                obj.OnReturnedToPool();
            }
        }

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.PoolManagerConfig;

            _container = new GameObject(ContainerName).transform;
            Object.DontDestroyOnLoad(_container.gameObject);

            foreach (IPoolable poolable in _config.PoolableScanner.GetAssets())
            {
                Register(poolable);   
            }
            
            base.Initialise(config);
        }

        private void Register<T>(T prefab) where T : IPoolable
        {
            if (prefab is not Component component)
            {
                return;
            }

            Type type = component.GetType();

            if (_prefabRegistry.ContainsKey(type))
            {
                Log.Error($"The type {type} has alreaady been registered");
                return;
            }

            _prefabRegistry[type] = prefab;
        }
        
        // Allows requesting runtime-known types
        public Component Get(Type type, SpawnParams parameters)
        {
            if (!_prefabRegistry.TryGetValue(type, out IPoolable prefab))
            {
                Log.Error($"Tried to retrieve an unregistered poolable object with type {type}");
                return null;
            }

            if (!_pools.ContainsKey(type))
            {
                Transform container = new GameObject($"{type.Name}s").transform;
                container.SetParent(_container);

                Type poolType = typeof(Pool<>).MakeGenericType(type);
                _pools[type] = (IPool)Activator.CreateInstance(poolType, prefab, container);
            }

            return (Component)_pools[type].Get(parameters);
        }

        public T Get<T>(SpawnParams parameters) where T : Component, IPoolable
        {
            return (T)Get(typeof(T), parameters);
        }

        public void Return<T>(T obj) where T : Component, IPoolable
        {
            Type type = typeof(T);
            
            if (!_pools.TryGetValue(type, out IPool pool))
            {
                Log.Error("No pool is defined for the poolable object being returned");
                return;
            }

            pool.Return(obj);
        }
    }
}