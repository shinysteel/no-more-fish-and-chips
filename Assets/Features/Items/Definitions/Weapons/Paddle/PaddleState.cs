using NoMoreFishAndChips.Entities;
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
        public PaddleHoldState(StateMachine<EPaddleState> parent) : base(parent)
        { }

        public override void Tick()
        {
            if (_player.InputLogic.LeftClickHeld)
            {
                return;
            }

            _parentStateMachine.ChangeState(EPaddleState.Release);
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
            _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackStateIntName, 1);
        }
    }
}