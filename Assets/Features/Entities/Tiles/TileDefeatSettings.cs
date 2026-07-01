using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TileDefeatSettings", menuName = "Settings/Entities/TileDefeatSettings")]
    public class TileDefeatSettings : EntityDefeatSettings
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _speed = 0.1f;

        public float Duration => _duration;
        public float Speed => _speed;
    }
}