using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerContextSettings", menuName = "Settings/Entities/RaftPlayerContextSettings")]
    public class RaftPlayerContextSettings : ScriptableObject
    {
        [SerializeField] private float _offset = 1f;

        public float Offset => _offset;
    }
}