using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Items;
using Newtonsoft.Json;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class DroppedItemSave
    {
        [JsonProperty] public DroppedItemType Type { get; private set; }
        [JsonProperty] public string InstanceId { get; private set; }
        [JsonProperty] public ItemId ItemId { get; private set; }
        [JsonProperty] public int Count { get; private set; }

        [JsonProperty] private SimpleVector3 _position = new();

        [JsonIgnore]
        public Vector3 Position
        {
            get => _position.ToVector3();
            set => _position = new SimpleVector3(value);
        }

        private const int Precision = 1;

        public DroppedItemSave()
        { }

        public DroppedItemSave(DroppedItem droppedItem)
        {
            Type = droppedItem.Type;
            InstanceId = droppedItem.NetItemInstance.value.InstanceId;
            ItemId = droppedItem.NetItemInstance.value.ItemId;
            Count = droppedItem.NetItemInstance.value.Count;
            Position = Utils.Math.RoundVector3(droppedItem.transform.position, Precision);
        }
    }
}