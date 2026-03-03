using PrimeTween;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerTarget : MonoBehaviour
    {
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private MeshRenderer _visualMeshRenderer;

        private Material _material;

        private const float MaxAlpha = 0.4f; // Equivalent to ~102 in color32

        private void Awake()
        {
            _material = _visualMeshRenderer.material;

            SetAlphaBlend(0f);
        }

        public void SetVisualScale(Vector3 scale)
        {
            _visualTransform.localScale = scale;
        }

        /// <summary>
        /// Returns a normalised blend from 0 - 1
        /// </summary>
        public float GetAlphaBlend()
        {
            return _material.color.a / MaxAlpha;
        }

        /// <summary>
        /// Applys a normalised blend of 0 - 1
        /// </summary>
        public void SetAlphaBlend(float blend)
        {
            Color color = _material.color;
            color.a = blend * MaxAlpha;
            _material.color = color;
        }
    }
}