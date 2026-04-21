using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;

namespace FishFlingers.Entities
{
    public enum RaftPlayerAttackState
    {
        None,
        Windup,
        Impact
    }

    public class RaftPlayerAttackLogic
    {
        private RaftPlayer _player;

        private RaftPlayerAttackState _attackState;
        public RaftPlayerAttackState AttackState => _attackState;
        
        public RaftPlayerAttackLogic(RaftPlayer player)
        {
            _player = player;
        }

        public async Task AttackAsync()
        {
            if (_attackState > RaftPlayerAttackState.None)
            {
                return;
            }

            _attackState = RaftPlayerAttackState.Windup;

            AnimateEvents events = new AnimateEvents()
            {
                new AnimateEvent(0.5f, () =>
                {
                    _player.Rigidbody.AddForce(_player.transform.forward, ForceMode.Impulse);
                    _attackState = RaftPlayerAttackState.Impact;

                    EntityManager entityManager = GameManager.Instance.Get<EntityManager>();
                    foreach (IEntity entity in entityManager.Entities)
                    {
                        if (entity.EntityData.Alliance == EntityAlliance.Ally)
                        {
                            continue;
                        }

                        float distance = Vector3.Distance(_player.transform.position, entity.Rigidbody.position);
                        if (distance <= 2.5f)
                        {
                            entity.HealthModule.ChangeHealth(-1);
                        }
                    }
                }),
            };

            await _player.AnimateLogic.AttackAsync(events);

            _attackState = RaftPlayerAttackState.None;
        }
    }
}