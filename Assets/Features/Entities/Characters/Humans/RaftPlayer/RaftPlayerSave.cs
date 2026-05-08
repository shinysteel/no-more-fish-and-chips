using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using Newtonsoft.Json;
using ShinyOwl.Common.Utils;
using UnityEngine;
using System.Threading.Tasks;
using ShinyOwl.Common;

namespace FishFlingers.Entities
{
    public class RaftPlayerSave
    {
        [JsonProperty] private SimpleVector3 _position = new();
        [JsonProperty] private SimpleQuaternion _rotation = new();

        [JsonIgnore]
        public Vector3 Position
        {
            get => _position.ToVector3();
            set => _position = new SimpleVector3(value);
        }

        [JsonIgnore]
        public Quaternion Rotation
        {
            get => _rotation.ToQuaternion();
            set => _rotation = new SimpleQuaternion(value);
        }

        [JsonProperty] public InventorySave Inventory { get; private set; } = new();
        [JsonProperty] public HotbarSave Hotbar { get; private set; } = new();

        private const int Precision = 1;

        public RaftPlayerSave()
        { }

        public async Task LoadToAsync(RaftPlayer player)
        {
            player.RaftPlayerPhysicsModule.Rigidbody.position = Position;
            player.RaftPlayerPhysicsModule.Rigidbody.rotation = Rotation.normalized;

            player.RaftPlayerPhysicsModule.Rigidbody.linearVelocity = Vector3.zero;
            player.RaftPlayerPhysicsModule.Rigidbody.angularVelocity = Vector3.zero;

            await Inventory.LoadToAsync(player.Inventory);

            Hotbar.LoadTo(player.Hotbar);
        }

        public void SaveFrom(RaftPlayer player)
        {
            Position = Utils.Math.RoundVector3(player.RaftPlayerPhysicsModule.Rigidbody.position, Precision);
            Rotation = Utils.Math.RoundQuaternion(player.RaftPlayerPhysicsModule.Rigidbody.rotation, Precision);

            Inventory.SaveFrom(player.Inventory);

            Hotbar.SaveFrom(player.Hotbar);
        }

        public void ApplyDefaults()
        {
            // Start the game facing the camera
            Rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);

            Inventory.Items.Add(new InventoryItemSave(Vector2Int.zero, Vector2Int.zero, 0, null, ItemId.Paddle, 1));
            Inventory.Items.Add(new InventoryItemSave(new Vector2Int(2, 0), Vector2Int.zero, 0, null, ItemId.Hammer, 1));
        }
    }
}