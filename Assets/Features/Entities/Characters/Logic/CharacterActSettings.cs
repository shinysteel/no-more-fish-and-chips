using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "CharacterActSettings", menuName = "Settings/Entities/CharacterActSettings")]
    public class CharacterActSettings : ScriptableObject
    {
        [SerializeField] private float _poise = 1f;
        [SerializeField] private float _regenDelay = 3f;
        [SerializeField] private float _staggerDuration = 1f;

        public float Poise => _poise;
        public float RegenDelay => _regenDelay;
        public float StaggerDuration => _staggerDuration;
    }
}