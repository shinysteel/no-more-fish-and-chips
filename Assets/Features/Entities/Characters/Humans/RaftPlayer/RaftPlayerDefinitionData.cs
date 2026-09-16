using ShinyOwl.Common.Structures;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerDefinitionData", menuName = "Data/Entities/Characters/RaftPlayer/RaftPlayerDefinitionData")]
    public class RaftPlayerDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private RaftPlayerInteractSettings _interactSettings;
        [SerializeField] private RaftPlayerDropInventoryItemSettings _dropInventoryItemSettings;
        [SerializeField] private RaftPlayerAttackSettings _attackSettings;
        [SerializeField] private RaftPlayerBuildSettings _buildSettings;
        [SerializeField] private RaftPlayerContextSettings _contextSettings;
        [SerializeField] private BoolGrid _unlockableInventoryLayout;
        [SerializeField] private BoolGrid _defaultUnlockedInventoryLayout;

        public RaftPlayerInteractSettings InteractSettings => _interactSettings;
        public RaftPlayerDropInventoryItemSettings DropInventoryItemSettings => _dropInventoryItemSettings;
        public RaftPlayerAttackSettings AttackSettings => _attackSettings;
        public RaftPlayerBuildSettings BuildSettings => _buildSettings;
        public RaftPlayerContextSettings ContextSettings => _contextSettings;
        public BoolGrid UnlockableInventoryLayout => _unlockableInventoryLayout;
        public BoolGrid DefaultUnlockedInventoryLayout => _defaultUnlockedInventoryLayout;

        public RaftPlayerDefeatSettings RaftPlayerDefeatSettings => (RaftPlayerDefeatSettings)_entityDefeatSettings;
        public RaftPlayerPhysicsSettings RaftPlayerPhysicsSettings => (RaftPlayerPhysicsSettings)_entityPhysicsSettings;
    }
}