using NoMoreFishAndChips.States;
using ShinyOwl.Common.Structures;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NoMoreFishAndChips.Inventories;

namespace NoMoreFishAndChips.Items
{
    public enum ItemType
    {
        Resource,
        Tool,
        Weapon,
        Hat,
        Clothing
    }

    [CreateAssetMenu(fileName = "ItemDefinitionData", menuName = "Data/Items/ItemDefinitionnData")]
    public class ItemDefinitionData : DefinitionData, ICraftable
    {
        [SerializeField] private ItemId _itemId;
        [SerializeField] private ItemType _itemType;
        [SerializeField] private string _spriteAssetName;
        [SerializeField] private int _maxStack;
        [SerializeField] private Recipe _recipe;
        [SerializeField] private BoolGrid _shape;
        [SerializeField] private ItemModel _model;
        [SerializeField] private ItemActionData[] _actionDatas = new ItemActionData[0];

        // To differentiate from InstanceId, we use ItemId
        public ItemId ItemId => _itemId;
        public ItemType ItemType => _itemType;

        public string SpriteAssetName => _spriteAssetName;
        public int MaxStack => _maxStack;
        public DefinitionData DefinitionData => this;
        public Recipe BuildRecipe => _recipe;
        public BoolGrid Shape => _shape;
        public ItemModel Model => _model;
        public ItemActionData[] ActionDatas => _actionDatas;

        bool ICraftable.TryCraft(GameplayContext context)
        {
            List<InventoryChangeParams> parameters = _recipe.ToChangeParams();

            if (!context.LocalPlayer.Inventory.TryRemoveItems(parameters))
            {
                return false;
            }

            NetItemInstance netItemInstance = new NetItemInstance(null, _itemId, 1);
            context.LocalPlayer.Inventory.TryAddItem(InventoryChangeParams.Create(netItemInstance), false, out _, out _, out _);

            return true;
        }
    }
}