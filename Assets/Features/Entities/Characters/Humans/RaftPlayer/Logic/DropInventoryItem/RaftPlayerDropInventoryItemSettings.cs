using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerDropInventoryItemSettings", menuName = "Settings/Entities/RaftPlayerDropInventoryItemSettings")]
    public class RaftPlayerDropInventoryItemSettings : ScriptableObject
    {
        [SerializeField] private float _pitch;
        [SerializeField] private float _strength;

        public float Pitch => _pitch;
        public float Strength => _strength;
    }
}