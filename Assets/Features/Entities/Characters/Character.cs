using UnityEngine;
using PurrNet;

namespace FishFlingers.Entities
{
    public class Character : NetworkBehaviour
    {
        [SerializeField] private float _moveSpeed = 2.5f;

        private void Update()
        {
            MovementUpdate();
        }

        private void MovementUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            transform.position += direction * _moveSpeed * Time.deltaTime;
        }
    }
}