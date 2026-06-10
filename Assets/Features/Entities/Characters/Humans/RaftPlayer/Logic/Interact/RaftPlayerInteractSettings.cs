using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerInteractSettings", menuName = "Settings/Entities/RaftPlayerInteractSettings")]
    public class RaftPlayerInteractSettings : ScriptableObject
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _radius = 1f;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        public LayerMask Mask => _mask;
        public float Radius => _radius;
        public Color ValidColor => _validColor;
        public Color InvalidColor => _invalidColor;
    }
}