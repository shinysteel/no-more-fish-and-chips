using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "FlyingFishDefinitionData", menuName = "Data/Entities/Characters/FlyingFishDefinitionData")]
    public class FlyingFishDefinitionData : CharacterDefinitionData
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