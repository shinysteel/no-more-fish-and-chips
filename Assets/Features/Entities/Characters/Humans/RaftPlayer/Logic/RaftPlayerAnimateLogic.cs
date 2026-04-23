using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using ShinyOwl.Common;

namespace FishFlingers.Entities
{
    public class RaftPlayerAnimateLogic
    {
        private RaftPlayer _player;

        private const string IsMovingBoolName = "IsMoving";
        private const string IsHoldingItemBoolName = "IsHoldingItem";
        private const string IsAttackingBoolName = "IsAttacking";
        private const string AttackTriggerName = "Attack";
        private const string AttackStateName = "Attack";

        private enum Layer
        {
            Base,
            RightArm
        }

        public RaftPlayerAnimateLogic(RaftPlayer player)
        {
            _player = player;
        }

        public void Tick()
        {
            bool isMoving = _player.InputLogic.MoveDirection != Vector3.zero;
            bool isHoldingItem = _player.Hotbar.SelectedSlot.InventoryItem != null;
            
            _player.CharacterModel.Animator.SetBool(IsMovingBoolName, isMoving);
            _player.CharacterModel.Animator.SetBool(IsHoldingItemBoolName, isHoldingItem);
        }

        public async Task AttackAsync(AnimateEvents events)
        {
            _ = events.PlayAsync(_player.CharacterModel.Animator, (int)Layer.Base, AttackStateName);

            _player.CharacterModel.SetTrigger(AttackTriggerName);

            // Mark IsAttacking as true until we are transitioning out of the attack state
            _player.CharacterModel.Animator.SetBool(IsAttackingBoolName, true);
            
            while (!_player.CharacterModel.Animator.GetCurrentAnimatorStateInfo((int)Layer.Base).IsName(AttackStateName))
            {
                await Task.Yield();
            }

            while (_player.CharacterModel.Animator.GetCurrentAnimatorStateInfo((int)Layer.Base).IsName(AttackStateName))
            {
                await Task.Yield();
            }

            _player.CharacterModel.Animator.SetBool(IsAttackingBoolName, false);
        }
    }
}