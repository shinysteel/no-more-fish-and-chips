using NoMoreFishAndChips.States;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class TileDefeatModule : EntityDefeatModule
    {
        private Tile _tile;
        private TileDefeatSettings _settings;

        private GameplayContext _context;

        private float _sinkTimer;

        public TileDefeatModule(Tile tile, Func<bool> isDefeatedGetter, Action<bool> isDefeatedSetter) : base(tile, isDefeatedGetter, isDefeatedSetter)
        {
            _tile = tile;
            _settings = (TileDefeatSettings)tile.EntityDefinitionData.EntityDefeatSettings;

            if (_networkManager.IsServer)
            {
                _tile.EntityHealthModule.OnChanged += HandleHealthChanged;
            }
        }

        public void SetContext(GameplayContext context)
        {
            _context = context;
        }

        ~TileDefeatModule()
        {
            if (_networkManager.IsServer && _tile != null)
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

            if (!_networkManager.IsServer)
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

            _sinkTimer += Time.fixedDeltaTime;

            if (_sinkTimer >= _settings.Duration)
            {
                _context.Raft.RemoveNetTile(_tile.Cell);
                return;
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