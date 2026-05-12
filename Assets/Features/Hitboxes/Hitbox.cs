using FishFlingers.Entities;
using FishFlingers.Pools;
using ShinyOwl.Common;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace FishFlingers.Hitboxes
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

        private float _timer;

        private Collider[] _collidersNonAlloc = new Collider[MaxOverlaps];
        private const int MaxOverlaps = 10;

        private List<IEntity> _hitEntities = new();

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
        }

        public void Initialise(HitboxData data)
        {
            _data = data;
        }

        private void FixedUpdate()
        {
            _timer += Time.fixedDeltaTime;

            StepFixedUpdate();

            if (_timer >= _data.HitboxDuration)
            {
                _poolManager.ReturnTypedPoolable(this);
            }
        }

        private void StepFixedUpdate()
        {
            foreach (HitboxStep step in _data.Steps)
            {
                if (!step.InTimeWindow(_timer))
                {
                    continue;
                }

                int overlaps = step.Shape switch
                {
                    HitboxShape.Box => Physics.OverlapBoxNonAlloc(step.GetPosition(transform), step.Size * 0.5f, _collidersNonAlloc, transform.rotation, _data.Mask),
                    HitboxShape.Sphere => Physics.OverlapSphereNonAlloc(step.GetPosition(transform), step.Radius, _collidersNonAlloc, _data.Mask),
                    _ => 0
                };

                for (int i = 0; i < overlaps; i++)
                {
                    if (!_collidersNonAlloc[i].TryGetComponent(out IEntity entity))
                    {
                        continue;
                    }

                    if (!entity.IsSpawned)
                    {
                        continue;
                    }

                    if (_hitEntities.Contains(entity))
                    {
                        continue;
                    }

                    if (_data.Alliance == entity.EntityDefinitionData.Alliance && _data.Alliance != EntityAlliance.Neutral)
                    {
                        continue;
                    }

                    if (entity.EntityLifecycleModule.InGracePeriod)
                    {
                        continue;
                    }

                    // Hit the entity
                    entity.EntityHealthModule.ChangeHealth(-_data.Damage);

                    // Damaging an entity can cause it to despawn, which nulls all modules
                    if (entity.IsSpawned)
                    {
                        Vector3 forceDirection = (entity.Transform.position - transform.position).normalized;
                        Vector3 force = forceDirection * _data.KnockbackForceStrength;

                        Vector3 torqueDirection = forceDirection;
                        torqueDirection.y = 0f;
                        torqueDirection.Normalize();
                        torqueDirection = -Vector3.Cross(torqueDirection, Vector3.up);
                        Vector3 torque = torqueDirection * _data.KnockbackTorqueStrength;

                        if (entity is NetEntity netEntity)
                        {
                            netEntity.AddForceRpc(netEntity.owner.Value, force);
                            netEntity.AddTorqueRpc(netEntity.owner.Value, torque);
                        }
                        else
                        {
                            entity.EntityPhysicsModule.Rigidbody.AddForce(force, ForceMode.Impulse);
                            entity.EntityPhysicsModule.Rigidbody.AddTorque(torque, ForceMode.Impulse);
                        }

                        if (entity is Character character)
                        {
                            character.StunRpc(_data.StunDuration);
                        }

                        // Manual AnimateHurt, since RaftPlayers aren't damageable but we still want to show it
                        if (entity is RaftPlayer player)
                        {
                            player.AnimateHurtRpc();
                        }
                    }

                    _hitEntities.Add(entity);
                }
            }
        }
        
        public void OnReturnedToPool()
        {
            _data = null;

            _timer = 0f;

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