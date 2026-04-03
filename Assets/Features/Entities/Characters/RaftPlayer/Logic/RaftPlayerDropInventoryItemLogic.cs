using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using PurrNet;
using UnityEngine;

namespace FishFlingers.Entities
{ 
    public class RaftPlayerDropInventoryItemLogic
    {
        private EntityManager _entityManager;
        private CameraManager _cameraManager;

        private RaftPlayer _player;

        private const float Pitch = -45f;
        private const float Strength = 3f;

        public RaftPlayerDropInventoryItemLogic(RaftPlayer player)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _player = player;
        }

        /// <summary>
        /// Spawns a DroppedItem at the player and launches it
        /// </summary>
        public void SpawnDroppedItem(ItemInstance itemInstance, bool towardsMouse)
        {
            Vector3 direction = _player.transform.forward;
            direction.y = 0f;
            direction.Normalize();

            if (towardsMouse)
            {
                Ray ray = _cameraManager.MainCamera.ScreenPointToRay(_player.InputLogic.Mouse);
                Plane plane = new Plane(Vector3.up, _player.transform.position);

                if (plane.Raycast(ray, out float distance))
                {
                    direction = (ray.GetPoint(distance) - _player.transform.position).normalized;
                }
            }

            direction = Quaternion.AngleAxis(Pitch, Vector3.Cross(Vector3.up, direction)) * direction;

            SpawnDroppedItemRpc(NetItemInstance.Create(itemInstance), _player.transform.position, direction, Strength);
        }

        [ServerRpc(requireOwnership: false)]
        private static void SpawnDroppedItemRpc(NetItemInstance netItemInstance, Vector3 position, Vector3 direction, float strength)
        {
            EntityManager entityManager = GameManager.Instance.Get<EntityManager>();
            entityManager.SpawnDroppedItem(new SpawnParams() { Position = position }, netItemInstance, direction, strength);
        }
    }
}