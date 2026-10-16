using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerAttackSharedSettings", menuName = "Settings/Entities/Characters/RaftPlayer/RaftPlayerAttackSharedSettings")]
    public class RaftPlayerAttackSharedSettings : ScriptableObject
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