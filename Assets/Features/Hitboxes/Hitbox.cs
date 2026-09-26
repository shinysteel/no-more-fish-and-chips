using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.Hitboxes
{
    public enum HitboxShape
    {
        Box,
        Sphere
    }

    public class Hitbox : MonoBehaviour, ITypedPoolable
    {
        private PoolManager _poolManager;
        private HitboxManager _hitboxManager;

        private HitboxData _data;
        public HitboxData Data => _data;

        private Entity _source;

        private float _timer;

        private Dictionary<HitboxStep, ColliderProxy> _stepProxyMap = new();

        private Dictionary<ELayer, int> _layerCountMap = new();

        private List<Entity> _hitEntities = new();

        private List<EntityCollision> _entityCollisions = new();

        private class EntityCollision
        {
            public Entity Entity { get; private set; }
            public Collider Collider { get; private set; }
            public Collider OtherCollider { get; private set; }

            public EntityCollision(Entity entity, Collider collider, Collider otherCollider)
            {
                Entity = entity;
                Collider = collider;
                OtherCollider = otherCollider;
            }
        }

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
        }

        public void Initialise(HitboxData data, Entity source)
        {
            _data = data;
            _source = source;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            StepUpdate();

            if (_timer >= _data.HitboxDuration)
            {
                _poolManager.ReturnTypedPoolable(this);
            }
        }

        private void StepUpdate()
        {
            foreach (HitboxStep step in _data.Steps)
            {
                if (step.InTimeWindow(_timer))
                {
                    if (!_stepProxyMap.ContainsKey(step))
                    {
                        ColliderProxy proxy = null;
                        SpawnParams parameters = new SpawnParams() { Position = step.GetPosition(transform), Rotation = transform.rotation };

                        switch (step.Shape)
                        {
                            case HitboxShape.Box:
                                proxy = _poolManager.GetTypedPoolable<BoxColliderProxy>(parameters);
                                ((BoxColliderProxy)proxy).Collider.size = step.Size;
                                break;

                            case HitboxShape.Sphere:
                                proxy = _poolManager.GetTypedPoolable<SphereColliderProxy>(parameters);
                                ((SphereColliderProxy)proxy).Collider.radius = step.Radius;
                                break;
                        }

                        proxy.OnUnityTriggerStay += HandleTriggerStay;

                        _stepProxyMap.Add(step, proxy);
                    }
                }
                else
                {
                    if (_stepProxyMap.ContainsKey(step))
                    {
                        _stepProxyMap[step].OnUnityTriggerStay -= HandleTriggerStay;

                        _poolManager.ReturnTypedPoolable(_stepProxyMap[step]);

                        _stepProxyMap.Remove(step);
                    }
                }
            }
        }

        private void HandleTriggerStay(Collider collider, Collider otherCollider)
        {
            if ((_data.Mask & (1 << otherCollider.gameObject.layer)) == 0)
            {
                return;
            }

            if (!Utils.Physics.ColliderTryGetComponent(otherCollider, out Entity entity))
            {
                return;
            }

            if (entity == _source)
            {
                return;
            }

            if (!entity.isSpawned)
            {
                return;
            }

            if (_data.Alliance == entity.EntityDefinitionData.Alliance && _data.Alliance != EntityAlliance.Neutral && !(_source is RaftPlayer && entity is RaftPlayer))
            {
                return;
            }

            if (entity.EntityLifecycleLogic.InGracePeriod)
            {
                return;
            }

            _entityCollisions.Add(new EntityCollision(entity, collider, otherCollider));
        }

        private void LateUpdate()
        {
            CollisionsLateUpdate();
        }

        private void CollisionsLateUpdate()
        {
            _entityCollisions.Sort((a, b) => Vector3.Distance(a.Collider.transform.position, a.OtherCollider.transform.position).CompareTo(Vector3.Distance(b.Collider.transform.position, b.OtherCollider.transform.position)));

            foreach (EntityCollision collision in _entityCollisions)
            {
                ELayer layer = (ELayer)collision.Entity.gameObject.layer;
                HitboxLimit limit = _data.Limits.FirstOrDefault(limit => limit.Layer == layer);
                if (_layerCountMap.TryGetValue(layer, out int count) && count >= (limit?.Count ?? int.MaxValue))
                {
                    continue;
                }

                if (_hitEntities.Contains(collision.Entity))
                {
                    continue;
                }

                _layerCountMap[layer] = _layerCountMap.GetValueOrDefault(layer) + 1;

                Physics.ComputePenetration(collision.Collider, collision.Collider.transform.position, collision.Collider.transform.rotation, 
                    collision.OtherCollider, collision.OtherCollider.transform.position, collision.OtherCollider.transform.rotation, out Vector3 direction, out _);

                // Inverting penetration will produce the best direction to separate collider from otherColider
                direction = -direction;

                collision.Entity.HitRpc(collision.Entity.owner.Value, _data.Hit, direction);

                _hitEntities.Add(collision.Entity);
            }

            _entityCollisions.Clear();
        }

        public void OnReturnedToPool()
        {
            _data = null;

            _timer = 0f;

            foreach (ColliderProxy proxy in _stepProxyMap.Values)
            {
                _poolManager.ReturnTypedPoolable(proxy);
            }

            _stepProxyMap.Clear();
            _layerCountMap.Clear();
            _hitEntities.Clear();
            _entityCollisions.Clear();
        }

        public void OnTakenFromPool()
        { }

        private void OnDrawGizmos()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            
            if (!_hitboxManager.Config.DrawGizmos)
            {
                return;
            }

            Gizmos.color = _data.Alliance switch
            {
                EntityAlliance.Ally => Color.green,
                EntityAlliance.Enemy => Color.red,
                EntityAlliance.Neutral => Color.gray,
                _ => Color.gray
            };

            foreach (HitboxStep step in _data.Steps)
            {
                if (!step.InTimeWindow(_timer))
                {
                    continue;
                }

                Gizmos.matrix = Matrix4x4.TRS(step.GetPosition(transform), transform.rotation, Vector3.one);

                if (step.Shape == HitboxShape.Box)
                {
                    Gizmos.DrawCube(Vector3.zero, step.Size);
                }
                else if (step.Shape == HitboxShape.Sphere)
                {
                    Gizmos.DrawSphere(Vector3.zero, step.Radius);
                }
            }
            
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}