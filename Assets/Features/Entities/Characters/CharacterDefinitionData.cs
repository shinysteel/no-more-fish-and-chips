using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class CharacterDefinitionData : EntityDefinitionData
    {
        [SerializeField] private CharacterActSharedSettings _actSharedSettings;
        [SerializeField] private CharacterActSettings _actSettings;
        [SerializeField] private CharacterNavigateSettings _navigateSettings;

        public CharacterActSharedSettings ActSharedSettings => _actSharedSettings;
        public CharacterActSettings ActSettings => _actSettings;
        public CharacterNavigateSettings NavigateSettings => _navigateSettings;

        public CharacterDefeatSharedSettings CharacterDefeatSharedSettings => (CharacterDefeatSharedSettings)_entityDefeatSharedSettings;
        public CharacterPhysicsSharedSettings CharacterPhysicsSharedSettings => (CharacterPhysicsSharedSettings)_entityPhysicsSharedSettings;
    }
}