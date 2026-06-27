using ShinyOwl.Common.Structures;
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
        [SerializeField] private BoolGrid _unlockableInventoryLayout;
        [SerializeField] private BoolGrid _defaultUnlockedInventoryLayout;

        public RaftPlayerInteractSettings InteractSettings => _interactSettings;
        public RaftPlayerDropInventoryItemSettings DropInventoryItemSettings => _dropInventoryItemSettings;
        public RaftPlayerAttackSettings AttackSettings => _attackSettings;
        public RaftPlayerTileTargetSettings TileTargetSettings => _tileTargetSettings;
        public BoolGrid UnlockableInventoryLayout => _unlockableInventoryLayout;
        public BoolGrid DefaultUnlockedInventoryLayout => _defaultUnlockedInventoryLayout;
    }
}