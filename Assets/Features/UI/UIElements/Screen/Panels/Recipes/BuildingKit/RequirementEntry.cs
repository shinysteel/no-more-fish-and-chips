using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Pools;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class RequirementEntry : MonoBehaviour, IPoolable
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _image;

        private ItemManager _itemManager;

        private void Awake()
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        public void Setup(RecipeRequirement requirement)
        {
            ItemDefinitionData itemData = _itemManager.GetItemData(requirement.ItemId);

            _text.text = $"x{requirement.Count}<sprite name=\"{itemData.SpriteAssetName}\">";

            _image.enabled = transform.GetSiblingIndex() % 2 != 0;
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}