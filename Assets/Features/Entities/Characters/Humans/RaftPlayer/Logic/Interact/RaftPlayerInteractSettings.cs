using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerInteractSettings", menuName = "Settings/Entities/RaftPlayerInteractSettings")]
    public class RaftPlayerInteractSettings : ScriptableObject
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _radius = 1f;
        [SerializeField] private float _maxDistance = 0.5f;
        [SerializeField] private float _maxAngle = 30f;

        public LayerMask Mask => _mask;
        public float Radius => _radius;
        public float MaxDistance => _maxDistance;
        public float MaxAngle => _maxAngle;
    }
}