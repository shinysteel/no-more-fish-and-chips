using NoMoreFishAndChips.Networking;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    public class LobbyState : GameplaySubState
    {
        private NetworkManager _networkManager;

        private LobbyStateConfig _config;

        public LobbyState(StateMachine<EGameplayState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        public override void Initialise(StateManagerConfig config)
        {
            _config = config.GameplayStateConfig.LobbyStateConfig;
        }

        public override void Enter()
        {
            if (_networkManager.IsServer)
            {
                Vector2Int cell = _context.Raft.Queries.Axes[Axis.Vertical].MinLine.MinEdge.Node.Cell;
                Vector3 position = _context.Raft.Queries.CellToWorldPosition(cell) + Vector3.left * 6f;

                _networkManager.Spawn(_config.IslandPrefab, new SpawnParams() { Position = position });
            }
        }
    }
}