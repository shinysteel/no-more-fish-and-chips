using UnityEngine;
using FishFlingers.UI;
using FishFlingers.Environments;
using FishFlingers.Entities;

namespace FishFlingers.Pools
{
    [CreateAssetMenu(fileName = "PoolManagerConfig", menuName = "Configs/Managers/PoolManagerConfig")]
    public class PoolManagerConfig : ScriptableObject
    {
        [SerializeField] private PoolableScanner _poolableScanner;

        public PoolableScanner PoolableScanner => _poolableScanner;
    }
}