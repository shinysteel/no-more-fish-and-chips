using PurrNet;
using UnityEngine;
using FishFlingers.Environments;

namespace FishFlingers.Entities
{
    public class DroppedItem : NetEntity, IInteractable
    {
        public DroppedItemData Data => (DroppedItemData)_entityData;

        public Vector3 Position => transform.position;

        public void Interact()
        {
            
        }
    }
}