using UnityEngine;
using System;

namespace NoMoreFishAndChips.Pools
{
    public abstract class ColliderProxy : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] protected Collider _collider;
        public Collider Collider => _collider;

        public event Action<Collider, Collider> OnUnityTriggerStay;

        private void OnTriggerStay(Collider collider)
        {
            OnUnityTriggerStay?.Invoke(_collider, collider);
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }

    public abstract class ColliderProxy<T> : ColliderProxy where T : Collider
    {
        public new T Collider => (T)_collider;
    }
}