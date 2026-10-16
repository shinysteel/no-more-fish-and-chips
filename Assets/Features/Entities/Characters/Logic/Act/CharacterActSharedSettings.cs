using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "CharacterActSharedSettings", menuName = "Settings/Entities/Characters/CharacterActSharedSettings")]
    public class CharacterActSharedSettings : ScriptableObject
    {
        [SerializeField] private float _regenDelay = 3f;
        [SerializeField] private float _staggerDuration = 1f;

        public float RegenDelay => _regenDelay;
        public float StaggerDuration => _staggerDuration;
    }
}