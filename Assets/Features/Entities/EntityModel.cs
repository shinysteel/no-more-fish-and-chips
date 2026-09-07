using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityModel : MonoBehaviour, IPoolable
    {
        [SerializeField] private EntityId _id;
        [SerializeField] protected Animator _animator;
        [SerializeField] private NetworkAnimator _networkAnimator;

        protected ItemManager _itemManager;

        private Dictionary<Material, Material> _sharedMaterialMap = new();
        
        public EntityId Id => _id;
        public Animator Animator => _animator;

        private void Awake()
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();

            foreach (MeshRenderer renderer in transform.GetComponentsInChildren<MeshRenderer>())
            {
                Material sharedMaterial = renderer.sharedMaterial;

                if (!_sharedMaterialMap.TryGetValue(sharedMaterial, out Material material))
                {
                    _sharedMaterialMap.Add(sharedMaterial, renderer.material);
                }
                else
                {
                    renderer.material = material;
                } 
            }
        }

        public void SetAnimatorTrigger(string name)
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

        public void SetMaterialFloat(string name, float value)
        {
            foreach (Material material in _sharedMaterialMap.Values)
            {
                material.SetFloat(name, value);
            }
        }

        public virtual void OnReturnedToPool()
        { }

        public virtual void OnTakenFromPool()
        { }
    }
}