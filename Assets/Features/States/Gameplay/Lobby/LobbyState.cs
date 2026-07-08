using NoMoreFishAndChips.Entities;
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

        private float _startTimer = 0f;
        private float _startDuration = 3f;

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

        public override void Tick()
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            StartTick();
        }

        private void StartTick()
        {
            // Start counting down once all players are on the raft
            bool canStart = true;

            foreach (RaftPlayer player in _context.Players)
            {
                if (!player.RaftPlayerPhysicsModule.OnRaft)
                {
                    canStart = false;
                    break;
                }
            }

            if (!canStart)
            {
                _startTimer = 0f;
                return;
            }

            _startTimer += Time.deltaTime;

            if (_startTimer >= _startDuration)
            {
                _parentStateMachine.ChangeState(EGameplayState.Stage);
            }
        }
    }
}