using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;
using FishFlingers.Items;
using System;
using FishFlingers.Audio;

namespace FishFlingers.Entities
{
    public class RaftPlayerAnimateLogic
    {
        private AudioManager _audioManager;

        private RaftPlayer _player;

        private StateAnimationEvents _moveStateAnimationEvents;
        private StateAnimationEvents _attackStateAnimationEvents;

        public StateAnimationEvents AttackStateAnimationEvents => _attackStateAnimationEvents;

        private const string IsMovingBoolName = "IsMoving";
        private const string IsHoldingItemBoolName = "IsHoldingItem";
        private const string IsAttackingBoolName = "IsAttacking";
        private const string InBarrelBoolName = "InBarrel";
        private const string InWaterBoolName = "InWater";

        private const string AttackTriggerName = "Attack";

        private const string RunStateName = "Base Layer.Ground.Run";
        private const string AttackStateName = "Attack";

        public RaftPlayerAnimateLogic(RaftPlayer player)
        {
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _player = player;

            _moveStateAnimationEvents = new StateAnimationEvents(RunStateName, true)
            {
                new StateAnimationEvent(0.1f, () => _audioManager.PlaySound(SoundId.Footstep)),
                new StateAnimationEvent(0.6f, () => _audioManager.PlaySound(SoundId.Footstep))
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
                bool isHoldingItem = _player.Hotbar.SelectedSlot.InventoryItem != null;
                bool isAttacking = _player.AttackLogic.AttackState > RaftPlayerAttackState.None;
                bool inBarrel = _player.RaftPlayerDefeatModule.InBarrel;
                bool inWater = _player.RaftPlayerPhysicsModule.InWater;

                _player.CharacterModel.Animator.SetBool(IsMovingBoolName, isMoving);
                _player.CharacterModel.Animator.SetBool(IsHoldingItemBoolName, isHoldingItem);
                _player.CharacterModel.Animator.SetBool(IsAttackingBoolName, isAttacking);
                _player.CharacterModel.Animator.SetBool(InBarrelBoolName, inBarrel);
                _player.CharacterModel.Animator.SetBool(InWaterBoolName, inWater);
            }

            AnimatorStateInfo info = _player.CharacterModel.Animator.GetCurrentAnimatorStateInfo(0);

            _moveStateAnimationEvents.Tick(info);
            _attackStateAnimationEvents.Tick(info);
        }

        public void Attack()
        {
            _player.CharacterModel.SetTrigger(AttackTriggerName);
        }
    }
}