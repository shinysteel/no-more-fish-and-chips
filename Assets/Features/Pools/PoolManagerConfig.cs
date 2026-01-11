using UnityEngine;
using FishFlingers.UI;
using FishFlingers.Environments;
using FishFlingers.Entities;

namespace FishFlingers.Pools
{
    [CreateAssetMenu(fileName = "PoolManagerConfig", menuName = "Configs/Managers/PoolManagerConfig")]
    public class PoolManagerConfig : ScriptableObject
    {
        [SerializeField] private RaftTile _raftTilePrefab;
        [SerializeField] private LobbyEntry _lobbyEntryPrefab;
        [SerializeField] private InventorySlotView _inventorySlotViewPrefab;
        [SerializeField] private InventoryItemView _inventoryItemViewPrefab;

        public RaftTile RaftTilePrefab => _raftTilePrefab;
        public LobbyEntry LobbyEntryPrefab => _lobbyEntryPrefab;
        public InventorySlotView InventorySlotView => _inventorySlotViewPrefab;
        public InventoryItemView InventoryItemView => _inventoryItemViewPrefab;
    }
}