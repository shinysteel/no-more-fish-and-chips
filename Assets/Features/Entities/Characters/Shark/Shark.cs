using FishFlingers.Environments;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Extensions;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using FishFlingers.Hitboxes;

namespace FishFlingers.Entities
{
    public class Shark : Character<SharkData>
    {
        private StateMachine<EState> _stateMachine;

        private RaftLine[] _targetLines = new RaftLine[2];
        private RaftLineNode[] _targetNodes = new RaftLineNode[2];
        private Direction _swimDirectionEnum;
        private int _swimDirectionFlat;
        private Vector3 _shiftDirection;

        private const string IsBitingBoolName = "IsBiting";

        private enum EState
        {
            None,
            Surface,
            Swim,
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
                if (!_shark._context.Raft.Queries.TryGetRandomLine((RaftLine line) => line.Nodes.Count > 0, out _shark._targetLines[0])
                    || !_shark._context.Raft.Queries.TryGetRandomAdjacentLine(_shark._targetLines[0], out _shark._targetLines[1], out int adjacentDirection))
                {
                    _shark._entityManager.Despawn(_shark);
                    return;
                }

                // Choose a random direction along the line, and select the edge with the bigge st index. Surface away from it 
                int edgeDirection = Random.value < 0.5f ? -1 : 1;
                RaftEdge[] edges = new RaftEdge[] { _shark._targetLines[0].GetEdge(edgeDirection), _shark._targetLines[1].GetEdge(edgeDirection) };
                RaftEdge furthestEdge = edges.OrderByDescending(edge => edge?.Node.AxisIndex ?? int.MinValue).First();
                _shark.transform.position = _shark._context.Raft.Queries.CellToWorldPosition(furthestEdge.Node.Cell) + Utils.Math.DirectionToVector3(furthestEdge.Direction) * Tile.Size * _surfaceOffset;

                // Store directions
                _shark._swimDirectionEnum = Utils.Math.FlipDirection(furthestEdge.Direction);
                _shark._swimDirectionFlat = -edgeDirection;

                // Sit in between the two lines
                _shark._shiftDirection = Utils.Math.DirectionToVector3(_shark._targetLines[0].RaftAxis.GetDirection()) * adjacentDirection;
                _shark.transform.position += _shark._shiftDirection * Tile.Size * 0.5f;

                // Face the raft, and submerge so that only the fin is showing
                _shark.transform.rotation = Quaternion.LookRotation(Utils.Math.DirectionToVector3(_shark._swimDirectionEnum), Vector3.up);
                _shark.transform.position += Vector3.down * _submergeDistance;

                // Tween a surface animation where the shark comes from below
                Vector3 startPosition = _shark.transform.position + Vector3.down * _surfaceDistance;
                Tween.Position(_shark.transform, startValue: startPosition, endValue: _shark.transform.position, duration: _surfaceDuration, ease: Ease.OutBack)
                    .OnComplete(() => _parentStateMachine.ChangeState(EState.Swim));
            }
        }

        /// <summary>
        /// Keep swimming forward until a tile is encountered or the Shark is too far from the Raft
        /// </summary>
        private class SwimState : State
        {
            private float _swimSpeed = 0.5f;
            private float _despawnDistance = 10f;

            public SwimState(StateMachine<EState> parent) : base(parent)
            { }
            
            public override void Tick()
            {
                if (_shark._stunLogic.IsStunned)
                {
                    return;
                }

                // Retrieve the next nodes in the lines
                int axisIndex = _shark._targetLines[0].RaftAxis.WorldPositionToAxisIndex(_shark.transform.position);
                for (int i = 0; i < _shark._targetLines.Length; i++)
                {
                    _shark._targetNodes[i] = _shark._targetLines[i].GetNextNode(axisIndex, _shark._swimDirectionFlat);
                }

                // Move to the next node from either line, and stop in front of it. If there is nno next node, keep swimming forward
                Vector3 targetPosition = GetTargetPosition(axisIndex);
                _shark.transform.position = Vector3.MoveTowards(_shark.transform.position, targetPosition, _swimSpeed * Time.deltaTime);
                
                if (_shark.transform.position.magnitude > _despawnDistance)
                {
                    _shark._entityManager.Despawn(_shark);
                    return;
                }

                // When close enough, move to bite state
                if (Vector3.Distance(_shark.transform.position, targetPosition) < 0.01f)
                {
                    _parentStateMachine.ChangeState(EState.Bite);
                }
            }

            private Vector3 GetTargetPosition(int axisIndex)
            {
                RaftLineNode closestNode = _shark._targetNodes
                    .Where(node => node != null)
                    .OrderBy(node => Mathf.Abs(node.AxisIndex - axisIndex))
                    .FirstOrDefault();
                
                Tile closestTile = null;
                if (closestNode != null)
                {
                    _shark._context.Raft.Tiles.TryGetValue(closestNode.Cell, out closestTile);
                }

                Vector3 targetPosition = closestTile != null
                    ? closestTile.transform.position - Utils.Math.DirectionToVector3(_shark._swimDirectionEnum) + _shark._shiftDirection * Tile.Size * 0.5f
                    : _shark.transform.position + Utils.Math.DirectionToVector3(_shark._swimDirectionEnum);

                targetPosition.y = _shark.transform.position.y;

                return targetPosition;
            }
        }

        /// <summary>
        /// Channel and perform a bite in a 2x1 space in front of the Shark
        /// </summary>
        private class BiteState : State
        {
            private float _biteTimer;
            private float _biteInterval = 3f;

            private float _cooldownTimer;
            private float _cooldownDuration = 1f;

            private float _tweenDuration = 0.5f;

            private Vector3 _startPosition;
            private Quaternion _startRotation;
            private Sequence _transitionSequence;

            private int? _markerId;

            public BiteState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _cooldownTimer = _cooldownDuration;

                _startPosition = _shark.transform.position;
                _startRotation = _shark.transform.rotation;

                Vector3 bitePosition = _shark.transform.position + _shark.transform.forward * 0.1f + Vector3.up * 0.2f;

                Tween.Position(_shark.transform, endValue: bitePosition, duration: _tweenDuration);
                TweenExtensions.Rotation(_shark.transform, endValue: _shark.transform.rotation * Quaternion.AngleAxis(-30f, Vector3.right), duration: _tweenDuration, ease: Ease.OutQuad);

                _shark.CharacterModel.Animator.SetBool(IsBitingBoolName, true);

            }

            public override void Tick()
            {
                if (_transitionSequence.isAlive)
                {
                    return;
                }

                if (_shark._stunLogic.IsStunned)
                {
                    RemoveMarker();
                    _cooldownTimer = 0f;
                    _biteTimer = 0f;
                    return;
                }

                List<Tile> tiles = ListPool<Tile>.Get();

                try
                {
                    foreach (RaftLineNode node in _shark._targetNodes)
                    {
                        if (node != null && _shark._context.Raft.Tiles.TryGetValue(node.Cell, out Tile tile))
                        {
                            tiles.Add(tile);
                        }
                    }

                    if (tiles.Count == 0)
                    {
                        RemoveMarker();
                        TransitionToSwim();
                        return;
                    }

                    if (_cooldownTimer < _cooldownDuration)
                    {
                        _cooldownTimer += Time.deltaTime;
                        return;
                    }

                    int axisIndex = _shark._targetLines[0].RaftAxis.WorldPositionToAxisIndex(_shark.transform.position);

                    _markerId ??= _shark._context.EnvironmentMarker.AddNetMarkedCells(_shark._targetLines.Select(line => line.AxisIndexToCell(axisIndex + 1 * _shark._swimDirectionFlat)).ToArray());                    

                    if (_biteTimer < _biteInterval)
                    {
                        _biteTimer += Time.deltaTime;
                        return;
                    }

                    _cooldownTimer -= _cooldownDuration;
                    _biteTimer -= _biteInterval;

                    Vector3 hitboxDirection = Utils.Math.DirectionToVector3(_shark._swimDirectionEnum);
                    Vector3 hitboxPosition = _shark._targetLines[0].AxisIndexToWorldPosition(axisIndex) + _shark._shiftDirection * Tile.Size * 0.5f + hitboxDirection;
                    Hitbox hitbox = _shark._poolManager.GetPoolable<Hitbox>(new SpawnParams() { Position = hitboxPosition, Rotation = Quaternion.LookRotation(hitboxDirection, Vector3.up) });
                    hitbox.Initialise(_shark.Data.BiteHitboxData);

                    RemoveMarker();
                }
                finally
                {
                    ListPool<Tile>.Release(tiles);
                }
            }

            private void TransitionToSwim()
            {
                _shark.CharacterModel.Animator.SetBool(IsBitingBoolName, false);

                _transitionSequence = Sequence.Create();
                _transitionSequence.Chain(Tween.Position(_shark.transform, endValue: _startPosition, duration: _tweenDuration));
                _transitionSequence.Group(TweenExtensions.Rotation(_shark.transform, endValue: _startRotation, duration: _tweenDuration, ease: Ease.OutQuad));
                _transitionSequence.OnComplete(() => _parentStateMachine.ChangeState(EState.Swim));
            }

            private void RemoveMarker()
            {
                if (_markerId.HasValue)
                {
                    _shark._context.EnvironmentMarker.RemoveNetMarkedCells(_markerId.Value);
                    _markerId = null;
                }
            }

            public override void Exit()
            {
                RemoveMarker();

                _biteTimer = 0f;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            _stateMachine = new();

            SurfaceState surfaceState = new SurfaceState(_stateMachine);
            SwimState swimState = new SwimState(_stateMachine);
            BiteState biteState = new BiteState(_stateMachine);

            surfaceState.Initialise(this);
            swimState.Initialise(this);
            biteState.Initialise(this);
            
            _stateMachine.AddState(EState.Surface, surfaceState);
            _stateMachine.AddState(EState.Swim, swimState);
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