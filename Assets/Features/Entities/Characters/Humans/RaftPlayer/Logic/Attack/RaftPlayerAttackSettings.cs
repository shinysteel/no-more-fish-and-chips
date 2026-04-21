using FishFlingers.Hitboxes;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerAttackSettings", menuName = "Settings/Entities/RaftPlayerAttackSettings")]
    public class RaftPlayerAttackSettings : ScriptableObject
    {
        [SerializeField] private HitboxData _hitboxData;
        [SerializeField] private float _lungeStrength;

        public HitboxData HitboxData => _hitboxData;
        public float LungeStrength => _lungeStrength;
    }
}