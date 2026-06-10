using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using System.Collections.Generic;
using UnityEngine;
using EntityId = NoMoreFishAndChips.Entities.EntityId;
using System.Linq;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.Environments
{
    public class DrowningSpawner : GameplayBehaviour, IEntityManagerListener
    {
        private Dictionary<RaftPlayer, Drowning> _playerDrowningMap = new();

        protected override void OnSpawned()
        {
            _entityManager.AddListener(this);

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RemoveListener(this);
        }

        private void Update()
        {
            if (!isOwner)
            {
                return;
            }

            if (!_isInitialised)
            {
                return;
            }

            SpawnUpdate();
        }

        // Manage the collection of drownings based on players who are alive and in the water for long enough
        private void SpawnUpdate()
        {
            foreach (RaftPlayer player in _context.Players)
            {
                if (player.RaftPlayerPhysicsModule.TimeInWater >= 1f && !player.RaftPlayerDefeatModule.InBarrel)
                {
                    SpawnDrowning(player);
                }
            }
        }

        // Create a drowning to target a valid player if one doesn't already exist
        private void SpawnDrowning(RaftPlayer player)
        {
            if (_playerDrowningMap.ContainsKey(player))
            {
                return;
            }

            Drowning drowning = (Drowning)_entityManager.Spawn(EntityId.Drowning, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
            drowning.SetTargetPlayer(player);
            _playerDrowningMap.Add(player, drowning);
        }

        void IEntityManagerListener.OnEntityDespawned(IEntity entity)
        {
            if (!isOwner)
            {
                return;
            }

            // When a drowning despawns itself, we need to track that
            if (entity is Drowning drowning)
            {
                Utils.Collections.RemoveDictionaryKeys(_playerDrowningMap, (KeyValuePair<RaftPlayer, Drowning> kvp) => kvp.Value == drowning);
            }
        }
    }
}