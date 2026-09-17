using NoMoreFishAndChips.Entities;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerBuildSettings", menuName = "Settings/Entities/RaftPlayerBuildSettings")]
    public class RaftPlayerBuildSettings : ScriptableObject
    {
        [SerializeField] private float _range = 0.75f;
        [SerializeField] private ActionData[] _actionDatas;
        [SerializeField] private BuildTargetSettings _targetSettings;

        public float Range => _range;
        public ActionData[] ActionDatas => _actionDatas;
        public BuildTargetSettings TargetSettings => _targetSettings;
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