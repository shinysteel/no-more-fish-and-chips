using NoMoreFishAndChips.Pools;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Prop : MonoBehaviour, IPoolable
    {
        [SerializeField] private PropId _id;
        public PropId Id => _id;

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