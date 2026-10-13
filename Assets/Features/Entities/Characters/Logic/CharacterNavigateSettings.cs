using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "CharacterNavigateSettings", menuName = "Settings/Entities/CharacterNavigateSettings")]
    public class CharacterNavigateSettings : ScriptableObject
    {
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _moveAcceleration = 10f;
        [SerializeField] private float _rotateSpeed = 7.5f;

        public float MoveSpeed => _moveSpeed;
        public float MoveAcceleration => _moveAcceleration;
        public float RotateSpeed => _rotateSpeed;
    }
}