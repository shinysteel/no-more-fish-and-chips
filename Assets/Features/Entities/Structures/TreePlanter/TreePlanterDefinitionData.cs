using NoMoreFishAndChips.Items;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TreePlanterDefinitionData", menuName = "Data/Entities/Structures/TreePlanterDefinitionData")]
    public class TreePlanterDefinitionData : StructureDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private Recipe _plantRecipe;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public Recipe PlantRecipe => _plantRecipe;
    }
}