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

        private RaftPlayerDropInventoryItemSettings _settings;

        public RaftPlayerDropInventoryItemLogic(RaftPlayer player)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _player = player;

            _settings = _player.DefinitionData.DropInventoryItemSettings;
        }

        /// <summary>
        /// Spawns a DroppedItem at the player and launches it
        /// </summary>
        public void DropItem(ItemInstance itemInstance)
        {
            Vector3 direction = _player.transform.forward;
            direction.y = 0f;
            direction.Normalize();
            
            direction = Quaternion.AngleAxis(_settings.Pitch, Vector3.Cross(Vector3.up, direction)) * direction;

            SpawnDroppedItemRpc(NetItemInstance.Create(itemInstance), _player.transform.position, direction, _settings.Strength);
        }

        [ServerRpc(requireOwnership: false)]
        private static void SpawnDroppedItemRpc(NetItemInstance netItemInstance, Vector3 position, Vector3 direction, float strength)
        {
            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();

            DroppedItem droppedItem = itemManager.SpawnDroppedItem(netItemInstance, DroppedItemType.Default, position);

            // Launch the item
            droppedItem.EntityPhysicsModule.Rigidbody.AddForce(direction * strength, ForceMode.Impulse);
        }
    }
}