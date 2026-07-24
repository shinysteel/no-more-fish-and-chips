using NoMoreFishAndChips.Items;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class HumanModel : CharacterModel
    {
        [SerializeField] private Transform _rightArmItemLocator;

        private ItemModel _rightArmItemModel;

        public ItemModel RightArmItemModel => _rightArmItemModel;

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
            base.OnReturnedToPool();

            HoldItem(ItemId.None);
        }
    }
}