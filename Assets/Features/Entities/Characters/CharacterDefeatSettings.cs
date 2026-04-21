using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "CharacterDefeatSettings", menuName = "Settings/Entities/CharacterDefeatSettings")]
    public class CharacterDefeatSettings : ScriptableObject
    {
        [SerializeField] private float _duration = 2.5f;

        public float Duration => _duration;
    }
}