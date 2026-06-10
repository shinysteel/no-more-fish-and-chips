using NoMoreFishAndChips.Audio;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerAnimateLogic
    {
        private AudioManager _audioManager;

        private RaftPlayer _player;

        private StateAnimationEvents _groundRunStateAnimationEvents;
        private StateAnimationEvents _waterSwimStateAnimationEvents;
        private StateAnimationEvents _attackStateAnimationEvents;

        public StateAnimationEvents AttackStateAnimationEvents => _attackStateAnimationEvents;

        private const string IsMovingBoolName = "IsMoving";
        private const string InWaterBoolName = "InWater";
        private const string InAirBoolName = "InAir";
        private const string IsHoldingItemBoolName = "IsHoldingItem";
        private const string IsAttackingBoolName = "IsAttacking";
        private const string InBarrelBoolName = "InBarrel";
        
        private const string AttackTriggerName = "Attack";
        private const string JumpTriggerName = "Jump";

        private const string GroundRunStateName = "Base Layer.Ground.Run";
        private const string WaterSwimStateName = "Base Layer.Water.Swim";
        private const string AttackStateName = "Attack";

        public RaftPlayerAnimateLogic(RaftPlayer player)
        {
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _player = player;

            _groundRunStateAnimationEvents = new StateAnimationEvents(GroundRunStateName, true)
            {
                new StateAnimationEvent(0.1f, () => _audioManager.PlaySound(SoundId.HumanFootstep)),
                new StateAnimationEvent(0.6f, () => _audioManager.PlaySound(SoundId.HumanFootstep))
            };

            _waterSwimStateAnimationEvents = new StateAnimationEvents(WaterSwimStateName, true)
            {
                new StateAnimationEvent(0.2f, () =>
                {
                    if (_player.Hotbar.SelectedSlot.InventoryItem == null)
                    {
                        _audioManager.PlaySound(SoundId.HumanSwim);
                    }
                }),
                new StateAnimationEvent(0.7f, () => _audioManager.PlaySound(SoundId.HumanSwim)),
            };

            _attackStateAnimationEvents = new StateAnimationEvents(AttackStateName, false)
            {
                new StateAnimationEvent(0.3f, () => _audioManager.PlaySound(SoundId.PaddleAttack)),
                new StateAnimationEvent(0.3f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(true)),
                new StateAnimationEvent(0.7f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(false)),
            };
        }

        public void Tick()
        {
            if (_player.isOwner)
            {
                bool isMoving = _player.InputLogic.MoveDirection != Vector3.zero;
                bool inWater = _player.RaftPlayerPhysicsModule.InWater;
                bool inAir = _player.RaftPlayerPhysicsModule.InAir;
                bool isHoldingItem = _player.Hotbar.SelectedSlot.InventoryItem != null;
                bool isAttacking = _player.AttackLogic.AttackState > RaftPlayerAttackState.None;
                bool inBarrel = _player.RaftPlayerDefeatModule.InBarrel;

                _player.CharacterModel.Animator.SetBool(IsMovingBoolName, isMoving);
                _player.CharacterModel.Animator.SetBool(InWaterBoolName, inWater);
                _player.CharacterModel.Animator.SetBool(InAirBoolName, inAir);
                _player.CharacterModel.Animator.SetBool(IsHoldingItemBoolName, isHoldingItem);
                _player.CharacterModel.Animator.SetBool(IsAttackingBoolName, isAttacking);
                _player.CharacterModel.Animator.SetBool(InBarrelBoolName, inBarrel);
            }

            AnimatorStateInfo info = _player.CharacterModel.Animator.GetCurrentAnimatorStateInfo(0);

            _groundRunStateAnimationEvents.Tick(info);
            _waterSwimStateAnimationEvents.Tick(info);
            _attackStateAnimationEvents.Tick(info);
        }

        public void Attack()
        {
            _player.CharacterModel.SetTrigger(AttackTriggerName);
        }

        public void Jump()
        {
            _player.CharacterModel.SetTrigger(JumpTriggerName);
        }
    }
}