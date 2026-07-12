using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerAttackSettings", menuName = "Settings/Entities/RaftPlayerAttackSettings")]
    public class RaftPlayerAttackSettings : ScriptableObject
    {
        [SerializeField] private HitboxData _paddleAttackHitboxData;
        [SerializeField] private float _paddleLungeStrength;

        public HitboxData PaddleAttackHitboxData => _paddleAttackHitboxData;
        public float PaddleLungeStrength => _paddleLungeStrength;
    }
}