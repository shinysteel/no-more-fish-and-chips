using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerContextActionsSettings", menuName = "Settings/Entities/RaftPlayerContextActionsSettings")]
    public class RaftPlayerContextActionsSettings : ScriptableObject
    {
        [SerializeField] private float _offset = 1f;

        public float Offset => _offset;
    }
}