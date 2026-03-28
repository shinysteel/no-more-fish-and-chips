
using FishFlingers.Inventories;
using FishFlingers.Networking;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Items
{
    public interface IItemManagerListener
    { }

    public class ItemManager : GameSystem<IItemManagerListener>
    {
        private NetworkManager _networkManager;

        private ItemManagerConfig _config;

        private Dictionary<ItemId, ItemData> _idDataMap = new();

        public override void Initialise(GameManagerConfig config)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _config = config.ItemManagerConfig;

            foreach (ItemData data in _config.ItemDataScanner.GetAssets())
            {
                _idDataMap.Add(data.ItemId, data);
            }

            base.Initialise(config);
        }

        public ItemData GetItemData(ItemId id)
        {
            return _idDataMap[id];
        }
    }
}