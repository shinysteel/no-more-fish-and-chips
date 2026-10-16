using ShinyOwl.Common.Structures;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerDefinitionData", menuName = "Data/Entities/Characters/RaftPlayer/RaftPlayerDefinitionData")]
    public class RaftPlayerDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private RaftPlayerInteractSharedSettings _interactSharedSettings;
        [SerializeField] private RaftPlayerDropInventoryItemSharedSettings _dropInventoryItemSharedSettings;
        [SerializeField] private RaftPlayerAttackSharedSettings _attackSharedSettings;
        [SerializeField] private RaftPlayerBuildSharedSettings _buildSharedSettings;
        [SerializeField] private RaftPlayerContextActionsSharedSettings _contextActionsSharedSettings;
        [SerializeField] private BoolGrid _unlockableInventoryLayout;
        [SerializeField] private BoolGrid _defaultUnlockedInventoryLayout;

        public RaftPlayerInteractSharedSettings InteractSharedSettings => _interactSharedSettings;
        public RaftPlayerDropInventoryItemSharedSettings DropInventoryItemSharedSettings => _dropInventoryItemSharedSettings;
        public RaftPlayerAttackSharedSettings AttackSharedSettings => _attackSharedSettings;
        public RaftPlayerBuildSharedSettings BuildSharedSettings => _buildSharedSettings;
        public RaftPlayerContextActionsSharedSettings ContextActionsSharedSettings => _contextActionsSharedSettings;
        public BoolGrid UnlockableInventoryLayout => _unlockableInventoryLayout;
        public BoolGrid DefaultUnlockedInventoryLayout => _defaultUnlockedInventoryLayout;

        public RaftPlayerDefeatSharedSettings RaftPlayerDefeatSharedSettings => (RaftPlayerDefeatSharedSettings)_entityDefeatSharedSettings;
        public RaftPlayerPhysicsSharedSettings RaftPlayerPhysicsSharedSettings => (RaftPlayerPhysicsSharedSettings)_entityPhysicsSharedSettings;
    }
}