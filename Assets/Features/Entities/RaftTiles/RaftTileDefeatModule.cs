using NoMoreFishAndChips.States;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftTileDefeatModule : EntityDefeatModule
    {
        private RaftTile _tile;
        private RaftTileDefeatSettings _settings;

        private GameplayContext _context;

        private float _sinkTimer;

        public RaftTileDefeatModule(RaftTile tile, Func<bool> isDefeatedGetter, Action<bool> isDefeatedSetter) : base(tile, isDefeatedGetter, isDefeatedSetter)
        {
            _tile = tile;
            _settings = (RaftTileDefeatSettings)tile.EntityDefinitionData.EntityDefeatSettings;

            _tile.EntityHealthModule.OnChanged += HandleHealthChanged;
        }

        public void SetContext(GameplayContext context)
        {
            _context = context;
        }

        ~RaftTileDefeatModule()
        {
            if (_tile != null)
            {
                _tile.EntityHealthModule.OnChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(int previous, int current)
        {
            if (!_isDefeatedGetter())
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
            if (_context == null)
            {
                return;
            }

            SinkFixedTick();
        }
        
        private void SinkFixedTick()
        {
            if (!_isDefeatedGetter())
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

            _tile.EntityPhysicsModule.Rigidbody.MovePosition(_tile.EntityPhysicsModule.Rigidbody.position + Vector3.down * _settings.Speed * Time.fixedDeltaTime);
        }

        public override void HandleIsDefeatedChanged(bool defeated)
        {
            RaiseIsDefeatedChanged();

            _sinkTimer = 0f;
        }
    }
}