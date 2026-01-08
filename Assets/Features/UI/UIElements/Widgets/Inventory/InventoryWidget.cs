using UnityEngine;

namespace FishFlingers.UI
{
    public class InventoryWidget : UIElement
    {
        // max size: 7x7 with no corners

        [SerializeField] private InventorySlot _inventorySlotPrefab;

        [SerializeField] private float _padding = 5f;

        public override void Load()
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    InventorySlot slot = Instantiate(_inventorySlotPrefab, transform);

                    Vector3 position = new Vector3(i * slot.RectTransform.sizeDelta.x + i * _padding, j * slot.RectTransform.sizeDelta.y + j * _padding);

                    slot.Setup(new Vector2Int(i, j), position);
                }
            }
        }
    }
}