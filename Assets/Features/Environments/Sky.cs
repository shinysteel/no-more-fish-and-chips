using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Sky : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;

        private Material _material;

        private const string CurrentGradientName = "_CurrentGradient";
        private const string NextGradientName = "_NextGradient";
        private const string GradientBlendName = "_GradientBlend";

        private void Awake()
        {
            _material = _meshRenderer.material;
        }

        public void SetGradientTextures(Texture2D currentTexture, Texture2D nextTexture)
        {
            _material.SetTexture(CurrentGradientName, currentTexture);
            _material.SetTexture(NextGradientName, nextTexture);
        }

        public void SetGradientBlend(float blend)
        {
            _material.SetFloat(GradientBlendName, blend);
        }
    }
}