using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerDefinitionData", menuName = "Data/Entities/Characters/RaftPlayerDefinitionData")]
    public class RaftPlayerDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private RaftPlayerInteractSettings _interactSettings;
        [SerializeField] private RaftPlayerDropInventoryItemSettings _dropInventoryItemSettings;
        [SerializeField] private RaftPlayerAttackSettings _attackSettings;
        [SerializeField] private RaftPlayerTileTargetSettings _tileTargetSettings;

        public RaftPlayerInteractSettings InteractSettings => _interactSettings;
        public RaftPlayerDropInventoryItemSettings DropInventoryItemSettings => _dropInventoryItemSettings;
        public RaftPlayerAttackSettings AttackSettings => _attackSettings;
        public RaftPlayerTileTargetSettings TileTargetSettings => _tileTargetSettings;
    }
}