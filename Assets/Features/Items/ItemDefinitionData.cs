using ShinyOwl.Common.Structures;
using UnityEngine;
using FishFlingers.Items;
using UnityEngine.Events;
using FishFlingers.States;

namespace FishFlingers.Inventories
{
    [CreateAssetMenu(fileName = "ItemDefinitionData", menuName = "Data/Items/ItemDefinitionnData")]
    public class ItemDefinitionData : DefinitionData, ICraftable
    {
        [SerializeField] private ItemId _itemId;
        [SerializeField] private string _spriteAssetName;
        [SerializeField] private int _maxStack;
        [SerializeField] private Recipe _recipe;
        [SerializeField] private BoolGrid _shape;
        [SerializeField] private ItemModel _model;
        [SerializeField] private ItemActionData _leftClickAction;
        [SerializeField] private ItemActionData _rightClickAction;

        // To differentiate from InstanceId, we use ItemId
        public ItemId ItemId => _itemId;

        public string SpriteAssetName => _spriteAssetName;
        public int MaxStack => _maxStack;
        public DefinitionData DefinitionData => this;
        public Recipe Recipe => _recipe;
        public BoolGrid Shape => _shape;
        public ItemModel Model => _model;
        public ItemActionData LeftClickAction => _leftClickAction;
        public ItemActionData RightClickAction => _rightClickAction;

        public bool CanRepair => _leftClickAction is RepairActionData || _rightClickAction is RepairActionData;

        public bool TryCraft(GameplayContext context)
        {
            return false;
        }
    }
}