using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TeslaCoilDefinitionData", menuName = "Data/Entities/Structures/TeslaCoilDefinitionData")]
    public class TeslaCoilDefinitionData : StructureDefinitionData
    {
        [SerializeField] private float _chargeDuration = 10f;
        [SerializeField] private Vector3 _electricityPosition = Vector3.up * 0.75f;
        [SerializeField] private float _electrocuteRadius = 3f;
        [SerializeField] private LayerMask _electrocuteMask;
        [SerializeField] private int _electroctuteDamage = 3;

        public float ChargeDuration => _chargeDuration;
        public Vector3 ElectricityPosition => _electricityPosition;
        public float ElectrocuteRadius => _electrocuteRadius;
        public LayerMask ElectrocuteMask => _electrocuteMask;
        public int ElectrocuteDamage => _electroctuteDamage;
    }
}