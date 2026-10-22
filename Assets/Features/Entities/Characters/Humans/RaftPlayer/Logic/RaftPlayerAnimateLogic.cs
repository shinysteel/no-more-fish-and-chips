using NoMoreFishAndChips.Audio;
using ShinyOwl.Common;
using UnityEngine;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerAnimateLogic : RaftPlayerLogic
    {
        private AudioManager _audioManager;

        private AnimatorStateTracker _groundRunAnimatorStateTracker;
        private AnimatorStateTracker _waterIdleAnimatorStateTracker;
        private AnimatorStateTracker _waterSwimAnimatorStateTracker;
        private AnimatorStateTracker _jumpAnimatorStateTracker;
        private AnimatorStateTracker _buildAnimatorStateTracker;
        private AnimatorStateTracker _paddleSwingAnimatorStateTracker;
        private AnimatorStateTracker _spearJab1AnimatorStateTracker;
        private AnimatorStateTracker _spearJab2AnimatorStateTracker;

        public AnimatorStateTracker PaddleSwingAnimatorStateTracker => _paddleSwingAnimatorStateTracker;
        public AnimatorStateTracker SpearJab1AnimatorStateTracker => _spearJab1AnimatorStateTracker;
        public AnimatorStateTracker SpearJab2AnimatorStateTracker => _spearJab2AnimatorStateTracker;

        private const string BaseLayerName = "Base Layer";
        private const string AttackLayerName = "Attack Layer";
        private const string ChannelLayerName = "Channel Layer";

        private const string IsMovingBoolName = "IsMoving";
        private const string InWaterBoolName = "InWater";
        private const string InAirBoolName = "InAir";
        private const string IsHoldingItemBoolName = "IsHoldingItem";
        private const string InBarrelBoolName = "InBarrel";

        public const string AttackWeaponTypeIntName = "AttackWeaponType";
        public const string AttackStateIntName = "AttackState";
        public const string ChannelAnimationIntName = "ChannelAnimation";

        private const string JumpTriggerName = "Jump";

        private const string GroundRunStateName = BaseLayerName + ".Ground.Run";
        private const string WaterIdleStateName = BaseLayerName + ".Water.Idle";
        private const string WaterSwimStateName = BaseLayerName + ".Water.Swim";
        private const string JumpStateName = BaseLayerName + ".Jump";
        private const string PaddleSwingStateName = AttackLayerName + ".Paddle.Swing";
        private const string SpearJab1StateName = AttackLayerName + ".Spear.Jab1";
        private const string SpearJab2StateName = AttackLayerName + ".Spear.Jab2";
        private const string BuildStateName = ChannelLayerName + ".Build";

        public RaftPlayerAnimateLogic(RaftPlayer player) : base(player)
        {
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _groundRunAnimatorStateTracker = new AnimatorStateTracker(GroundRunStateName, true);
            _groundRunAnimatorStateTracker.AddEvent(new StateAnimationEvent(0.1f, PlayFootstepSound));
            _groundRunAnimatorStateTracker.AddEvent(new StateAnimationEvent(0.6f, PlayFootstepSound));

            _waterIdleAnimatorStateTracker = new AnimatorStateTracker(WaterIdleStateName, true);
            _waterIdleAnimatorStateTracker.AddEvent(new StateAnimationEvent(0f, () => _audioManager.PlaySound(SoundId.HumanSwim, _player.transform.position, 0f)));

            _waterSwimAnimatorStateTracker = new AnimatorStateTracker(WaterSwimStateName, true);
            _waterSwimAnimatorStateTracker.AddEvent(new StateAnimationEvent(0.2f, () =>
            {
                if (_player.Hotbar.SelectedSlot.InventoryItem == null)
                {
                    _audioManager.PlaySound(SoundId.HumanSwim, _player.transform.position, 0f);
                }
            }));
            _waterSwimAnimatorStateTracker.AddEvent(new StateAnimationEvent(0.7f, () => _audioManager.PlaySound(SoundId.HumanSwim, _player.transform.position, 0f)));

            _jumpAnimatorStateTracker = new AnimatorStateTracker(JumpStateName, false);
            _jumpAnimatorStateTracker.AddEvent(new StateAnimationEvent(0f, PlayJumpSound));

            _paddleSwingAnimatorStateTracker = new AnimatorStateTracker(PaddleSwingStateName, false);
            _paddleSwingAnimatorStateTracker.AddEvent(new StateAnimationEvent(0f, () =>
            {
                _audioManager.PlaySound(SoundId.PaddleSwing, _player.transform.position, 0f);
                _player.HumanModel.RightArmItemModel?.SetTrailEmitting(true);
            }));
            _paddleSwingAnimatorStateTracker.AddEvent(new StateAnimationEvent(0.66f, () => _player.HumanModel.RightArmItemModel?.SetTrailEmitting(false)));

            _spearJab1AnimatorStateTracker = new AnimatorStateTracker(SpearJab1StateName, false);
            _spearJab1AnimatorStateTracker.AddEvent(new StateAnimationEvent(0.2f, () =>
            {
                _audioManager.PlaySound(SoundId.SpearJab, _player.transform.position, 0f);
                _player.HumanModel.RightArmItemModel?.SetTrailEmitting(true);
            }));
            _spearJab1AnimatorStateTracker.AddEvent(new StateAnimationEvent(0.8f, () => _player.HumanModel.RightArmItemModel?.SetTrailEmitting(false)));

            _spearJab2AnimatorStateTracker = new AnimatorStateTracker(SpearJab2StateName, false);
            _spearJab2AnimatorStateTracker.AddEvent(new StateAnimationEvent(0.2f, () =>
            {
                _audioManager.PlaySound(SoundId.SpearJab, _player.transform.position, 0f);
                _player.HumanModel.RightArmItemModel?.SetTrailEmitting(true);
            }));
            _spearJab2AnimatorStateTracker.AddEvent(new StateAnimationEvent(0.8f, () => _player.HumanModel.RightArmItemModel?.SetTrailEmitting(false)));

            _buildAnimatorStateTracker = new AnimatorStateTracker(BuildStateName, true);
            _buildAnimatorStateTracker.AddEvent(new StateAnimationEvent(0.5f, () => _audioManager.PlaySound(SoundId.HammerBuild, _player.transform.position, 0f)));
        }

        private void PlayJumpSound()
        {
            if (_player.CharacterPhysicsLogic.LastGroundSurface == null)
            {
                return;
            }

            SoundId id = _player.CharacterPhysicsLogic.LastGroundSurface.SurfaceType switch
            {
                SurfaceType.None => SoundId.None,
                SurfaceType.Wood => SoundId.HumanJumpWood,
                SurfaceType.Sand => SoundId.HumanJumpSand,
                SurfaceType.Grass => SoundId.HumanJumpGrass,
                SurfaceType.Goop => SoundId.HumanJumpGoop,
                SurfaceType.Metal => SoundId.HumanJumpMetal,
                _ => SoundId.None
            };

            if (id != SoundId.None)
            {
                _audioManager.PlaySound(id, _player.transform.position, 0f);
            }
        }

        private void PlayFootstepSound()
        {
            if (_player.CharacterPhysicsLogic.LastGroundSurface == null)
            {
                return;
            }

            SoundId id = _player.CharacterPhysicsLogic.LastGroundSurface.SurfaceType switch
            {
                SurfaceType.None => SoundId.None,
                SurfaceType.Wood => SoundId.HumanFootstepWood,
                SurfaceType.Grass => SoundId.HumanFootstepGrass,
                SurfaceType.Sand => SoundId.HumanFootstepSand,
                SurfaceType.Goop => SoundId.HumanFootstepGoop,
                SurfaceType.Metal => SoundId.HumanFootstepMetal,
                _ => SoundId.None
            };

            if (id != SoundId.None)
            {
                _audioManager.PlaySound(id, _player.transform.position, 0f);
            }
        }

        public override void Tick()
        {
            if (_player.isOwner)
            {
                bool isMoving = _player.RaftPlayerActLogic.CanAct && _player.InputLogic.MoveDirection != Vector3.zero;
                bool inWater = _player.CharacterPhysicsLogic.InWater;
                bool inAir = _player.CharacterPhysicsLogic.InAir;
                bool isHoldingItem = _player.Hotbar.SelectedSlot.InventoryItem != null;
                bool inBarrel = _player.RaftPlayerDefeatLogic.InBarrel;

                _player.EntityModel.Animator.SetBool(IsMovingBoolName, isMoving);
                _player.EntityModel.Animator.SetBool(InWaterBoolName, inWater);
                _player.EntityModel.Animator.SetBool(InAirBoolName, inAir);
                _player.EntityModel.Animator.SetBool(IsHoldingItemBoolName, isHoldingItem);
                _player.EntityModel.Animator.SetBool(InBarrelBoolName, inBarrel);
            }

            AnimatorStateInfo baseLayerInfo = _player.EntityModel.Animator.GetCurrentAnimatorStateInfo(0);

            _groundRunAnimatorStateTracker.Tick(baseLayerInfo);
            _waterIdleAnimatorStateTracker.Tick(baseLayerInfo);
            _waterSwimAnimatorStateTracker.Tick(baseLayerInfo);
            _jumpAnimatorStateTracker.Tick(baseLayerInfo);

            AnimatorStateInfo attackLayerInfo = _player.EntityModel.Animator.GetCurrentAnimatorStateInfo(2);

            _paddleSwingAnimatorStateTracker.Tick(attackLayerInfo);
            _spearJab1AnimatorStateTracker.Tick(attackLayerInfo);
            _spearJab2AnimatorStateTracker.Tick(attackLayerInfo);

            AnimatorStateInfo channelLayerInfo = _player.EntityModel.Animator.GetCurrentAnimatorStateInfo(3);

            _buildAnimatorStateTracker.Tick(channelLayerInfo);
        }

        public void Jump()
        {
            _player.EntityModel.SetAnimatorTrigger(JumpTriggerName);
        }
    }
}