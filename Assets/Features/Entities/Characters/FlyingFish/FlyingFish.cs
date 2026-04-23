using FishFlingers.Environments;
using PurrNet;
using ShinyOwl.Common.Framework;
using UnityEngine;
using ShinyOwl.Common.Utils;
using PrimeTween;
using ShinyOwl.Common.Extensions;
using ShinyOwl.Common;
using FishFlingers.Effects;

namespace FishFlingers.Entities
{
    public class FlyingFish : Character<FlyingFishData>
    {
        private StateMachine<EState> _stateMachine;

        private Tile _targetTile;

        private int _tileMarkId = -1;

        private const string IsFlyingBoolName = "IsFlying";

        private enum EState
        {
            None,
            Scout,
            Fly
        }

        private class State : State<EState, ENone>
        {
            protected FlyingFish _flyingFish;

            public State(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(FlyingFish flyingFish)
            {
                _flyingFish = flyingFish;
            }
        }

        // Surface and scout a tile on the raft
        private class ScoutState : State
        {
            private float _scoutTimer;

            public ScoutState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _scoutTimer = 0f;

                // Choose a tile to target and a position to scout from
                if (!_flyingFish._context.Raft.Queries.TryGetRandomTile(out _flyingFish._targetTile) 
                    || !_flyingFish._context.Raft.Queries.TryGetClosestEdge(_flyingFish._targetTile.Cell, out RaftEdge edge))
                {
                    _flyingFish._entityManager.Despawn(_flyingFish);
                    return;
                }

                int scoutOffset = 3;
                _flyingFish.transform.position = _flyingFish._context.Raft.Queries.CellToWorldPosition(edge.Tile.Cell + edge.CellDirection * scoutOffset);

                // Rest slightly in the water
                float restDistance = 0.05f;
                _flyingFish.transform.position += Vector3.down * restDistance;
                
                // Face towards the raft, with a slight tilt up
                float scoutTilt = 15f;
                _flyingFish.transform.rotation = Quaternion.LookRotation(edge.WorldDirection);
                _flyingFish.transform.rotation = Quaternion.AngleAxis(scoutTilt, _flyingFish.transform.right) * _flyingFish.transform.rotation;

                // Animate from underwater to surface
                float surfaceDuration = 0.5f;
                float surfaceDistance = 0.5f;
                _flyingFish.transform.position += Vector3.down * surfaceDistance;
                Tween.Position(_flyingFish.transform, _flyingFish.transform.position + Vector3.up * surfaceDistance, surfaceDuration, Ease.OutBack);

                // Place a marker
                _flyingFish._tileMarkId = _flyingFish._context.TileMarker.AddNetMarkedCell(new NetTileMark(_flyingFish._targetTile.Cell, TileMarkShape.Single));
            }

            public override void Tick()
            {
                _scoutTimer += Time.deltaTime;

                // Scout for some time before attacking
                if (_scoutTimer < _flyingFish.Data.ScoutDuration)
                {
                    return;
                }

                _parentStateMachine.ChangeState(EState.Fly);
            }
        }

        // Fly into a tile on the raft
        private class FlyState : State
        {
            private Vector3 _anticipatePosition;
            private Quaternion _anticipateRotation;

            private Vector3 _landPosition;
            private Quaternion _landRotation;

            private bool _isAnticipating;
            private float _flyTimer;

            public FlyState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _flyTimer = 0f;
                
                _isAnticipating = true;
                Vector3 anticipateOffset = Vector3.down * 0.2f;
                float anticipateDuration = 0.2f;

                _anticipatePosition = _flyingFish.transform.position + anticipateOffset;

                // Match the launch angle
                _anticipateRotation = Quaternion.AngleAxis(_flyingFish.Data.LaunchAngle, _flyingFish.transform.right) * _flyingFish.transform.rotation;

                // Anticipate with a small duck
                Sequence.Create()
                    .Group(Tween.Position(_flyingFish.transform, _anticipatePosition, anticipateDuration, Ease.OutQuad))
                    .Group(TweenExtensions.Rotate(_flyingFish.transform, _anticipateRotation, anticipateDuration, Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        _isAnticipating = false;
                        _flyingFish.CharacterModel.Animator.SetBool(IsFlyingBoolName, true);
                    });

                _landPosition = _flyingFish._targetTile.transform.position;

                // Straight down
                _landRotation = Quaternion.AngleAxis(-90f, _flyingFish.transform.right) * _flyingFish.transform.rotation;
            }
            
            public override void Tick()
            {
                if (_isAnticipating)
                {
                    return;
                }

                _flyTimer += Time.deltaTime;

                // Interpolate from start to end
                float time = _flyTimer / _flyingFish.Data.FlyDuration;
                _flyingFish.transform.position = Utils.Physics.GetProjectilePosition(_anticipatePosition, _landPosition, Physics.gravity.magnitude, _flyingFish.Data.LaunchAngle, time);
                _flyingFish.transform.rotation = Quaternion.Slerp(_anticipateRotation, _landRotation, time);

                if (_flyTimer > _flyingFish.Data.FlyDuration)
                {
                    _flyingFish._context.Raft.ChangeNetTileHealth(_flyingFish._targetTile.Cell, -1);

                    _flyingFish._entityManager.Despawn(_flyingFish);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            ScoutState scoutState = new ScoutState(_stateMachine);
            FlyState flyState = new FlyState(_stateMachine);

            scoutState.Initialise(this);
            flyState.Initialise(this);

            _stateMachine.AddState(EState.Scout, scoutState);
            _stateMachine.AddState(EState.Fly, flyState);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isServer)
            {
                _defeatLogic.OnDefeated += HandleDefeated;

                _stateMachine.ChangeState(EState.Scout);
            }
        }

        protected override void OnDespawned()
        {
            if (isServer)
            {
                Cleanup();

                _defeatLogic.OnDefeated -= HandleDefeated;
            }

            base.OnDespawned();
        }

        protected override void Update()
        {
            base.Update();
            
            if (!isServer)
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
            _targetTile = null;

            if (_tileMarkId >= 0)
            {
                _context.TileMarker.RemoveNetMarkedCell(_tileMarkId);
                _tileMarkId = -1;
            }

            CharacterModel.Animator.SetBool(IsFlyingBoolName, false);

            // Cleanup will always happen on Despawn, but can also happen when Defeated
            if (_stateMachine.CurrentEnum != EState.None)
            {
                _stateMachine.ChangeState(EState.None);
            }
        }
    }
}