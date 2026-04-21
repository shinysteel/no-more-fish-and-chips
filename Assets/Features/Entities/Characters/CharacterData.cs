using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Data/Entities/Characters/CharacterData")]
    public abstract class CharacterData : EntityData
    {
        [SerializeField] protected CharacterDefeatSettings _characterDefeatSettings;
        [SerializeField] protected CharacterPhysicsSettings _characterPhysicsSettings;

        public CharacterDefeatSettings CharacterDefeatSettings => _characterDefeatSettings;
        public CharacterPhysicsSettings CharacterPhysicsSettings => _characterPhysicsSettings;
    }
}