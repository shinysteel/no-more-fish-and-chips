using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "GiantClamDefinitionData", menuName = "Data/Entities/Characters/GiantClamDefinitionData")]
    public class GiantClamDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
    }
}