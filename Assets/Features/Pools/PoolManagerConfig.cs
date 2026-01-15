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
        [SerializeField] private BlueprintEntry _blueprintEntryPrefab;
        [SerializeField] private RequirementEntry _requirementEntryPrefab;

        public RaftTile RaftTilePrefab => _raftTilePrefab;
        public LobbyEntry LobbyEntryPrefab => _lobbyEntryPrefab;
        public InventorySlotView InventorySlotViewPrefab => _inventorySlotViewPrefab;
        public InventoryItemView InventoryItemViewPrefab => _inventoryItemViewPrefab;
        public BlueprintEntry BlueprintEntryPrefab => _blueprintEntryPrefab;
        public RequirementEntry RequirementEntryPrefab => _requirementEntryPrefab;
    }
}