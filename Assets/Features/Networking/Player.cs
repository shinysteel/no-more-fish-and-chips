using FishFlingers.Cameras;
using ShinyOwl.Common;
using UnityEngine;
using FishFlingers.Networking;
using PurrNet;
using PurrNet.Prediction;
using FishFlingers.Scenes;
using System.Threading.Tasks;
using System;
using PurrNet.Packing;

namespace FishFlingers.Networking
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;

        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _moveAcceleration = 10f;
        [SerializeField] private float _moveDeceleration = 7.5f;

        [SerializeField] private float _jumpForce = 3f;
        
        private CameraManager _cameraManager;

        private Vector2 _directionInput;
        private bool _jumpInput;

        protected override void OnInitializeModules()
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
        }

        protected override void OnSpawned()
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            if (isOwner)
            {
                _cameraManager.SetMode(new FollowCameraMode(transform, new Vector3(0f, 3f, -5f)));
            }
        }

        private void Update()
        {
            InputUpdate();
            MovementUpdate();
            JumpUpdate();
        }

        private void InputUpdate()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _directionInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

            _jumpInput = Input.GetKeyDown(KeyCode.Space);
        }

        private void MovementUpdate()
        {
            Vector3 moveDirection = new Vector3(_directionInput.x, 0f, _directionInput.y);
            Vector3 targetVelocity = moveDirection * _moveSpeed;
            targetVelocity.y = _rigidbody.linearVelocity.y;
            float speed = moveDirection != Vector3.zero ? _moveAcceleration : _moveDeceleration;
            _rigidbody.linearVelocity = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void JumpUpdate()
        {
            if (_jumpInput)
            {
                _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            }
        }
    }
}