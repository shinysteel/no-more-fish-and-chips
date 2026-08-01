using PrimeTween;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterRagdollLogic : CharacterLogic
    {
        private bool _isKinematic;
        private RigidbodyConstraints _rigidbodyConstraints;

        public CharacterRagdollLogic(Character character) : base(character)
        {
            _isKinematic = _character.EntityPhysicsLogic.Rigidbody.isKinematic;
            _rigidbodyConstraints = _character.EntityPhysicsLogic.Rigidbody.constraints;
        }

        public void SetEnabled(bool enabled)
        {
            _character.EntityModel.Animator.enabled = !enabled;

            if (!_character.isOwner)
            {
                return;
            }

            _character.EntityPhysicsLogic.Rigidbody.isKinematic = _isKinematic && !enabled;
            _character.EntityPhysicsLogic.Rigidbody.constraints = enabled ? RigidbodyConstraints.None : _rigidbodyConstraints;

            if (!enabled)
            {
                Tween.StopAll(_character.gameObject);
            }
        }
    }
}