using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Sky : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;

        private Material _material;

        private const string GradientName = "_Gradient";

        private void Awake()
        {
            _material = _meshRenderer.material;
        }

        public void SetGradient(Texture2D texture)
        {
            _material.SetTexture(GradientName, texture);
        }
    }
}