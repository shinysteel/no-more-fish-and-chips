using ShinyOwl.Common.Structures;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "GiantClamDefinitionData", menuName = "Data/Entities/Characters/GiantClamDefinitionData")]
    public class GiantClamDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private BoolGrid _inventoryLayout;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public BoolGrid InventoryLayout => _inventoryLayout;
    }
}