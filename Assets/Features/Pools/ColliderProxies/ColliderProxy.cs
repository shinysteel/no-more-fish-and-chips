using UnityEngine;
using System;

namespace NoMoreFishAndChips.Pools
{
    public abstract class ColliderProxy : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] protected Collider _collider;
        [SerializeField] private GameObject _ownerGameObject;

        public Collider Collider => _collider;
        public GameObject OwnerGameObject => _ownerGameObject;
        
        public event Action<Collider, Collider> OnUnityTriggerStay;

        private void OnTriggerStay(Collider collider)
        {
            OnUnityTriggerStay?.Invoke(_collider, collider);
        }

        public void SetOwnerGameObject(GameObject gameObject)
        {
            _ownerGameObject = gameObject;
        }

        public void OnReturnedToPool()
        {
            _ownerGameObject = null;
        }

        public void OnTakenFromPool()
        { }
    }

    public abstract class ColliderProxy<T> : ColliderProxy where T : Collider
    {
        public new T Collider => (T)_collider;
    }
}