using FishFlingers.Environments;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class Shark : Character<SharkData>
    {
        private StateMachine<EState> _stateMachine;

        private RaftLine _targetLine;
        private Vector2Int _lineDirection;

        private Vector3 _swimDirection;

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

        // Appear from the water
        private class SurfaceState : State
        {
            public SurfaceState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                // Find a target edge
                if (!_shark._context.Raft.Queries.TryGetRandomLine(out _shark._targetLine))
                {
                    _shark._entityManager.Despawn(_shark);
                    return;
                }

                RaftEdge edge = Random.value < 0.5f ? _shark._targetLine.MinEdge : _shark._targetLine.MaxEdge;
                _shark._lineDirection = -edge.CellDirection;

                // Start away from the edge
                float edgeDistance = Tile.Size * 8;
                _shark.transform.position = _shark._context.Raft.Queries.CellToWorldPosition(edge.Tile.Cell) + edge.WorldDirection * edgeDistance;

                // Shift either left or right by half a tile
                int shiftSign = Random.value < 0.5f ? 1 : -1;
                Vector3 shiftDirection = Vector3.Cross(edge.WorldDirection, Vector3.up) * shiftSign;
                float shiftDistance = Tile.Size * 0.5f;
                _shark.transform.position += shiftDirection * shiftDistance;

                // Have the shark submerged so that only its fin is showing
                float submergeDistance = 0.4f;
                _shark.transform.position += Vector3.down * submergeDistance;

                // Face the raft
                _shark._swimDirection = -edge.WorldDirection;
                _shark.transform.rotation = Quaternion.LookRotation(_shark._swimDirection, Vector3.up);

                // Play a surface animation, coming from below
                float surfaceDistance = 0.5f;
                float surfaceDuration = 1f;
                Vector3 startPosition = _shark.transform.position + Vector3.down * surfaceDistance;

                Tween.Position(_shark.transform, startValue: startPosition, endValue: _shark.transform.position, duration: surfaceDuration, ease: Ease.OutBack)
                    .OnComplete(() => _parentStateMachine.ChangeState(EState.Approach));
            }
        }

        // Approach the raft
        private class SwimState : State
        {
            public SwimState(StateMachine<EState> parent) : base(parent)
            { }
            
            public override void Tick()
            {
                Vector3 targetPosition = _shark.transform.position + _shark._swimDirection * Mathf.Infinity;

                

                float swimSpeed = 0.5f;
                _shark.transform.position += _shark._swimDirection * swimSpeed * Time.deltaTime;
            }
        }

        // Bite the raft
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