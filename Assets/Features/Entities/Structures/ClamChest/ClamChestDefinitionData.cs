using ShinyOwl.Common.Structures;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "ClamChestDefinitionData", menuName = "Data/Entities/Structures/ClamChestDefinitionData")]
    public class ClamChestDefinitionData : StructureDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private BoolGrid _inventoryLayout;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public BoolGrid InventoryLayout => _inventoryLayout;
    }
}