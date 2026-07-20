using NoMoreFishAndChips.Pools;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Prop : MonoBehaviour, IPoolable, ISurface
    {
        [SerializeField] private PropId _id;
        [SerializeField] private SurfaceType _surfaceType;

        public PropId Id => _id;
        SurfaceType ISurface.SurfaceType => _surfaceType;

        private Material _material;
        private Color _defaultColor;

        private void Awake()
        {
            foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
            {
                if (_material == null)
                {
                    _material = renderer.material;
                }
                else
                {
                    renderer.material = _material;
                }
            }

            _defaultColor = _material.color;
        }

        public void SetColor(Color color)
        {
            _material.color = color;
        }

        public void OnReturnedToPool()
        {
            SetColor(_defaultColor);
        }

        public void OnTakenFromPool()
        { }
    }
}