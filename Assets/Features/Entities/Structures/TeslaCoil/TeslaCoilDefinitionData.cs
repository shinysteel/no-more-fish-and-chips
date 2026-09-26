using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TeslaCoilDefinitionData", menuName = "Data/Entities/Structures/TeslaCoilDefinitionData")]
    public class TeslaCoilDefinitionData : StructureDefinitionData
    {
        [SerializeField] private float _chargeDuration = 10f;
        [SerializeField] private float _electrocuteRadius = 3f;
        [SerializeField] private LayerMask _electrocuteMask;
        [SerializeField] private Hit _electrocuteHit;
        [SerializeField] private Vector3 _electricityPosition = Vector3.up * 0.75f;

        public float ChargeDuration => _chargeDuration;
        public float ElectrocuteRadius => _electrocuteRadius;
        public LayerMask ElectrocuteMask => _electrocuteMask;
        public Hit ElectrocuteHit => _electrocuteHit;
        public Vector3 ElectricityPosition => _electricityPosition;
    }
}