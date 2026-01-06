using FishFlingers.Entities;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Environments
{
    public class Ocean : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;

        [SerializeField] private float _buoyancyStrength = 30f;

        [SerializeField] private float _currentSpeed = 0.5f;

        private void OnTriggerStay(Collider other)
        {
            if (!other.gameObject.TryGetComponent(out IEntity entity))
            {
                return;
            }

            float surfaceY = _boxCollider.bounds.max.y;
            float depth = surfaceY - other.bounds.min.y;
            Vector3 buoyancyForce = Vector3.up * _buoyancyStrength * depth;

            // Push the entity upwards to simulate floating
            entity.Rigidbody.AddForce(buoyancyForce);

            // Push the entity back to fake that the raft is moving forward
            if (!entity.Rigidbody.isKinematic)
            {
                entity.Rigidbody.MovePosition(entity.Rigidbody.position + Vector3.back * _currentSpeed * Time.fixedDeltaTime);
            }
        }
    }
}