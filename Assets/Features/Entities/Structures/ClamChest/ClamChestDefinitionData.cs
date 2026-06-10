using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "ClamChestDefinitionData", menuName = "Data/Entities/Structures/ClamChestDefinitionData")]
    public class ClamChestDefinitionData : StructureDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
    }
}