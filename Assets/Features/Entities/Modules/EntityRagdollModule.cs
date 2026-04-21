using PrimeTween;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class EntityRagdollModule
    {
        private Rigidbody _rigidbody;
        private RigidbodyConstraints _constraints;

        public Rigidbody Rigidbody => _rigidbody;

        public EntityRagdollModule(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
            _constraints = _rigidbody.constraints;
        }

        public void SetEnabled(bool enabled)
        {
            _rigidbody.isKinematic = !enabled;
            _rigidbody.constraints = enabled ? RigidbodyConstraints.None : _constraints;

            if (!enabled)
            {
                Tween.StopAll(_rigidbody.transform);
            }
        }
    }
}