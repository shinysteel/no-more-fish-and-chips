using NoMoreFishAndChips.Entities;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerBuildTargetSettings", menuName = "Settings/Entities/RaftPlayerBuildTargetSettings")]
    public class RaftPlayerBuildTargetSettings : ScriptableObject
    {
        [SerializeField] private BuildTargetSettings _buildTargetSettings;

        public BuildTargetSettings BuildTargetSettings => _buildTargetSettings;
    }

    [Serializable]
    public class BuildTargetSettings
    {
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        public Color ValidColor => _validColor;
        public Color InvalidColor => _invalidColor;
    }
}