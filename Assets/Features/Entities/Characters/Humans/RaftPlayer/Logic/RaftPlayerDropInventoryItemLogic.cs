using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using PurrNet;
using System.Security.Cryptography;
using UnityEngine;

namespace FishFlingers.Entities
{ 
    public class RaftPlayerDropInventoryItemLogic
    {
        private EntityManager _entityManager;
        private CameraManager _cameraManager;

        private RaftPlayer _player;

        private const float Pitch = -45f;
        private const float Strength = 5f;

        public RaftPlayerDropInventoryItemLogic(RaftPlayer player)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _player = player;
        }

        /// <summary>
        /// Spawns a DroppedItem at the player and launches it
        /// </summary>
        public void DropItem(ItemInstance itemInstance)
        {
            Vector3 direction = _player.transform.forward;
            direction.y = 0f;
            direction.Normalize();

            direction = Quaternion.AngleAxis(Pitch, Vector3.Cross(Vector3.up, direction)) * direction;

            SpawnDroppedItemRpc(NetItemInstance.Create(itemInstance), _player.transform.position, direction, Strength);
        }

        [ServerRpc(requireOwnership: false)]
        private static void SpawnDroppedItemRpc(NetItemInstance netItemInstance, Vector3 position, Vector3 direction, float strength)
        {
            EntityManager entityManager = GameManager.Instance.Get<EntityManager>();

            DroppedItem item = (DroppedItem)entityManager.Spawn(EntityId.DroppedItem, new SpawnParams() { Position = position });

            item.Set(netItemInstance, DroppedItemType.Default);

            // Launch the item
            item.Rigidbody.AddForce(direction * strength, ForceMode.Impulse);
        }
    }
}