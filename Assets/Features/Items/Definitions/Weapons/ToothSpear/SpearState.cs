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
        private int _comboCount;
        private bool _canCombo;

        public SpearJabState(StateMachine<ESpearState> parent) : base(parent)
        { }

        public override void Initialise(RaftPlayer player)
        {
            base.Initialise(player);

            _player.AnimateLogic.SpearJab1StateAnimationEvents.Add(new StateAnimationEvent(0.5f, () => _canCombo = true));
            _player.AnimateLogic.SpearJab1StateAnimationEvents.Add(new StateAnimationEvent(1f, () => ResolveJab(0)));
            _player.AnimateLogic.SpearJab2StateAnimationEvents.Add(new StateAnimationEvent(1f, () => ResolveJab(1)));
        }

        public override void Enter()
        {
            _comboCount = 0;
            _canCombo = false;
        }

        private void ResolveJab(int count)
        {
            if (count == _comboCount)
            {
                _parentStateMachine.ChangeState(ESpearState.None);
            }
        }

        public override void Tick()
        {
            base.Tick();

            ComboTick();            
        }

        private void ComboTick()
        {
            if (!_canCombo)
            {
                return;
            }

            if (_player.InputLogic.LeftClickPressed)
            {
                int state = _player.EntityModel.Animator.GetInteger(RaftPlayerAnimateLogic.AttackStateIntName) + 1;
                _player.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.AttackStateIntName, state);

                _comboCount++;
                _canCombo = false;
            }
        }
    }
}