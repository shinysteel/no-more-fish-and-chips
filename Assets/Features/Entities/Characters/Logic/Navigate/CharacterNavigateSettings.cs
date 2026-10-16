using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [Serializable]
    public class CharacterNavigateSettings
    {
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _moveAcceleration = 10f;
        [SerializeField] private float _rotateSpeed = 7.5f;

        public float MoveSpeed => _moveSpeed;
        public float MoveAcceleration => _moveAcceleration;
        public float RotateSpeed => _rotateSpeed;
    }
}