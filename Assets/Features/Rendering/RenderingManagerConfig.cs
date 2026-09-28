using UnityEngine;

namespace NoMoreFishAndChips.Rendering
{
    [CreateAssetMenu(fileName = "RenderingManagerConfig", menuName = "Configs/Managers/RenderingManagerConfig")]
    public class RenderingManagerConfig : ScriptableObject
    {
        [SerializeField] private Material _outlineMaskMaterial;
        [SerializeField] private Material _outlineMaterial;

        public Material OutlineMaskMaterial => _outlineMaskMaterial;
        public Material OutlineMaterial => _outlineMaterial;
    }
}