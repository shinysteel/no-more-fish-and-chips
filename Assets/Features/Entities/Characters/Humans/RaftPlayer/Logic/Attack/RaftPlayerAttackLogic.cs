using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using ShinyOwl.Common.Framework;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerAttackLogic
    {
        private HitboxManager _hitboxManager;

        private RaftPlayer _player;

        private RaftPlayerAttackSettings _settings;

        private IStateMachine _currentStateMachine;
        private StateMachine<EPaddleState> _paddleStateMachine;

        public RaftPlayerAttackLogic(RaftPlayer player)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();

            _player = player;

            _settings = _player.DefinitionData.AttackSettings;

            _player.AnimateLogic.PaddleReleaseStateAnimationEvents.Add(
                new StateAnimationEvent(0f, () => _player.EntityPhysicsModule.Rigidbody.AddForce(_player.transform.forward * _settings.PaddleLungeStrength, ForceMode.Impulse)));

            _player.AnimateLogic.PaddleReleaseStateAnimationEvents.Add(
                new StateAnimationEvent(0f, () => _hitboxManager.SpawnHitbox(_settings.PaddleSwingHitboxData, new SpawnParams() { Position = _player.transform.position, Rotation = _player.transform.rotation })));

            _paddleStateMachine = new();

            PaddleHoldState paddleHoldState = new PaddleHoldState(_paddleStateMachine);
            PaddleReleaseState paddleReleaseState = new PaddleReleaseState(_paddleStateMachine);

            paddleHoldState.Initialise(_player);
            paddleReleaseState.Initialise(_player);

            _paddleStateMachine.AddState(EPaddleState.Hold, paddleHoldState);
            _paddleStateMachine.AddState(EPaddleState.Release, paddleReleaseState);

            _paddleStateMachine.OnStateChanged += HandlePaddleStateChanged;
        }

        ~RaftPlayerAttackLogic()
        {
            _paddleStateMachine.OnStateChanged -= HandlePaddleStateChanged;
        }

        public void Attack()
        {
            if (_currentStateMachine != null)
            {
                return;
            }

            if (_player.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data is not WeaponDefinitionData weaponData)
            {
                return;
            }

            WeaponType type = weaponData.WeaponType;

            if (type == WeaponType.None)
            {
                Log.Error("Tried to attack using a weapon with an invalid type");
                return;
            }

            _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackWeaponTypeIntName, (int)type);

            if (type == WeaponType.Paddle)
            {
                _currentStateMachine = _paddleStateMachine;
                _paddleStateMachine.ChangeState(EPaddleState.Hold);
            }
            else if (type == WeaponType.Spear)
            {

            }
            else if (type == WeaponType.Slingshot)
            {

            }
        }

        public void Tick()
        {
            _currentStateMachine?.Tick();
        }

        private void HandlePaddleStateChanged(EPaddleState previous, EPaddleState current)
        {
            if (current == EPaddleState.None)
            {
                _currentStateMachine = null;
                _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackWeaponTypeIntName, 0);
                _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackStateIntName, 0);
            }
        }
    }
}