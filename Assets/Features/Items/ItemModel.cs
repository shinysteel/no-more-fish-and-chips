using Ara;
using NoMoreFishAndChips.Pools;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    public class ItemModel : MonoBehaviour, IPoolable
    {
        [SerializeField] private ItemId _itemId;
        [SerializeField] private AraTrail[] _trails;

        public ItemId ItemId => _itemId;

        public void SetTrailEmitting(bool emit)
        {
            if (_trails == null)
            {
                return;
            }

            foreach (AraTrail trail in _trails)
            {
                trail.emit = emit;
            }
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}