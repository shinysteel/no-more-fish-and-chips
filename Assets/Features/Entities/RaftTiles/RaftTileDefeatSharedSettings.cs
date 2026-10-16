using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftTileDefeatSharedSettings", menuName = "Settings/Entities/Characters/RaftPlayer/RaftTileDefeatSharedSettings")]
    public class RaftTileDefeatSharedSettings : EntityDefeatSharedSettings
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _speed = 0.25f;

        public float Duration => _duration;
        public float Speed => _speed;
    }
}