using PurrNet;
using UnityEngine;
using FishFlingers.Environments;

namespace FishFlingers.Entities
{
    public class DroppedItem : NetEntity
    {
        public DroppedItemData Data => (DroppedItemData)_entityData;
    }
}