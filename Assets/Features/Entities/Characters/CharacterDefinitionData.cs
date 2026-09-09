using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class CharacterDefinitionData : EntityDefinitionData
    {
        [SerializeField] private CharacterActSettings _actSettings;
        public CharacterActSettings ActSettings => _actSettings;

        public CharacterDefeatSettings CharacterDefeatSettings => (CharacterDefeatSettings)_entityDefeatSettings;
        public CharacterPhysicsSettings CharacterPhysicsSettings => (CharacterPhysicsSettings)_entityPhysicsSettings;
    }
}