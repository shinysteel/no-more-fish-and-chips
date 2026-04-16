using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;

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

        public async Task AttackAsync()
        {
            _model.Animator.SetBool(IsAttackingBoolName, true);
            
            while (!_model.Animator.GetCurrentAnimatorStateInfo((int)Layer.Base).IsName(AttackStateName))
            {
                await Task.Yield();
            }

            while (_model.Animator.GetCurrentAnimatorStateInfo((int)Layer.Base).normalizedTime < 0.5f)
            {
                await Task.Yield();
            }

            _player.Rigidbody.AddForce(_player.transform.forward * 2f, ForceMode.Impulse);

            while (!_model.Animator.IsInTransition((int)Layer.Base))
            {
                await Task.Yield();
            }

            _model.Animator.SetBool(IsAttackingBoolName, false);
        }
    }
}