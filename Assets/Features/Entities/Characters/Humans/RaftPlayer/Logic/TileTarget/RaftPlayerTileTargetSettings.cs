using FishFlingers.Entities;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerTileTargetSettings", menuName = "Settings/Entities/RaftPlayerTileTargetSettings")]
    public class RaftPlayerTileTargetSettings : ScriptableObject
    {
        [SerializeField] private RaftPlayerTileTargetVisual _targetVisualPrefab;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        public RaftPlayerTileTargetVisual TargetVisualPrefab => _targetVisualPrefab;
        public Color ValidColor => _validColor;
        public Color InvalidColor => _invalidColor;
    }
}