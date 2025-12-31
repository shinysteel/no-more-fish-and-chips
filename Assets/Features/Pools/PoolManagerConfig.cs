using UnityEngine;
using FishFlingers.UI;
using FishFlingers.Environments;

namespace FishFlingers.Pools
{
    [CreateAssetMenu(fileName = "PoolManagerConfig", menuName = "Configs/Managers/PoolManagerConfig")]
    public class PoolManagerConfig : ScriptableObject
    {
        [SerializeField] private Tile _tilePrefab;

        public Tile TilePrefab => _tilePrefab;
    }
}