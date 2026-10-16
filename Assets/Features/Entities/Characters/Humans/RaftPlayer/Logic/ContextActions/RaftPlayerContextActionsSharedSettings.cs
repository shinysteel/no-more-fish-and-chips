using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerContextActionsSharedSettings", menuName = "Settings/Entities/Characters/RaftPlayer/RaftPlayerContextActionsSharedSettings")]
    public class RaftPlayerContextActionsSharedSettings : ScriptableObject
    {
        [SerializeField] private float _offset = 1f;

        public float Offset => _offset;
    }
}