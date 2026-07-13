using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    public enum EPaddleState
    {
        None,
        Hold,
        Release
    }

    public class PaddleState : State<EPaddleState, ENone>
    {
        protected RaftPlayer _player;

        public PaddleState(StateMachine<EPaddleState> parent) : base(parent)
        { }

        public virtual void Initialise(RaftPlayer player)
        {
            _player = player;
        }
    }

    public class PaddleHoldState : PaddleState
    {
        private UIManager _uiManager;

        private ProgressBarUI _progressBarUI;

        private float _chargeTimer;

        private const float ChargeDelay = 0.1f;
        private const float ChargeDuration = 0.4f;

        public PaddleHoldState(StateMachine<EPaddleState> parent) : base(parent)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
        }

        public override void Enter()
        {
            base.Enter();

            _chargeTimer = 0f;
        }

        public override void Tick() 
        {
            base.Tick();

            if (_player.InputLogic.LeftClickHeld)
            {
                ProgressTick();
            }
            else
            {
                _parentStateMachine.ChangeState(EPaddleState.Release);
            }
        }

        private void ProgressTick()
        {
            if (_stateTimer < ChargeDelay)
            {
                return;
            }

            if (_progressBarUI == null)
            {
                _progressBarUI = _uiManager.CreateWorldUI(_uiManager.Config.ProgressBarUIPrefab, Vector3.zero);

                _chargeTimer = _stateTimer - ChargeDelay;
            }
            else
            {
                _chargeTimer += Time.deltaTime;
            }

            float amount = Mathf.Min(_chargeTimer / ChargeDuration, 1f);
            _progressBarUI.SetFillAmount(amount);

            _progressBarUI.transform.position = _player.transform.position + Vector3.up * 0.75f;
        }

        public override void Exit()
        {
            base.Exit();

            if (_progressBarUI != null)
            {
                _uiManager.DestroyWorldUI(_progressBarUI);
                _progressBarUI = null;
            }
        }
    }

    public class PaddleReleaseState : PaddleState
    {
        public PaddleReleaseState(StateMachine<EPaddleState> parent) : base(parent)
        { }

        public override void Initialise(RaftPlayer player)
        {
            base.Initialise(player);

            _player.AnimateLogic.PaddleReleaseStateAnimationEvents.Add(new StateAnimationEvent(1f, () => _parentStateMachine.ChangeState(EPaddleState.None)));
        }

        public override void Enter()
        {
            base.Enter();

            _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackStateIntName, 1);
        }
    }
}