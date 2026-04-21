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

    public class Hitbox : MonoBehaviour, IPoolable
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

        private void Update()
        {
            _timer += Time.deltaTime;

            StepUpdate();

            if (_timer >= _data.Duration)
            {
                _poolManager.ReturnPoolable(this);
            }
        }

        private void StepUpdate()
        {
            foreach (HitboxStep step in _data.Steps)
            {
                if (!step.InTimeWindow(_timer))
                {
                    continue;
                }

                int overlaps = step.Shape switch
                {
                    HitboxShape.Box => Physics.OverlapBoxNonAlloc(step.GetPosition(transform), step.Size * 0.5f, _collidersNonAlloc),
                    HitboxShape.Sphere => Physics.OverlapSphereNonAlloc(step.GetPosition(transform), step.Radius, _collidersNonAlloc),
                    _ => 0
                };

                for (int i = 0; i < overlaps; i++)
                {
                    if (!_collidersNonAlloc[i].TryGetComponent(out IEntity entity))
                    {
                        continue;
                    }

                    if (_hitEntities.Contains(entity))
                    {
                        continue;
                    }

                    if (_data.Alliance == entity.EntityData.Alliance && _data.Alliance != EntityAlliance.Neutral)
                    {
                        continue;
                    }

                    // Hit the entity
                    entity.HealthModule.ChangeHealth(-_data.Damage);

                    Vector3 forceDirection = (entity.Transform.position - transform.position).normalized;
                    entity.Rigidbody.AddForce(forceDirection * _data.KnockbackForceStrength, ForceMode.Impulse);

                    Vector3 torqueDirection = forceDirection;
                    torqueDirection.y = 0f;
                    torqueDirection.Normalize();
                    torqueDirection = -Vector3.Cross(torqueDirection, Vector3.up);
                    entity.Rigidbody.AddTorque(torqueDirection * _data.KnockbackTorqueStrength, ForceMode.Impulse);

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
                
                if (step.Shape == HitboxShape.Box)
                {
                    Gizmos.DrawCube(step.GetPosition(transform), step.Size);
                }
                else if (step.Shape == HitboxShape.Sphere)
                {
                    Gizmos.DrawSphere(step.GetPosition(transform), step.Radius);
                }
            }
        }
    }
}