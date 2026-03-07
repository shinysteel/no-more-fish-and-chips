using PrimeTween;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerTargetVisual : MonoBehaviour
    {
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private MeshRenderer _visualMeshRenderer;

        private Material _material;

        public Material Material => _material;

        public enum EColor
        {
            Valid   ,
            Invalid ,
        }

        private void Awake()
        {
            _material = _visualMeshRenderer.material;

            SetAlpha(0f);
        }

        public void SetVisualScale(Vector3 scale)
        {
            _visualTransform.localScale = scale;
        }

        public void SetAlpha(float alpha)
        {
            Color color = _material.color;
            color.a = alpha;
            _material.color = color;
        }

        public void SetColor(Color color)
        {
            color.a = _material.color.a;
            _material.color = color;
        }
    }
}