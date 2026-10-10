using NoMoreFishAndChips.Items;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class HumanModel : CharacterModel
    {
        [SerializeField] private Transform _rightArmItemLocator;

        private ItemManager _itemManager;

        private ItemModel _rightArmItemModel;

        public ItemModel RightArmItemModel => _rightArmItemModel;

        protected override void Awake()
        {
            base.Awake();

            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        public void HoldItem(ItemId id)
        {
            if (_rightArmItemModel != null && _rightArmItemModel.ItemId != id)
            {
                _itemManager.ReturnModel(_rightArmItemModel);
                _rightArmItemModel = null;
            }

            if (_rightArmItemModel == null && id != ItemId.None)
            {
                ItemDefinitionData data = _itemManager.GetItemDefinitionData(id);

                _rightArmItemModel = _itemManager.GetModel(id, new SpawnParams()
                {
                    Position = data.HoldOffset,
                    Rotation = Quaternion.AngleAxis(90f, Vector3.up),
                    Parent = _rightArmItemLocator
                });
            }
        }

        public override void OnReturnedToPool()
        {
            HoldItem(ItemId.None);
        }
    }
}