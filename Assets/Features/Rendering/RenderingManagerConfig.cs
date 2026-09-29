using UnityEngine;
using System;

namespace NoMoreFishAndChips.Rendering
{
    [CreateAssetMenu(fileName = "RenderingManagerConfig", menuName = "Configs/Managers/RenderingManagerConfig")]
    public class RenderingManagerConfig : ScriptableObject
    {
        [SerializeField] private OutlineConfig _outlineConfig;

        public OutlineConfig OutlineConfig => _outlineConfig;
    }

    [Serializable]
    public class OutlineConfig
    {
        [SerializeField] private Material _maskMaterial;
        [SerializeField] private Material _occlusionMaterial;
        [SerializeField] private Material _combineMaterial;
        [SerializeField] private Material _horizontalMaterial;
        [SerializeField] private Material _verticalMaterial;

        public Material MaskMaterial => _maskMaterial;
        public Material CombineMaterial => _combineMaterial;
        public Material OcclusionMaterial => _occlusionMaterial;
        public Material HorizontalMaterial => _horizontalMaterial;
        public Material VerticalMaterial => _verticalMaterial;
    }
}