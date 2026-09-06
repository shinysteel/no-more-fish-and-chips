using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

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
            if (!otherCollider.TryGetComponent(out Entity entity))
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

            ELayer layer = (ELayer)entity.gameObject.layer;
            HitboxLimit limit = _data.Limits.FirstOrDefault(limit => limit.Layer == layer);
            if (_layerCountMap.TryGetValue(layer, out int count) && count >= (limit?.Count ?? int.MaxValue))
            {
                return;
            }

            if (_hitEntities.Contains(entity))
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

            _layerCountMap[layer] = _layerCountMap.GetValueOrDefault(layer) + 1;

            // Hit the entity
            entity.EntityHealthLogic.ChangeHealth(-_data.Damage);

            // Damaging an entity can cause it to despawn, which nulls all modules
            if (entity.isSpawned)
            {
                Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, otherCollider, otherCollider.transform.position, otherCollider.transform.rotation, out Vector3 forceDirection, out _);
               
                forceDirection.y = 0f;
                forceDirection.Normalize();

                // Inverting penetration will produce the best direction to separate collider from otherColider
                forceDirection = -forceDirection;

                // Torque is dependent on the horizontal value of forceDirection
                Vector3 torqueDirection = forceDirection;

                // Universal pitching for hitbox force
                forceDirection = Quaternion.AngleAxis(45f, Vector3.Cross(forceDirection, Vector3.up).normalized) * forceDirection;
                Vector3 force = forceDirection * _data.KnockbackForceStrength;

                // Using the cross product, torque can make the entity rotate backwards relative to the hitbox
                torqueDirection = -Vector3.Cross(torqueDirection, Vector3.up);
                Vector3 torque = torqueDirection * _data.KnockbackTorqueStrength;

                entity.AddForceRpc(entity.owner.Value, force);
                entity.AddTorqueRpc(entity.owner.Value, torque);

                if (entity is Character character)
                {
                    character.StunRpc(character.owner.Value, _data.StunDuration);
                }

                // Manual AnimateHurt, since RaftPlayers aren't damageable but we still want to show it
                if (entity is RaftPlayer player)
                {
                    player.AnimateHurtRpc();
                }
            }

            _hitEntities.Add(entity);
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