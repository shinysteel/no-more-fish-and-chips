using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.Pools;

namespace NoMoreFishAndChips.Entities
{
    public enum RaftPlayerAttackState
    {
        None,
        Windup,
        Impact
    }

    public class RaftPlayerAttackLogic
    {
        private HitboxManager _hitboxManager;

        private RaftPlayer _player;

        private RaftPlayerAttackSettings _settings;

        private RaftPlayerAttackState _attackState;
        public RaftPlayerAttackState AttackState => _attackState;
        
        public RaftPlayerAttackLogic(RaftPlayer player)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();

            _player = player;

            _settings = _player.DefinitionData.AttackSettings;

            if (_player.isOwner)
            {
                _player.AnimateLogic.AttackStateAnimationEvents.Add(new StateAnimationEvent(0.5f, Lunge));
                _player.AnimateLogic.AttackStateAnimationEvents.Add(new StateAnimationEvent(1f, () => _attackState = RaftPlayerAttackState.None));
            }
        }

        public void Attack()
        {
            if (_attackState > RaftPlayerAttackState.None)
            {
                return;
            }

            // StateAnimationEvents aren't perfect. Ideally this would be set to normalised time of 0f, but this can be too late for the
            // IsAttacking bool to be valid. For example, an Attack trigger can be interrupted by a Jump trigger
            _attackState = RaftPlayerAttackState.Windup;
            
            _player.AnimateLogic.Attack();
        }

        private void Lunge()
        {
            _attackState = RaftPlayerAttackState.Impact;
            _player.EntityPhysicsModule.Rigidbody.AddForce(_player.transform.forward * _settings.LungeStrength, ForceMode.Impulse);

            _hitboxManager.SpawnHitbox(_settings.HitboxData, new SpawnParams() { Position = _player.transform.position, Rotation = _player.transform.rotation });
        }
    }
}