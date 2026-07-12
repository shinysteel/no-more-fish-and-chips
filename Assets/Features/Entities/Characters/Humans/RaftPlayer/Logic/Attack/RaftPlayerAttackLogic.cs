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
    public enum EPaddleAttackState
    {
        None,
        Hold,
        Release
    }

    public class PaddleAttackState : State<EPaddleAttackState, ENone>
    {
        protected RaftPlayerAttackLogic _logic;
        protected RaftPlayer _player;

        public PaddleAttackState(StateMachine<EPaddleAttackState> parent) : base(parent)
        { }

        public virtual void Initialise(RaftPlayerAttackLogic logic, RaftPlayer player)
        {
            _logic = logic;
            _player = player;
        }
    }

    public class PaddleAttackHoldState : PaddleAttackState
    {
        public PaddleAttackHoldState(StateMachine<EPaddleAttackState> parent) : base(parent)
        { }

        public override void Tick()
        {
            if (_player.InputLogic.LeftClickHeld)
            {
                return;
            }

            _parentStateMachine.ChangeState(EPaddleAttackState.Release);
        }
    }

    public class PaddleAttackReleaseState : PaddleAttackState
    {
        public PaddleAttackReleaseState(StateMachine<EPaddleAttackState> parent) : base(parent)
        { }

        public override void Initialise(RaftPlayerAttackLogic logic, RaftPlayer player)
        {
            base.Initialise(logic, player);

            _player.AnimateLogic.PaddleAttackReleaseStateAnimationEvents.Add(new StateAnimationEvent(1f, () => _parentStateMachine.ChangeState(EPaddleAttackState.None)));
        }

        public override void Enter()
        {
            _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackStateIntName, 1);
        }
    }

    public class RaftPlayerAttackLogic
    {
        private HitboxManager _hitboxManager;

        private RaftPlayer _player;

        private RaftPlayerAttackSettings _settings;

        private IStateMachine _currentStateMachine;
        private StateMachine<EPaddleAttackState> _paddleAttackStateMachine;

        public RaftPlayerAttackLogic(RaftPlayer player)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();

            _player = player;

            _settings = _player.DefinitionData.AttackSettings;

            _paddleAttackStateMachine = new();

            PaddleAttackHoldState holdState = new PaddleAttackHoldState(_paddleAttackStateMachine);
            PaddleAttackReleaseState releaseState = new PaddleAttackReleaseState(_paddleAttackStateMachine);

            holdState.Initialise(this, _player);
            releaseState.Initialise(this, _player);

            _paddleAttackStateMachine.AddState(EPaddleAttackState.Hold, holdState);
            _paddleAttackStateMachine.AddState(EPaddleAttackState.Release, releaseState);

            _paddleAttackStateMachine.OnStateChanged += HandlePaddleAttackStateChanged;
        }

        ~RaftPlayerAttackLogic()
        {
            _paddleAttackStateMachine.OnStateChanged -= HandlePaddleAttackStateChanged;
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
                _currentStateMachine = _paddleAttackStateMachine;
                _paddleAttackStateMachine.ChangeState(EPaddleAttackState.Hold);
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

        private void HandlePaddleAttackStateChanged(EPaddleAttackState previous, EPaddleAttackState current)
        {
            if (current == EPaddleAttackState.None)
            {
                _currentStateMachine = null;
                _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackWeaponTypeIntName, 0);
                _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackStateIntName, 0);
            }
        }

        // attack state integer value

        // play hold animation 
        // add speed modifier
        // show progress bar filling up
        // player looks towards cursor
        // attack key is released
        // play release animation

        //_player.EntityPhysicsModule.Rigidbody.AddForce(_player.transform.forward * _settings.LungeStrength, ForceMode.Impulse);
        //_hitboxManager.SpawnHitbox(_settings.HitboxData, new SpawnParams() { Position = _player.transform.position, Rotation = _player.transform.rotation });
    }
}