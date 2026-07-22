using UnityEngine;

namespace NoMoreFishAndChips.Scenes
{
    public class EnvironmentMenusReferences : MonoBehaviour
    {
        [SerializeField] private Transform _raftTransform;

        public Transform RaftTransform => _raftTransform;
    }
}