using FishFlingers.Hitboxes;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "FlyingFishData", menuName = "Data/Entities/Characters/FlyingFishData")]
    public class FlyingFishData : CharacterData
    {
        [SerializeField] private float _scoutDuration = 1.5f;
        [SerializeField] private float _flyDuration = 1.25f;
        [SerializeField] private float _launchAngle = 75f;
        [SerializeField] private HitboxData _impactHitboxData;

        public float ScoutDuration => _scoutDuration;
        public float FlyDuration => _flyDuration;
        public float LaunchAngle => _launchAngle;
        public HitboxData ImpactHitboxData => _impactHitboxData;
    }
}