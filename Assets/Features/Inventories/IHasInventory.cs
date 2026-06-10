using UnityEngine;

namespace NoMoreFishAndChips.Inventories
{
    public interface IHasInventory
    {
        Inventory Inventory { get; }
    }
}