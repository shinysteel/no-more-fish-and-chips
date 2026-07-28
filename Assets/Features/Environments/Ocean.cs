using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Ocean : MonoBehaviour
    {
        private StateManager _stateManager;

        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private BoxCollider _boxCollider;

        [SerializeField, Range(0f, 1f)] private float _submergePercent = 0.5f;
        [SerializeField] private Vector3 _linearDrag = new Vector3(5f, 1f, 5f);
        [SerializeField] private float _angularDrag = 2.5f;
        [SerializeField] private float _currentSpeed = 0.5f;
        
        private Material _material;

        private float _defaultCurrentSpeed;

        private Tween _speedTween;

        private const string FoamTileTimeName = "_FoamTileTime";

        public const float DefaultSetCurrentDuration = 2.5f;

        private void Awake()
        {
            _stateManager = GameManager.Instance.Get<StateManager>();

            _material = _meshRenderer.material;

            _defaultCurrentSpeed = _currentSpeed;
        }
        
        private void Update()
        {
            FoamTileTimeUpdate();
        }

        private void FoamTileTimeUpdate()
        {
            // Current speed influences how fast 'time' increases
            float time = _material.GetFloat(FoamTileTimeName);
            time += _currentSpeed * 2f * Time.deltaTime;
            _material.SetFloat(FoamTileTimeName, time);
        }

        // Adjusts current speed to what it should be over a duration
        public void SetCurrent(bool on, float duration)        
        {
            float startCurrentSpeed = _currentSpeed;
            float endCurrentSpeed = on ? _defaultCurrentSpeed : 0f;

            if (startCurrentSpeed == endCurrentSpeed)
            {
                return;
            }

            if (_speedTween.isAlive)
            {
                _speedTween.Stop();
            }

            _speedTween = Tween.Custom(startValue: startCurrentSpeed, endValue: endCurrentSpeed, duration: duration, ease: Ease.Linear, onValueChange: (float value) => _currentSpeed = value);
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.TryGetComponent(out Entity entity))
            {
                if (!entity.isSpawned)
                {
                    return;
                }

                if (!entity.isOwner)
                {
                    return;
                }

                BuoyancyOnTriggerStay(collider, entity);
                CurrentOnTriggerStay(entity.EntityPhysicsLogic.NetworkRigidbody, false);
                DragOnTriggerStay(collider, entity);
            }
            else if (collider.gameObject.TryGetComponent(out Island island))
            {
                if (!island.isSpawned)
                {
                    return;
                }

                if (!island.isOwner)
                {
                    return;
                }
                
                CurrentOnTriggerStay(island.NetworkRigidbody, true);
            }
        }

        private float GetBuoyancyFactor(Collider collider)
        {
            float surfaceY = _boxCollider.bounds.max.y;
            float depth = surfaceY - collider.bounds.min.y;
            return Mathf.Clamp01(depth / collider.bounds.size.y);
        }

        private void BuoyancyOnTriggerStay(Collider collider, Entity entity)
        { 
            // More mass = more force
            float strength = entity.EntityPhysicsLogic.NetworkRigidbody.mass * Physics.gravity.magnitude / _submergePercent;
            float factor = GetBuoyancyFactor(collider);
            Vector3 force = Vector3.up * strength * factor;

            // Push the entity upwards to simulate floating
            entity.EntityPhysicsLogic.NetworkRigidbody.AddForce(force, ForceMode.Force);
        }

        // Current is referring to motion in water
        private void CurrentOnTriggerStay(NetworkRigidbody rigidbody, bool allowKinematic)
        {
            if (_currentSpeed == 0f)
            {
                return;
            }

            if (!allowKinematic && rigidbody.isKinematic)
            {
                return;
            }

            rigidbody.MovePosition(rigidbody.position + Vector3.back * _currentSpeed * Time.fixedDeltaTime);
        }

        private void DragOnTriggerStay(Collider collider, Entity entity)
        {
            // Drag stops the entity being 'launched' from buoyancy, and slows it down on the XZ plane
            entity.EntityPhysicsLogic.NetworkRigidbody.AddForce(Vector3.Scale(-entity.EntityPhysicsLogic.NetworkRigidbody.linearVelocity, _linearDrag), ForceMode.Acceleration);
            entity.EntityPhysicsLogic.NetworkRigidbody.AddTorque(-entity.EntityPhysicsLogic.NetworkRigidbody.angularVelocity * _angularDrag, ForceMode.Acceleration);
        }
    }
}