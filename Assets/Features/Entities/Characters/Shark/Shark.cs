using FishFlingers.Environments;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System.Linq;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class Shark : Character<SharkData>
    {
        private StateMachine<EState> _stateMachine;

        private RaftLine[] _targetLines = new RaftLine[2];
        private Direction _swimDirection;
        private int _flatSwimDirection;

        private enum EState
        {
            None,
            Surface,
            Approach,
            Bite
        }

        private class State : State<EState, ENone>
        {
            protected Shark _shark;

            public State(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(Shark shark)
            {
                _shark = shark;
            }
        }

        /// <summary>
        /// Surface far away from the raft and do setup for how the Shark will attack the Raft
        /// </summary>
        private class SurfaceState : State
        {
            private int _surfaceOffset = 8;
            private float _submergeDistance = 0.4f;
            private float _surfaceDistance = 0.5f;
            private float _surfaceDuration = 1f;

            public SurfaceState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                // Retrieve two lines to swim along
                if (!_shark._context.Raft.Queries.TryGetRandomLine((RaftLine line) => line.LineTiles.Count > 0, out _shark._targetLines[0])
                    || !_shark._context.Raft.Queries.TryGetRandomAdjacentLine(_shark._targetLines[0], out _shark._targetLines[1], out int adjacentDirection))
                {
                    _shark._entityManager.Despawn(_shark);
                    return;
                }

                // Choose a random direction along the line, and select the edge with the biggest index. Surface away from it 
                int edgeDirection = Random.value < 0.5f ? -1 : 1;
                RaftEdge[] edges = new RaftEdge[] { _shark._targetLines[0].GetEdge(edgeDirection), _shark._targetLines[1].GetEdge(edgeDirection) };
                RaftEdge furthestEdge = edges.OrderByDescending(edge => edge?.LineTile.AxisIndex ?? int.MinValue).First();
                _shark.transform.position = _shark._context.Raft.Queries.CellToWorldPosition(furthestEdge.LineTile.Tile.Cell) + Utils.Math.DirectionToVector3(furthestEdge.Direction) * Tile.Size * _surfaceOffset;
                _shark._flatSwimDirection = -edgeDirection;

                // Sit in between the two lines
                _shark._swimDirection = Utils.Math.FlipDirection(furthestEdge.Direction);
                Vector3 shiftDirection = Utils.Math.DirectionToVector3(Utils.Math.PerpendicularDirection(_shark._swimDirection, true)) * adjacentDirection;
                _shark.transform.position += shiftDirection * Tile.Size * 0.5f;

                // Face the raft, and submerge so that only the fin is showing
                _shark.transform.rotation = Quaternion.LookRotation(Utils.Math.DirectionToVector3(_shark._swimDirection), Vector3.up);
                _shark.transform.position += Vector3.down * _submergeDistance;

                // Tween a surface animation where the shark comes from below
                Vector3 startPosition = _shark.transform.position + Vector3.down * _surfaceDistance;
                Tween.Position(_shark.transform, startValue: startPosition, endValue: _shark.transform.position, duration: _surfaceDuration, ease: Ease.OutBack)
                    .OnComplete(() => _parentStateMachine.ChangeState(EState.Approach));
            }
        }

        /// <summary>
        /// Keep swimming forward until a tile is encountered or the Shark is too far from the Raft
        /// </summary>
        private class SwimState : State
        {
            private float _swimSpeed = 0.5f;
            private RaftLineTile[] _lineTiles = new RaftLineTile[2];

            public SwimState(StateMachine<EState> parent) : base(parent)
            { }
            
            public override void Tick()
            {
                // Determine our axis position
                Vector2Int cell = _shark._context.Raft.Queries.WorldPositionToCell(_shark.transform.position);
                int axisIndex = _shark._targetLines[0].RaftAxis.GetAxisIndex(cell);

                for (int i = 0; i < _shark._targetLines.Length; i++)
                {
                    _lineTiles[i] = _shark._targetLines[i].GetNextLineTile(axisIndex, _shark._flatSwimDirection);
                }

                // Determine which tile in the RaftLine is 'next' given our position and direction
                RaftLineTile closestLineTile = _lineTiles.OrderBy(lineTile => Mathf.Abs((lineTile?.AxisIndex ?? int.MaxValue) - axisIndex)).First();

                // Move forward until we are 1 unit away from it
                Vector3 targetPosition = closestLineTile.Tile.transform.position - Utils.Math.DirectionToVector3(_shark._swimDirection);
                targetPosition.y = _shark.transform.position.y;
                if (_shark._targetLines[0].RaftAxis.Type == Axis.Horizontal)
                {
                    targetPosition.z = _shark.transform.position.z;
                }
                else
                {
                    targetPosition.x = _shark.transform.position.x;
                }

                _shark.transform.position = Vector3.MoveTowards(_shark.transform.position, targetPosition, _swimSpeed * Time.deltaTime);

                // Move to bite state
                if (Vector3.Distance(_shark.transform.position, targetPosition) < 0.01f)
                {
                    _parentStateMachine.ChangeState(EState.Bite);
                }
            }
        }

        /// <summary>
        /// Channel and perform a bite in a 2x1 space in front of the Shark
        /// </summary>
        private class BiteState : State
        {
            public BiteState(StateMachine<EState> parent) : base(parent)
            { }
        }

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            SurfaceState surfaceState = new SurfaceState(_stateMachine);
            SwimState approachState = new SwimState(_stateMachine);
            BiteState biteState = new BiteState(_stateMachine);

            surfaceState.Initialise(this);
            approachState.Initialise(this);
            biteState.Initialise(this);
            
            _stateMachine.AddState(EState.Surface, surfaceState);
            _stateMachine.AddState(EState.Approach, approachState);
            _stateMachine.AddState(EState.Bite, biteState);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner)
            {
                _defeatLogic.OnDefeated += HandleDefeated;

                _stateMachine.ChangeState(EState.Surface);
            }
        }

        protected override void OnDespawned()
        {
            if (isOwner)
            {
                Cleanup();

                _defeatLogic.OnDefeated -= HandleDefeated;
            }

            base.OnDespawned();
        }

        protected override void Update()
        {
            base.Update();

            if (!isOwner)
            {
                return;
            }

            _stateMachine.Tick();
        }

        private void HandleDefeated()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_stateMachine.CurrentEnum != EState.None)
            {
                _stateMachine.ChangeState(EState.None);
            }
        }
    }
}