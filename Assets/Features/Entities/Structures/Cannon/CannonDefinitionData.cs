using NoMoreFishAndChips.Items;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "CannonDefinitionData", menuName = "Data/Entities/Structures/CannonDefinitionData")]
    public class CannonDefinitionData : StructureDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private Recipe _loadRecipe;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public Recipe LoadRecipe => _loadRecipe;
    }
}