using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "CharacterDefeatSettings", menuName = "Settings/Entities/CharacterDefeatSettings")]
    public class CharacterDefeatSettings : ScriptableObject
    {
        [SerializeField] private float _defeatDuration = 1f;
        [SerializeField] private float _tweenDuration = 0.5f;

        public float DefeatDuration => _defeatDuration;
        public float TweenDuration => _tweenDuration;
    }
}