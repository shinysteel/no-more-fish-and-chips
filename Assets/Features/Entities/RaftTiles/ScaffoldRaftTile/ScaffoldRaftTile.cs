using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.UI;
using NUnit.Framework;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System.Collections.Generic;
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.Entities
{
    public class ScaffoldRaftTile : RaftTile, IBuildable
    {
        private EntityManager _entityManager;

        private SyncVar<EntityId> _netBuildId = new SyncVar<EntityId>(ownerAuth: true);
        private SyncVar<int> _netBuildRotations = new SyncVar<int>(ownerAuth: true);

        private List<Interaction> _interactions = new();
        public override List<Interaction> Interactions => _interactions;

        protected override void Awake()
        {
            base.Awake();

            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _interactions.Add(new BuildScaffoldRaftTileInteraction(this, _context));
        }

        public void SetNetBuildId(EntityId id)
        {
            _netBuildId.value = id;
        }

        public void SetNetBuildRotations(int rotations)
        {
            _netBuildRotations.value = rotations;
        }

        void IBuildable.Build()
        {
            RaftTile prefab = (RaftTile)_entityManager.GetPrefab(_netBuildId.value);

            _context.LocalPlayer.RaftPlayerActLogic.StartChannel(prefab.TileDefinitionData.BuildRecipe.Duration, ChannelAnimation.Build, () => _context.Raft.SetTileRpc(_netCell.value, _netBuildId.value, prefab.EntityDefinitionData.EntityHealthSettings.Health, _netBuildRotations.value));
        }
    }
}