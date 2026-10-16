using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [Serializable]
    public class CharacterActSettings
    {
        [SerializeField] private float _poise = 1f;

        public float Poise => _poise;
    }
}