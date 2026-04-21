using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;
using FishFlingers.Hitboxes;
using FishFlingers.Pools;

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
        private PoolManager _poolManager;

        private RaftPlayer _player;

        private RaftPlayerAttackSettings _settings;

        private RaftPlayerAttackState _attackState;
        public RaftPlayerAttackState AttackState => _attackState;
        
        public RaftPlayerAttackLogic(RaftPlayer player)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _player = player;

            _settings = _player.Data.AttackSettings;
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
                    _player.Rigidbody.AddForce(_player.transform.forward * _settings.LungeStrength, ForceMode.Impulse);

                    _attackState = RaftPlayerAttackState.Impact;

                    Hitbox hitbox = _poolManager.GetPoolable<Hitbox>(new SpawnParams() { Position = _player.transform.position, Rotation = _player.transform.rotation });
                    hitbox.Initialise(_settings.HitboxData);
                }),
            };

            await _player.AnimateLogic.AttackAsync(events);

            _attackState = RaftPlayerAttackState.None;
        }
    }
}