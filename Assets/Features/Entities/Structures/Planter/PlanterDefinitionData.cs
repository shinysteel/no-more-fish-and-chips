using NoMoreFishAndChips.Items;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "PlanterDefinitionData", menuName = "Data/Entities/Structures/PlanterDefinitionData")]
    public class PlanterDefinitionData : StructureDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private Recipe _plantRecipe;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public Recipe PlantRecipe => _plantRecipe;
    }
}