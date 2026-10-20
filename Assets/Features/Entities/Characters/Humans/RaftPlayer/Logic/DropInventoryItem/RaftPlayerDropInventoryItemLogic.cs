using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{ 
    public class RaftPlayerDropInventoryItemLogic : RaftPlayerLogic
    {
        private RaftPlayerDropInventoryItemSharedSettings _sharedSettings;

        public RaftPlayerDropInventoryItemLogic(RaftPlayer player) : base(player)
        {
            _sharedSettings = _player.DefinitionData.DropInventoryItemSharedSettings;
        }

        /// <summary>
        /// Spawns a DroppedItem at the player and launches it
        /// </summary>
        public void DropItem(ItemInstance itemInstance)
        {
            Vector3 direction = _player.transform.forward;
            direction.y = 0f;
            direction.Normalize();
            
            direction = Quaternion.AngleAxis(_sharedSettings.Pitch, Vector3.Cross(Vector3.up, direction)) * direction;

            SpawnDroppedItemRpc(NetItemInstance.Create(itemInstance), _player.transform.position, direction, _sharedSettings.Strength);

            AudioManager.PlaySoundRpc(SoundId.DroppedItemDrop, _player.transform.position, 0f);
        }

        [ServerRpc(requireOwnership: false)]
        private static void SpawnDroppedItemRpc(NetItemInstance netItemInstance, Vector3 position, Vector3 direction, float strength)
        {
            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();

            DroppedItem droppedItem = itemManager.SpawnDroppedItem(netItemInstance, DroppedItemType.Default, position, false);

            // Launch the item
            droppedItem.EntityPhysicsLogic.Rigidbody.AddForce(direction * strength, ForceMode.Impulse);
        }
    }
}