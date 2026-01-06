using FishFlingers.Cameras;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayer : NetEntity
    {
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _moveAcceleration = 10f;
        [SerializeField] private float _moveDeceleration = 7.5f;

        [SerializeField] private float _jumpStrength = 3f;
        
        private Vector2 _directionInput;
        private bool _jumpInput;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (!isOwner)
            {
                return;
            }

            _cameraManager.SetMode(new FollowCameraMode(transform, new Vector3(0f, 3f, -5f)));

            // Figure out a better method for players getting a reference to the raft
            // _raft = FindFirstObjectByType<Raft>();

            // Spawn on a random starting tile
            // transform.position = _raft.TryGetRandomTile(out Tile tile) ? _raft.CellToWorldPosition(tile.Cell) : Vector3.zero;

            transform.position = new Vector3(Random.Range(-1, 2), 0f, Random.Range(-1, 2));
        }

        private void Update()
        {
            InputUpdate();
            MovementUpdate();
            JumpUpdate();
        }

        private void InputUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _directionInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

            _jumpInput = Input.GetKeyDown(KeyCode.Space);
        }

        private void MovementUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            Vector3 moveDirection = new Vector3(_directionInput.x, 0f, _directionInput.y);
            Vector3 targetVelocity = moveDirection * _moveSpeed;
            targetVelocity.y = _rigidbody.linearVelocity.y;
            float speed = moveDirection != Vector3.zero ? _moveAcceleration : _moveDeceleration;
            _rigidbody.linearVelocity = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void JumpUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            if (_jumpInput)
            {
                _rigidbody.AddForce(Vector3.up * _jumpStrength, ForceMode.Impulse);
            }
        }
    }
}