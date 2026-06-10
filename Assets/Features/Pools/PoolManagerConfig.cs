using UnityEngine;
using NoMoreFishAndChips.UI;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Entities;

namespace NoMoreFishAndChips.Pools
{
    [CreateAssetMenu(fileName = "PoolManagerConfig", menuName = "Configs/Managers/PoolManagerConfig")]
    public class PoolManagerConfig : ScriptableObject
    {
        [SerializeField] private ITypedPoolableScanner _iTypedPoolableScanner;

        public ITypedPoolableScanner ITypedPoolableScanner => _iTypedPoolableScanner;
    }
}