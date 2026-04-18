using ShinyOwl.Common.Structures;
using UnityEngine;
using FishFlingers.Items;
using UnityEngine.Events;

namespace FishFlingers.Inventories
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Data/Items/ItemData")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private ItemId _itemId;
        [SerializeField] private string _spriteAssetName;
        [SerializeField] private int _maxStack;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private BoolGrid _shape;
        [SerializeField] private ItemModel _model;
        [SerializeField] private ItemActionData _leftClickAction;
        [SerializeField] private ItemActionData _rightClickAction;
        [SerializeField] private bool _showsTileTarget;

        // To differentiate from InstanceId, we use ItemId
        public ItemId ItemId => _itemId;

        public string SpriteAssetName => _spriteAssetName;
        public int MaxStack => _maxStack;
        public Sprite Sprite => _sprite;
        public BoolGrid Shape => _shape;
        public ItemModel Model => _model;
        public ItemActionData LeftClickAction => _leftClickAction;
        public ItemActionData RightClickAction => _rightClickAction;
        public bool ShowsTileTarget => _showsTileTarget;
    }
}