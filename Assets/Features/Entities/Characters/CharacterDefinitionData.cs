using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class CharacterDefinitionData : EntityDefinitionData
    {
        [SerializeField] private CharacterActSettings _actSettings;
        [SerializeField] private CharacterNavigateSettings _navigateSettings;

        public CharacterActSettings ActSettings => _actSettings;
        public CharacterNavigateSettings NavigateSettings => _navigateSettings;

        public CharacterDefeatSettings CharacterDefeatSettings => (CharacterDefeatSettings)_entityDefeatSettings;
        public CharacterPhysicsSettings CharacterPhysicsSettings => (CharacterPhysicsSettings)_entityPhysicsSettings;
    }
}