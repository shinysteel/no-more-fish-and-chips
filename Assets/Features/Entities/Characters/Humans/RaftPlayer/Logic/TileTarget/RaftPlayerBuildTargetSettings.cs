using NoMoreFishAndChips.Entities;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerBuildTargetSettings", menuName = "Settings/Entities/RaftPlayerBuildTargetSettings")]
    public class RaftPlayerBuildTargetSettings : ScriptableObject
    {
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        public Color ValidColor => _validColor;
        public Color InvalidColor => _invalidColor;
    }
}