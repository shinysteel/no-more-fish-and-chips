using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;

namespace FishFlingers.Entities
{
    public class RaftPlayerAnimateLogic
    {
        private RaftPlayer _player;
        private CharacterModel _model;

        private const string IsMovingBoolName = "IsMoving";
        private const string IsHoldingItemBoolName = "IsHoldingItem";
        private const string IsAttackingBoolName = "IsAttacking";
        private const string AttackStateName = "Attack";

        private enum Layer
        {
            Base,
            RightArm
        }

        public RaftPlayerAnimateLogic(RaftPlayer player, CharacterModel model)
        {
            _player = player;
            _model = model;
        }

        public void Tick()
        {
            bool isMoving = _player.InputLogic.MoveDirection != Vector3.zero;
            bool isHoldingItem = _player.Hotbar.SelectedSlot.InventoryItem != null;
            
            _model.Animator.SetBool(IsMovingBoolName, isMoving);
            _model.Animator.SetBool(IsHoldingItemBoolName, isHoldingItem);
        }

        public async Task AttackAsync(AnimateEvents events)
        {
            _ = events.PlayAsync(_model.Animator, (int)Layer.Base, AttackStateName);
            
            // Mark IsAttacking as true until we are transitioning out of the attack state
            _model.Animator.SetBool(IsAttackingBoolName, true);
            
            while (!_model.Animator.GetCurrentAnimatorStateInfo((int)Layer.Base).IsName(AttackStateName))
            {
                await Task.Yield();
            }

            while (_model.Animator.GetCurrentAnimatorStateInfo((int)Layer.Base).IsName(AttackStateName))
            {
                await Task.Yield();
            }

            _model.Animator.SetBool(IsAttackingBoolName, false);
        }
    }
}