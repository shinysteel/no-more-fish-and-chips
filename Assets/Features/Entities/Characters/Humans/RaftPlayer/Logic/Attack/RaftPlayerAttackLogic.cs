using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System.Linq;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerAttackLogic
    {
        private HitboxManager _hitboxManager;

        private RaftPlayer _player;

        private RaftPlayerAttackSettings _settings;

        private IStateMachine _currentStateMachine;

        private StateMachine<EPaddleState> _paddleStateMachine;
        private StateMachine<ESpearState> _spearStateMachine;

        public RaftPlayerAttackLogic(RaftPlayer player)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();

            _player = player;

            _settings = _player.DefinitionData.AttackSettings;

            _player.AnimateLogic.PaddleSwingStateAnimationEvents.Add(
                new StateAnimationEvent(0f, () => _player.EntityPhysicsModule.Rigidbody.AddForce(_player.transform.forward * _settings.PaddleLungeStrength, ForceMode.Impulse)));

            _player.AnimateLogic.PaddleSwingStateAnimationEvents.Add(
                new StateAnimationEvent(0f, () => _hitboxManager.SpawnHitbox(_settings.PaddleSwingHitboxData, new SpawnParams() { Position = _player.transform.position, Rotation = _player.transform.rotation })));

            _player.AnimateLogic.SpearJabStateAnimationEvents.Add(
                new StateAnimationEvent(0.2f, () => _player.EntityPhysicsModule.Rigidbody.AddForce(_player.transform.forward * _settings.SpearLungeStrength, ForceMode.Impulse)));

            _player.AnimateLogic.SpearJabStateAnimationEvents.Add(
                new StateAnimationEvent(0.4f, () => _hitboxManager.SpawnHitbox(_settings.SpearJabHitboxData, new SpawnParams() { Position = _player.transform.position, Rotation = _player.transform.rotation })));

            _paddleStateMachine = new();

            PaddleHoldState paddleHoldState = new PaddleHoldState(_paddleStateMachine);
            PaddleReleaseState paddleReleaseState = new PaddleReleaseState(_paddleStateMachine);

            paddleHoldState.Initialise(_player);
            paddleReleaseState.Initialise(_player);

            _paddleStateMachine.AddState(EPaddleState.Hold, paddleHoldState);
            _paddleStateMachine.AddState(EPaddleState.Release, paddleReleaseState);

            _spearStateMachine = new();

            SpearJabState spearJabState = new SpearJabState(_spearStateMachine);

            spearJabState.Initialise(_player);

            _spearStateMachine.AddState(ESpearState.Jab, spearJabState);

            _paddleStateMachine.OnStateChanged += HandlePaddleStateChanged;
            _spearStateMachine.OnStateChanged += HandleSpearStateChanged;
        }

        ~RaftPlayerAttackLogic()
        {
            _paddleStateMachine.OnStateChanged -= HandlePaddleStateChanged;
            _spearStateMachine.OnStateChanged -= HandleSpearStateChanged;
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
                _currentStateMachine = _spearStateMachine;
                _spearStateMachine.ChangeState(ESpearState.Jab);
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
                Reset();
            }
        }

        private void HandleSpearStateChanged(ESpearState previous, ESpearState current)
        {
            if (current == ESpearState.None)
            {
                Reset();
            }
        }

        private void Reset()
        {
            _currentStateMachine = null;

            _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackWeaponTypeIntName, 0);
            _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackStateIntName, 0);
        }
    }
}