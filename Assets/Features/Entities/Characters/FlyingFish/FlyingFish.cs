using FishFlingers.Environments;
using PurrNet;
using ShinyOwl.Common.Framework;
using UnityEngine;
using ShinyOwl.Common.Utils;
using PrimeTween;
using ShinyOwl.Common.Extensions;

namespace FishFlingers.Entities
{
    public class FlyingFish : NetEntity
    {
        private StateMachine<EState> _stateMachine;

        private Tile _targetTile;

        private enum EState
        {
            None,
            Scout,
            Fly
        }

        private class ScoutState : State<EState, ENone>
        {
            private FlyingFish _flyingFish;

            private float _scoutTimer;

            private const float ScoutDuration = 1.5f;

            public ScoutState(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(FlyingFish fish)
            {
                _flyingFish = fish;
            }

            public override void Enter()
            {
                _scoutTimer = 0f;

                // Choose a tile to target
                if (!_flyingFish._context.Raft.TryGetRandomTile(out _flyingFish._targetTile))
                {
                    _flyingFish._networkManager.Despawn(_flyingFish);
                    return;
                }

                // Choose a position to scout from
                int scoutOffset = 3;
                if (!_flyingFish._context.Raft.TryGetClosestEdge(_flyingFish._targetTile.Cell, out RaftEdge edge))
                {
                    _flyingFish._networkManager.Despawn(_flyingFish);
                    return;
                }

                _flyingFish.transform.position = _flyingFish._context.Raft.CellToWorldPosition(edge.Tile.Cell + edge.Direction2D * scoutOffset);

                // Face towards the raft, with a slight tilt up
                float scoutTilt = 15f;
                _flyingFish.transform.rotation = Quaternion.LookRotation(edge.Direction3D);
                _flyingFish.transform.rotation = Quaternion.AngleAxis(scoutTilt, _flyingFish.transform.right) * _flyingFish.transform.rotation;

                // Animate from underwater to surface
                float surfaceDuration = 0.5f;
                _flyingFish.transform.position += Vector3.down;
                Tween.Position(_flyingFish.transform, _flyingFish.transform.position + Vector3.up, surfaceDuration, Ease.OutBack);
            }

            public override void Update()
            {
                _scoutTimer += Time.deltaTime;

                // Scout for some time before attacking
                if (_scoutTimer < ScoutDuration)
                {
                    return;
                }

                _parentStateMachine.ChangeState(EState.Fly);
            }
        }

        private class FlyState : State<EState, ENone>
        {
            private FlyingFish _flyingFish;

            private Vector3 _anticipatePosition;
            private Quaternion _anticipateRotation;

            private Vector3 _landPosition;
            private Quaternion _landRotation;

            private bool _isAnticipating;
            private float _flyTimer;

            private const float FlyDuration = 1.25f;
            private const float LaunchAngle = 75f;

            public FlyState(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(FlyingFish fish)
            {
                _flyingFish = fish;
            }

            public override void Enter()
            {
                _isAnticipating = true;
                Vector3 anticipateOffset = Vector3.down * 0.2f;
                float anticipateDuration = 0.2f;

                _anticipatePosition = _flyingFish.transform.position + anticipateOffset;

                // Match the launch angle
                _anticipateRotation = Quaternion.AngleAxis(LaunchAngle, _flyingFish.transform.right) * _flyingFish.transform.rotation;

                // Anticipate with a small duck
                Sequence.Create()
                    .Group(Tween.Position(_flyingFish.transform, _anticipatePosition, anticipateDuration, Ease.OutQuad))
                    .Group(TweenExtensions.Rotate(_flyingFish.transform, _anticipateRotation, anticipateDuration, Ease.OutQuad))
                    .OnComplete(() => _isAnticipating = false);

                _landPosition = _flyingFish._targetTile.transform.position;

                // Straight down
                _landRotation = Quaternion.AngleAxis(-90f, _flyingFish.transform.right) * _flyingFish.transform.rotation;
            }
            
            public override void Update()
            {
                if (_isAnticipating)
                {
                    return;
                }

                _flyTimer += Time.deltaTime;

                // Interpolate from start to end
                float time = _flyTimer / FlyDuration;
                _flyingFish.transform.position = Utils.Physics.GetProjectilePosition(_anticipatePosition, _landPosition, -Physics.gravity.y, LaunchAngle, time);
                _flyingFish.transform.rotation = Quaternion.Slerp(_anticipateRotation, _landRotation, time);

                if (_flyTimer > FlyDuration)
                {
                    _flyingFish._context.Raft.ChangeNetTileHealth(_flyingFish._targetTile.Cell, -1);

                    _flyingFish._networkManager.Despawn(_flyingFish);
                }
            }
        }

        protected override void OnInitializeModules()
        {
            base.OnInitializeModules();

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

            if (!isServer)
            {
                return;
            }

            _stateMachine.ChangeState(EState.Scout);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _targetTile = null;

            if (_stateMachine.CurrentEnum != EState.None)
            {
                _stateMachine.ChangeState(EState.None);
            }
        }

        private void Update()
        {
            if (!isServer)
            {
                return;
            }

            _stateMachine.Update();
        }
    }
}