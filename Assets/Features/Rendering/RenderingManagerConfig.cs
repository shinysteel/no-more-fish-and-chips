using UnityEngine;

namespace NoMoreFishAndChips.Rendering
{
    [CreateAssetMenu(fileName = "RenderingManagerConfig", menuName = "Configs/Managers/RenderingManagerConfig")]
    public class RenderingManagerConfig : ScriptableObject
    {
        [SerializeField] private Material _outlineMaskMaterial;
        [SerializeField] private Material _outlineHorizontalMaterial;
        [SerializeField] private Material _outlineVerticalMaterial;

        public Material OutlineMaskMaterial => _outlineMaskMaterial;
        public Material OutlineHorizontalMaterial => _outlineHorizontalMaterial;
        public Material OutlineVerticalMaterial => _outlineVerticalMaterial;
    }
}