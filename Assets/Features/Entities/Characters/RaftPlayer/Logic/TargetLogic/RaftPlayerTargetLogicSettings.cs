using FishFlingers.Entities;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerTargetLogicSettings", menuName = "Settings/Entities/RaftPlayerTargetLogicSettings")]
    public class RaftPlayerTargetLogicSettings : ScriptableObject
    {
        [SerializeField] private RaftPlayerTargetVisual _targetVisualPrefab;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        public RaftPlayerTargetVisual TargetVisualPrefab => _targetVisualPrefab;
        public Color ValidColor => _validColor;
        public Color InvalidColor => _invalidColor;
    }
}