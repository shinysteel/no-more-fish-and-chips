using UnityEngine;

namespace FishFlingers.Inventories
{
    public interface IHasInventory
    {
        Inventory Inventory { get; }
    }
}