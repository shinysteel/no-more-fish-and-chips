using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class BlueprintRequirementEntry : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private TextMeshProUGUI _text;

        private ItemManager _itemManager;

        private void Awake()
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        public void Setup(RecipeRequirement requirement)
        {
            ItemDefinitionData itemData = _itemManager.GetItemDefinitionData(requirement.ItemId);

            _text.text = $"x{requirement.Count}<sprite name=\"{itemData.SpriteAssetName}\">";
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}