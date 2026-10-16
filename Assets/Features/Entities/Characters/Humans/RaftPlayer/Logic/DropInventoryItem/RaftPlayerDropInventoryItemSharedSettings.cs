using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerDropInventoryItemSharedSettings", menuName = "Settings/Entities/Characters/RaftPlayer/RaftPlayerDropInventoryItemSharedSettings")]
    public class RaftPlayerDropInventoryItemSharedSettings : ScriptableObject
    {
        [SerializeField] private float _pitch = -45f;
        [SerializeField] private float _strength = 15f;

        public float Pitch => _pitch;
        public float Strength => _strength;
    }
}