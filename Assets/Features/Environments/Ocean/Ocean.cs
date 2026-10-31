using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using UnityEngine;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.Environments
{
    public class Ocean : GameplayBehaviour, IPathFindable
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private BoxCollider _boxCollider;

        [SerializeField] private OceanWaveData[] _waveDatas;
        [SerializeField, Range(0f, 1f)] private float _submergePercent = 0.5f;
        [SerializeField] private Vector3 _linearDrag = new Vector3(5f, 1f, 5f);
        [SerializeField] private float _angularDrag = 2.5f;
        [SerializeField] private Vector2 _currentDirection = Vector2.down;
        [SerializeField] private float _currentOnSpeed = 1f;
        [SerializeField] private float _vertexTimeMultiplier = 0.1f;

        private NetworkManager _networkManager;

        private Material _material;

        private SyncVar<float> _netCurrentTime = new SyncVar<float>(ownerAuth: true);
        private float _currentSpeed;

        private Tween _speedTween;

        private const string OceanTimeName = "_OceanTime";
        private const string CurrentTimeName = "_CurrentTime";
        private const string HorizonColorName = "_HorizonColor";
        private const string HorizonEmissionName = "_HorizonEmission";

        public const float SetCurrentDuration = 2.5f;

        bool IPathFindable.IsTraversable(Vector2Int cell)
        {
            Vector2Int tileCell = _context.Raft.Queries.StructureCellToTileCell(cell);

            if (_context.Raft.Tiles.ContainsKey(tileCell))
            {
                return false;
            }

            return !_context.Raft.Structures.ContainsKey(cell);
        }

        protected override void Awake()
        {
            base.Awake();

            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _material = _meshRenderer.material;

            _currentSpeed = _currentOnSpeed;
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleNetCurrentTimeChanged(_netCurrentTime.value);

            _netCurrentTime.onChanged += HandleNetCurrentTimeChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netCurrentTime.onChanged -= HandleNetCurrentTimeChanged;
        }

        private void HandleNetCurrentTimeChanged(float time)
        {
            _material.SetFloat(CurrentTimeName, time);
        }

        private void Update()
        {
            _material.SetFloat(OceanTimeName, isFullySpawned ? _networkManager.ServerTime : Time.time);

            if (isOwner && isFullySpawned)
            {
                // Current speed influences how fast 'time' increases. The 2f is here so that to keep the visual fast without affecting entities
                _netCurrentTime.value += _currentSpeed * 2f * Time.deltaTime;
            }
        }

        // Adjusts current speed to what it should be over a duration
        public void SetCurrent(bool on, float duration)        
        {
            float startCurrentSpeed = _currentSpeed;
            float endCurrentSpeed = on ? _currentOnSpeed : 0f;

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

        public float SampleHeight(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);
            Vector2 targetPosition = new Vector2(localPosition.x, localPosition.z);

            targetPosition -= _netCurrentTime.value * _vertexTimeMultiplier * _currentDirection;

            // Since Gerstner waves offset vertices horizontally, we need to use fixed-point iteration to approximate the original position
            Vector2 originalPosition = targetPosition;
            int iterations = 3;
            float oceanTime = isFullySpawned ? _networkManager.ServerTime : Time.time;

            for (int i = 0; i < iterations; i++)
            {
                Vector2 offset = Vector2.zero;

                foreach (OceanWaveData data in _waveDatas)
                {
                    float number = 2f * Mathf.PI / data.Length;
                    float phase = Vector2.Dot(originalPosition, data.Direction) * number - oceanTime * data.Speed;
                    offset += data.Direction * data.Steepness * data.Amplitude * Mathf.Cos(phase);
                }

                originalPosition = targetPosition - offset;
            }

            float height = transform.position.y;

            foreach (OceanWaveData data in _waveDatas)
            {
                float number = 2f * Mathf.PI / data.Length;
                float phase = Vector2.Dot(originalPosition, data.Direction) * number - oceanTime * data.Speed;
                height += data.Amplitude * Mathf.Sin(phase) * transform.lossyScale.y;
            }

            return height;
        }

        public bool InWater(Collider collider)
        {
            if (!Physics.ComputePenetration(_boxCollider, _boxCollider.transform.position, _boxCollider.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out _, out _))
            {
                return false;
            }

            return collider.bounds.min.y <= SampleHeight(collider.transform.position);
        }

        private void OnTriggerStay(Collider collider)
        {
            if (Utils.Physics.ColliderTryGetComponent(collider, out IFloatable floatable))
            {
                if (!floatable.CanFloat())
                {
                    return;
                }

                if (floatable is Island)
                {
                    CurrentOnTriggerStay(floatable, true);
                }
                else
                {
                    float height = SampleHeight(collider.transform.position);

                    if (collider.bounds.min.y > height)
                    {
                        return;
                    }

                    BuoyancyOnTriggerStay(collider, height, floatable.Rigidbody);
                    CurrentOnTriggerStay(floatable, false);
                    DragOnTriggerStay(collider, floatable.Rigidbody);
                }
            }
        }

        private float GetBuoyancyFactor(Collider collider, float height)
        {
            float depth = height - collider.bounds.min.y;
            return Mathf.Clamp01(depth / collider.bounds.size.y);
        }

        private void BuoyancyOnTriggerStay(Collider collider, float height, Rigidbody rigidbody)
        {
            // More mass = more force
            float strength = rigidbody.mass * Physics.gravity.magnitude / _submergePercent;
            float factor = GetBuoyancyFactor(collider, height);
            Vector3 force = Vector3.up * strength * factor;

            // Push the entity upwards to simulate floating
            rigidbody.AddForce(force, ForceMode.Force);
        }

        // Current is referring to motion in water
        private void CurrentOnTriggerStay(IFloatable floatable, bool allowKinematic)
        {
            if (floatable is Character character && !character.EntityDefeatLogic.IsDefeated)
            {
                return;
            }

            if (_currentSpeed == 0f)
            {
                return;
            }

            if (floatable.Rigidbody.isKinematic && !allowKinematic)
            {
                return;
            }

            floatable.Rigidbody.MovePosition(floatable.Rigidbody.position + Vector3.back * _currentSpeed * Time.fixedDeltaTime);
        }

        private void DragOnTriggerStay(Collider collider, Rigidbody rigidbody)
        {
            // Drag stops the entity being 'launched' from buoyancy, and slows it down on the XZ plane
            rigidbody.AddForce(Vector3.Scale(-rigidbody.linearVelocity, _linearDrag), ForceMode.Acceleration);
            rigidbody.AddTorque(-rigidbody.angularVelocity * _angularDrag, ForceMode.Acceleration);
        }

        public void SetHorizon(Color color, float emission)
        {
            _material.SetColor(HorizonColorName, color);
            _material.SetFloat(HorizonEmissionName, emission);
        }
    }
}