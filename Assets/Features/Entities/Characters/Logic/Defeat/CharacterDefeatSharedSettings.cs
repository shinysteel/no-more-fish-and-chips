using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "CharacterDefeatSharedSettings", menuName = "Settings/Entities/Characters/CharacterDefeatSharedSettings")]
    public class CharacterDefeatSharedSettings : EntityDefeatSharedSettings
    {
        [SerializeField] private float _defeatDuration = 1f;
        [SerializeField] private float _tweenDuration = 0.5f;

        public float DefeatDuration => _defeatDuration;
        public float TweenDuration => _tweenDuration;
    }
}