using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftTileDefeatLogic : EntityDefeatLogic
    {
        private RaftTile _tile;
        private RaftTileDefeatSettings _settings;

        private float _sinkTimer;

        public RaftTileDefeatLogic(RaftTile tile, SyncVar<bool> netIsDefeated) : base(tile, netIsDefeated)
        {
            _tile = tile;

            _settings = (RaftTileDefeatSettings)_tile.EntityDefinitionData.EntityDefeatSettings;
            
            _tile.EntityHealthLogic.OnChanged += HandleHealthChanged;
        }

        public override void Dispose()
        {
            if (_tile != null)
            {
                _tile.EntityHealthLogic.OnChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(int previous, int current)
        {
            if (!_netIsDefeated.value)
            {
                return;
            }

            if (current > 0)
            {
                SetIsDefeated(false);
            }
        }

        public override void FixedTick()
        {
            SinkFixedTick();
        }
        
        private void SinkFixedTick()
        {
            if (!_netIsDefeated.value)
            {
                return;
            }

            // The server will remove the tile after sinking long enough
            if (_networkManager.IsServer)
            {
                _sinkTimer += Time.fixedDeltaTime;

                if (_sinkTimer >= _settings.Duration)
                {
                    _context.Raft.RemoveNetRaftTile(_tile.Cell);
                    return;
                }
            }

            _tile.EntityPhysicsLogic.Rigidbody.MovePosition(_tile.EntityPhysicsLogic.Rigidbody.position + Vector3.down * _settings.Speed * Time.fixedDeltaTime);
        }

        protected override void HandleNetIsDefeatedChanged(bool defeated)
        {
            RaiseIsDefeatedChanged();

            _sinkTimer = 0f;
        }
    }
}