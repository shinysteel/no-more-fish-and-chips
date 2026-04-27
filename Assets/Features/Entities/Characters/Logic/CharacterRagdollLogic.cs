using PrimeTween;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class CharacterRagdollLogic
    {
        private Character _character;
        private bool _isKinematic;
        private RigidbodyConstraints _rigidbodyConstraints;

        public CharacterRagdollLogic(Character character)
        {
            _character = character;

            _isKinematic = _character.Rigidbody.isKinematic;
            _rigidbodyConstraints = _character.Rigidbody.constraints;
        }

        public void SetEnabled(bool enabled)
        {
            _character.Rigidbody.isKinematic = _isKinematic && !enabled;
            _character.Rigidbody.constraints = enabled ? RigidbodyConstraints.None : _rigidbodyConstraints;

            _character.CharacterModel.Animator.enabled = !enabled;

            if (!enabled)
            {
                Tween.StopAll(_character.gameObject);
            }
        }
    }
}