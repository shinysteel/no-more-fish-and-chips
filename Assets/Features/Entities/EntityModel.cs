using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityModel : MonoBehaviour, IPoolable
    {
        [SerializeField] private EntityId _id;
        [SerializeField] protected Animator _animator;
        [SerializeField] private NetworkAnimator _networkAnimator;

        protected ItemManager _itemManager;

        protected Material _material;

        public EntityId Id => _id;
        public Animator Animator => _animator;
        public Material Material => _material;

        private void Awake()
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();

            foreach (MeshRenderer renderer in transform.GetComponentsInChildren<MeshRenderer>())
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
        }

        public void SetTrigger(string name)
        {
            if (_networkAnimator == null)
            {
                _animator.SetTrigger(name);
            }
            else
            {
                _networkAnimator.SetTrigger(name);
            }
        }

        public virtual void OnReturnedToPool()
        {
            Utils.GameObjects.TraverseHierarchy(gameObject, (GameObject obj) => obj.layer = (int)ELayer.Default);
        }

        public virtual void OnTakenFromPool()
        { }
    }
}