using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "CharacterDefeatSettings", menuName = "Settings/Entities/CharacterDefeatSettings")]
    public class CharacterDefeatSettings : EntityDefeatSettings
    {
        [SerializeField] private float _defeatDuration = 1f;
        [SerializeField] private float _tweenDuration = 0.5f;
        [SerializeField] private bool _defeatsInWater;

        public float DefeatDuration => _defeatDuration;
        public float TweenDuration => _tweenDuration;
        public bool DefeatsInWater => _defeatsInWater;
    }
}