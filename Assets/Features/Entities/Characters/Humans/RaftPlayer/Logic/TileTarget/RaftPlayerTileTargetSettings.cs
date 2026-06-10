using NoMoreFishAndChips.Entities;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerTileTargetSettings", menuName = "Settings/Entities/RaftPlayerTileTargetSettings")]
    public class RaftPlayerTileTargetSettings : ScriptableObject
    {
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        public Color ValidColor => _validColor;
        public Color InvalidColor => _invalidColor;
    }
}