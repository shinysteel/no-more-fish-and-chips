using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftTileDefeatSettings", menuName = "Settings/Entities/RaftTileDefeatSettings")]
    public class RaftTileDefeatSettings : EntityDefeatSettings
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _speed = 0.25f;

        public float Duration => _duration;
        public float Speed => _speed;
    }
}