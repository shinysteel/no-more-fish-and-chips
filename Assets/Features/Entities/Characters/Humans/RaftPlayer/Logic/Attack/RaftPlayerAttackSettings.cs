using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerAttackSettings", menuName = "Settings/Entities/RaftPlayerAttackSettings")]
    public class RaftPlayerAttackSettings : ScriptableObject
    {
        [SerializeField] private HitboxData _paddleSwingHitboxData;
        [SerializeField] private float _paddleLungeStrength = 20f;
        [SerializeField] private HitboxData _spearJabHitboxData;
        [SerializeField] private float _spearLungeStrength = 10f;

        public HitboxData PaddleSwingHitboxData => _paddleSwingHitboxData;
        public float PaddleLungeStrength => _paddleLungeStrength;
        public HitboxData SpearJabHitboxData => _spearJabHitboxData;
        public float SpearLungeStrength => _spearLungeStrength;
    }
}