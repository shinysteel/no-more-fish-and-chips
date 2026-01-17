using ShinyOwl.Common.Framework;
using UnityEngine;

namespace FishFlingers.Pools
{
    [CreateAssetMenu(fileName = "PoolableScanner", menuName = "Scanners/PoolableScanner")]
    public class PoolableScanner : AssetScanner<IPoolable>
    { }
}