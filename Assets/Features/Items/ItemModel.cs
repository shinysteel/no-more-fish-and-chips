using Ara;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    public class ItemModel : MonoBehaviour, IPoolable
    {
        [SerializeField] private ItemId _itemId;
        [SerializeField] private AraTrail[] _trails = new AraTrail[0];

        private MeshRenderer[] _meshRenderers;

        public ItemId ItemId => _itemId;
        public MeshRenderer[] MeshRenderers => _meshRenderers;

        private void Awake()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        public void SetTrailEmitting(bool emit)
        {
            foreach (AraTrail trail in _trails)
            {
                trail.emit = emit;
            }
        }

        public void OnReturnedToPool()
        {
            SetTrailEmitting(false);
        }

        public void OnTakenFromPool()
        { }
    }
}