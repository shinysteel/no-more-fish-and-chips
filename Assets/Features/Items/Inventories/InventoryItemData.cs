using UnityEngine;

namespace FishFlingers.Items
{
    [CreateAssetMenu(fileName = "InventoryItemData", menuName = "Data/InventoryItemData")]
    public class InventoryItemData : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
    }
}