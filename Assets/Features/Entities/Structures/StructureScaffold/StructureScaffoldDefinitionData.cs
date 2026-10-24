using UnityEngine;
using NoMoreFishAndChips.Interactions;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "StructureScaffoldDefinitionData", menuName = "Data/Entities/Structures/StructureScaffoldDefinitionData")]
    public class StructureScaffoldDefinitionData : StructureDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
    }
}