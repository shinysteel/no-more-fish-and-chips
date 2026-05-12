using FishFlingers.Entities;
using FishFlingers.Networking;
using UnityEngine;

namespace FishFlingers.Environments
{
    public class Ocean : MonoBehaviour
    {
        private NetworkManager _networkManager;

        [SerializeField] private BoxCollider _boxCollider;

        [SerializeField, Range(0f, 1f)] private float _submergePercent = 0.5f;
        [SerializeField] private Vector3 _linearDrag = new Vector3(5f, 1f, 5f);
        [SerializeField] private float _angularDrag = 2.5f;
        [SerializeField] private float _currentSpeed = 0.25f;

        private void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        private void OnTriggerStay(Collider collider)
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (!collider.gameObject.TryGetComponent(out IEntity entity))
            {
                return;
            }

            if (!entity.IsSpawned)
            {
                return;
            }
            
            BuoyancyOnTriggerStay(collider, entity);
            CurrentOnTriggerStay(entity);
            DragOnTriggerStay(collider, entity);
        }

        private float GetBuoyancyFactor(Collider collider)
        {
            float surfaceY = _boxCollider.bounds.max.y;
            float depth = surfaceY - collider.bounds.min.y;
            return Mathf.Clamp01(depth / collider.bounds.size.y);
        }

        private void BuoyancyOnTriggerStay(Collider collider, IEntity entity)
        { 
            // More mass = more force
            float strength = entity.EntityPhysicsModule.Rigidbody.mass * Physics.gravity.magnitude / _submergePercent;
            float factor = GetBuoyancyFactor(collider);
            Vector3 force = Vector3.up * strength * factor;

            // Push the entity upwards to simulate floating
            entity.EntityPhysicsModule.Rigidbody.AddForce(force, ForceMode.Force);
        }

        // Current is referring to motion in water
        private void CurrentOnTriggerStay(IEntity entity)
        {
            if (!entity.EntityPhysicsModule.Rigidbody.isKinematic)
            {
                entity.EntityPhysicsModule.Rigidbody.MovePosition(entity.EntityPhysicsModule.Rigidbody.position + Vector3.back * _currentSpeed * Time.fixedDeltaTime);
            }
        }

        private void DragOnTriggerStay(Collider collider, IEntity entity)
        {
            // Drag stops the entity being 'launched' from buoyancy, and
            // slows it down on the XZ plane
            entity.EntityPhysicsModule.Rigidbody.AddForce(Vector3.Scale(-entity.EntityPhysicsModule.Rigidbody.linearVelocity, _linearDrag), ForceMode.Acceleration);
            entity.EntityPhysicsModule.Rigidbody.AddTorque(-entity.EntityPhysicsModule.Rigidbody.angularVelocity * _angularDrag, ForceMode.Acceleration);
        }
    }
}