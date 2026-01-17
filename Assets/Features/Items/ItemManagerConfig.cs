using FishFlingers.Inventories;
using UnityEngine;

namespace FishFlingers.Items
{
    [CreateAssetMenu(fileName = "ItemManagerConfig", menuName = "Configs/Managers/ItemManagerConfig")]
    public class ItemManagerConfig : ScriptableObject
    {
        [SerializeField] private ItemDataScanner _itemDataScanner;

        public ItemDataScanner ItemDataScanner => _itemDataScanner;
    }
}