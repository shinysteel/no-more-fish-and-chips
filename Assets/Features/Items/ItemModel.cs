using FishFlingers.Pools;
using UnityEngine;

namespace FishFlingers.Items
{
    public class ItemModel : MonoBehaviour, IPoolable
    {
        [SerializeField] private ItemId _itemId;

        public ItemId ItemId => _itemId;

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}