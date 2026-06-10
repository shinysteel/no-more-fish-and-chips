using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class RequirementPromptItem : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _countText;

        private ItemManager _itemManager;

        private GameplayContext _context;
        private RecipeRequirement _requirement;

        private void Awake()
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        public void Setup(GameplayContext context, RecipeRequirement requirement)
        {
            _context = context;
            _requirement = requirement;

            ItemDefinitionData data = _itemManager.GetItemDefinitionData(requirement.ItemId);
            _image.sprite = data.Sprite;

            Refresh();
            _context.LocalPlayer.Inventory.OnInventoryItemChanged += HandleInventoryItemChanged;
        }

        public void OnReturnedToPool()
        {
            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.Inventory.OnInventoryItemChanged -= HandleInventoryItemChanged;
            }
        }

        private void HandleInventoryItemChanged(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
        {
            Refresh();
        }

        private void Refresh()
        {
            int count = _context.LocalPlayer.Inventory.InventoryItems.Values
                .Where(item => item.ItemInstance.Data.ItemId == _requirement.ItemId)
                .Sum(item => item.ItemInstance.Count);

            _countText.text = $"{count}/{_requirement.Count}";
        }

        public void OnTakenFromPool()
        { }
    }
}