using NoMoreFishAndChips.Entities;
using ShinyOwl.Common.Framework;
using UnityEngine;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Items
{
    public enum ESpearState
    {
        None,
        Jab
    }

    public abstract class SpearState : State<ESpearState, ENone>
    {
        protected RaftPlayer _player;

        public SpearState(StateMachine<ESpearState> parent) : base(parent)
        { }

        public virtual void Initialise(RaftPlayer player)
        {
            _player = player;
        }
    }

    public class SpearJabState : SpearState
    {
        public SpearJabState(StateMachine<ESpearState> parent) : base(parent)
        { }

        public override void Initialise(RaftPlayer player)
        {
            base.Initialise(player);

            _player.AnimateLogic.SpearJabStateAnimationEvents.Add(new StateAnimationEvent(1f, () => _parentStateMachine.ChangeState(ESpearState.None)));
        }
    }
}