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
        private StateAnimationEvents _jumpStateAnimationEvents;
        private StateAnimationEvents _paddleSwingStateAnimationEvents;
        private StateAnimationEvents _spearJab1StateAnimationEvents;
        private StateAnimationEvents _spearJab2StateAnimationEvents;

        public StateAnimationEvents PaddleSwingStateAnimationEvents => _paddleSwingStateAnimationEvents;
        public StateAnimationEvents SpearJab1StateAnimationEvents => _spearJab1StateAnimationEvents;
        public StateAnimationEvents SpearJab2StateAnimationEvents => _spearJab2StateAnimationEvents;

        private const string BaseLayerName = "Base Layer";
        private const string AttackLayerName = "Attack Layer";

        private const string IsMovingBoolName = "IsMoving";
        private const string InWaterBoolName = "InWater";
        private const string InAirBoolName = "InAir";
        private const string IsHoldingItemBoolName = "IsHoldingItem";
        private const string InBarrelBoolName = "InBarrel";

        public const string AttackWeaponTypeIntName = "AttackWeaponType";
        public const string AttackStateIntName = "AttackState";

        private const string JumpTriggerName = "Jump";

        private const string GroundRunStateName = BaseLayerName + ".Ground.Run";
        private const string WaterSwimStateName = BaseLayerName + ".Water.Swim";
        private const string JumpStateName = BaseLayerName + ".Jump";
        private const string PaddleSwingStateName = AttackLayerName + ".Paddle.Swing";
        private const string SpearJab1StateName = AttackLayerName + ".Spear.Jab1";
        private const string SpearJab2StateName = AttackLayerName + ".Spear.Jab2";

        public RaftPlayerAnimateLogic(RaftPlayer player)
        {
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _player = player;

            _groundRunStateAnimationEvents = new StateAnimationEvents(GroundRunStateName, true)
            {
                new StateAnimationEvent(0.1f, () => _audioManager.PlaySound(SoundId.HumanFootstepWood)),
                new StateAnimationEvent(0.6f, () => _audioManager.PlaySound(SoundId.HumanFootstepWood))
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

            _jumpStateAnimationEvents = new StateAnimationEvents(JumpStateName, false)
            {
                new StateAnimationEvent(0f, () => _audioManager.PlaySound(SoundId.HumanJumpWood))
            };

            _paddleSwingStateAnimationEvents = new StateAnimationEvents(PaddleSwingStateName, false)
            {
                new StateAnimationEvent(0f, () => _audioManager.PlaySound(SoundId.PaddleSwing)),
                new StateAnimationEvent(0f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(true)),
                new StateAnimationEvent(0.66f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(false))
            };

            _spearJab1StateAnimationEvents = new StateAnimationEvents(SpearJab1StateName, false)
            {
                new StateAnimationEvent(0.2f, () => _audioManager.PlaySound(SoundId.SpearJab)),
                new StateAnimationEvent(0.2f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(true)),
                new StateAnimationEvent(0.8f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(false))
            };

            _spearJab2StateAnimationEvents = new StateAnimationEvents(SpearJab2StateName, false)
            {
                new StateAnimationEvent(0.2f, () => _audioManager.PlaySound(SoundId.SpearJab)),
                new StateAnimationEvent(0.2f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(true)),
                new StateAnimationEvent(0.8f, () => _player.HeldInventoryItemLogic.HeldModel?.SetTrailEmitting(false))
            };
        }

        public void Tick()
        {
            if (_player.isOwner)
            {
                bool isMoving = _player.InputLogic.MoveDirection != Vector3.zero;
                bool inWater = _player.CharacterPhysicsModule.InWater;
                bool inAir = _player.CharacterPhysicsModule.InAir;
                bool isHoldingItem = _player.Hotbar.SelectedSlot.InventoryItem != null;
                bool inBarrel = _player.RaftPlayerDefeatModule.InBarrel;

                _player.EntityModel.Animator.SetBool(IsMovingBoolName, isMoving);
                _player.EntityModel.Animator.SetBool(InWaterBoolName, inWater);
                _player.EntityModel.Animator.SetBool(InAirBoolName, inAir);
                _player.EntityModel.Animator.SetBool(IsHoldingItemBoolName, isHoldingItem);
                _player.EntityModel.Animator.SetBool(InBarrelBoolName, inBarrel);
            }

            AnimatorStateInfo baseLayerInfo = _player.EntityModel.Animator.GetCurrentAnimatorStateInfo(0);

            _groundRunStateAnimationEvents.Tick(baseLayerInfo);
            _waterSwimStateAnimationEvents.Tick(baseLayerInfo);
            _jumpStateAnimationEvents.Tick(baseLayerInfo);

            AnimatorStateInfo attackLayerInfo = _player.EntityModel.Animator.GetCurrentAnimatorStateInfo(2);

            _paddleSwingStateAnimationEvents.Tick(attackLayerInfo);
            _spearJab1StateAnimationEvents.Tick(attackLayerInfo);
            _spearJab2StateAnimationEvents.Tick(attackLayerInfo);
        }

        public void Jump()
        {
            _player.EntityModel.SetTrigger(JumpTriggerName);
        }
    }
}