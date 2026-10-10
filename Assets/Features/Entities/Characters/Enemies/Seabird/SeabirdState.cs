using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Entities
{
    public enum ESeabirdState
    {
        None,
        Arrive,
        Air,
        Ground,
        Water,
        Stun
    }

    public interface ISeabirdState
    {
        void InitialiseContext(GameplayContext context);
    }

    public abstract class SeabirdState<T> : State<ESeabirdState, T>, ISeabirdState where T : Enum
    {
        protected Seabird _seabird;
        protected GameplayContext _context;

        public SeabirdState(StateMachine<ESeabirdState> parent, Seabird seabird) : base(parent)
        {
            _seabird = seabird;
        }

        public virtual void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class SeabirdArriveState : SeabirdState<ENone>
    {
        private SeabirdArriveSettings _settings;

        public SeabirdArriveState(StateMachine<ESeabirdState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.ArriveSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _context.Raft.Queries.TryGetRandomTile(_ => true, out RaftTile tile);

            Vector3 position = tile.transform.position;
            position += new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f);
            position += Vector3.up * _settings.StartAltitude;

            _seabird.EntityPhysicsLogic.Rigidbody.position = position;

            _seabird.EntityPhysicsLogic.Rigidbody.linearVelocity = Vector3.zero;
        }
        
        public override void FixedTick()
        {
            base.FixedTick();

            if (_stateTimer < _settings.Delay)
            {
                return;
            }

            // Float down
            _seabird.StabiliseAltitude(_settings.DampingStrength);

            if (_stateTimer >= _settings.Duration)
            {
                _parentStateMachine.ChangeState(ESeabirdState.Air);
            }
        }
    }

    public enum ESeabirdAirState
    {
        None,
        Takeoff,
        Strafe,
        Land
    }

    public class SeabirdAirState : SeabirdState<ESeabirdAirState>
    {
        public SeabirdAirState(StateMachine<ESeabirdState> parent, Seabird seabird) : base(parent, seabird)
        { }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (SeabirdAirSubState state in _subStateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        public override void Enter()
        {
            base.Enter();

            if (!_seabird.CharacterPhysicsLogic.InAir)
            {
                _subStateMachine.ChangeState(ESeabirdAirState.Takeoff);
            }
            else
            {
                _subStateMachine.ChangeState(ESeabirdAirState.Strafe);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (_subStateMachine.CurrentStateEnum != ESeabirdAirState.Takeoff)
            {
                _seabird.EvaluateState();
            }
        }

        public override void Exit()
        {
            base.Exit();

            _subStateMachine.ChangeState(ESeabirdAirState.None);
        }
    }

    public abstract class SeabirdAirSubState : State<ESeabirdAirState, ENone>
    {
        protected Seabird _seabird;
        protected GameplayContext _context;

        public SeabirdAirSubState(StateMachine<ESeabirdAirState> parent, Seabird seabird) : base(parent)
        {
            _seabird = seabird;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class SeabirdAirTakeoffState : SeabirdAirSubState
    {
        private SeabirdAirTakeoffSettings _settings;

        public SeabirdAirTakeoffState(StateMachine<ESeabirdAirState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.AirSettings.TakeoffSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _seabird.EntityModel.Animator.SetBool(Seabird.IsFlappingBoolName, true);
        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (!_seabird.CanGlide())
            {
                Vector3 direction = Vector3.up;

                float dot = Vector3.Dot(_seabird.EntityPhysicsLogic.Rigidbody.linearVelocity, direction);
                float delta = _settings.Speed - dot;

                if (delta > 0f)
                {
                    float change = Mathf.Min(delta, _settings.Acceleration * Time.fixedDeltaTime);
                    float acceleration = change / Time.fixedDeltaTime;

                    _seabird.EntityPhysicsLogic.Rigidbody.AddForce(direction * acceleration, ForceMode.Acceleration);
                }
            }
            else
            {
                _parentStateMachine.ChangeState(ESeabirdAirState.Strafe);
            }
        }

        public override void Exit()
        {
            base.Exit();

            _seabird.EntityModel.Animator.SetBool(Seabird.IsFlappingBoolName, false);
        }
    }

    public class SeabirdAirStrafeState : SeabirdAirSubState
    {
        private SeabirdAirStrafeSettings _settings;

        private int _strafeCount;
        private Strafe _strafe;

        private class Strafe
        {
            private SeabirdAirStrafeState _state;

            private Vector3 _direction;

            private float _timer;

            public Vector3 Direction => _direction;
            public bool IsComplete => _timer >= _state._settings.StrafeDuration + _state._settings.BrakeDuration;

            public Strafe(SeabirdAirStrafeState state, Vector3 direction)
            {
                _state = state;
                _direction = direction;
            }

            public void FixedTick()
            {
                _timer += Time.fixedDeltaTime;

                if (IsComplete)
                {
                    return;
                }

                Vector3 direction;
                float strength;
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                
                // Strafe, then brake
                if (_timer < _state._settings.StrafeDuration)
                {
                    direction = _direction;
                    strength = _state._settings.StrafeAcceleration;
                    rotation *= Quaternion.AngleAxis(-_state._settings.StrafeRoll * Mathf.Sign(direction.x), Vector3.forward);
                }
                else
                {
                    direction = -_direction;
                    strength = Mathf.Abs(_state._seabird.EntityPhysicsLogic.Rigidbody.linearVelocity.x) * _state._settings.BrakeStrength;
                }

                _state._seabird.EntityPhysicsLogic.Rigidbody.AddForce(direction * strength, ForceMode.Acceleration);
                _state._seabird.EntityPhysicsLogic.Rigidbody.MoveRotation(Quaternion.Slerp(_state._seabird.EntityPhysicsLogic.Rigidbody.rotation, rotation, _state._settings.RotateSpeed * Time.fixedDeltaTime));
            }
        }

        public SeabirdAirStrafeState(StateMachine<ESeabirdAirState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.AirSettings.StrafeSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _strafeCount = 0;

            Vector3 centerPosition = _context.Raft.Queries.GetCenterPosition();
            Vector3 strafeDirection = new Vector3(centerPosition.x - _seabird.transform.position.x, 0f, 0f).normalized;

            _strafe = new Strafe(this, strafeDirection);
        }

        public override void Tick()
        {
            base.Tick();

            if (!_strafe.IsComplete)
            {
                return;
            }

            if (_strafeCount < 1)
            {
                _strafe = new Strafe(this, -_strafe.Direction);
                _strafeCount++;
            }
            else
            {
                _parentStateMachine.ChangeState(ESeabirdAirState.Land);
            }
        }
        
        public override void FixedTick()
        {
            base.FixedTick();

            _seabird.StabiliseAltitude(_settings.DampingStrength);

            _strafe.FixedTick();
        }

        public override void Exit()
        {
            base.Exit();

            Quaternion rotation = Quaternion.LookRotation(_seabird.transform.forward, Vector3.up);
            _seabird.EntityPhysicsLogic.Rigidbody.MoveRotation(rotation);
        }
    }

    public class SeabirdAirLandState : SeabirdAirSubState
    {
        private SeabirdAirLandSettings _settings;

        private Vector3 _landPosition;
        private Quaternion _landRotation;

        public SeabirdAirLandState(StateMachine<ESeabirdAirState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.AirSettings.LandSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _seabird.EntityModel.Animator.SetBool(Seabird.IsFlappingBoolName, true);

            _context.Raft.Queries.TryGetClosestTile(_seabird.transform.position, out RaftTile tile);

            _landPosition = tile.transform.position;
            _landPosition += new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f);

            Vector3 direction = (_landPosition - _seabird.transform.position);
            direction.y = 0f;
            direction.Normalize();
            _landRotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        public override void FixedTick()
        {
            base.FixedTick();

            AlignFixedTick();
            RotateFixedTick();

            _seabird.EntityPhysicsLogic.Rigidbody.AddForce(Vector3.up * _settings.FlapStrength, ForceMode.Acceleration);
        }

        private void AlignFixedTick()
        {
            Vector3 offset = _landPosition - _seabird.transform.position;
            offset.y = 0f;

            float distance = offset.magnitude;
            if (distance == 0f)
            {
                return;
            }

            Vector3 direction = offset.normalized;

            Vector3 velocity = _seabird.EntityPhysicsLogic.Rigidbody.linearVelocity;
            velocity.y = 0f;

            float speed = Vector3.Dot(velocity, direction);

            // The rate of change required to come to a stop at _landPosition
            float stopDeceleration = Mathf.Pow(speed, 2f) / (2f * distance);

            Vector3 force;

            if (stopDeceleration <= _settings.AlignDeceleration)
            {
                force = direction * _settings.AlignAcceleration;
            }
            else
            {
                force = -direction * _settings.AlignDeceleration;
            }

            _seabird.EntityPhysicsLogic.Rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void RotateFixedTick()
        {
            Quaternion rotation = Quaternion.Slerp(_seabird.EntityPhysicsLogic.Rigidbody.rotation, _landRotation, _settings.RotateSpeed * Time.fixedDeltaTime);

            _seabird.EntityPhysicsLogic.Rigidbody.rotation = rotation;
        }

        public override void Exit()
        {
            base.Exit();

            _seabird.EntityModel.Animator.SetBool(Seabird.IsFlappingBoolName, false);
        }
    }

    public enum ESeabirdGroundState
    {
        None,
        Idle,
        Roam,
        Squawk
    }

    public class SeabirdGroundState : SeabirdState<ESeabirdGroundState>
    {
        private SeabirdGroundSettings _settings;

        private Collider[] _squawkCollidersNonAlloc = new Collider[1];

        public SeabirdGroundState(StateMachine<ESeabirdState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.GroundSettings;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (SeabirdGroundSubState state in _subStateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        public override void Enter()
        {
            base.Enter();

            _subStateMachine.ChangeState(ESeabirdGroundState.Idle);
        }

        public override void Tick()
        {
            base.Tick();

            if (_subStateMachine.CurrentStateEnum == ESeabirdGroundState.Squawk)
            {
                return;
            }

            _seabird.EvaluateState();

            // EvaluateState can exit this state
            if (_parentStateMachine.CurrentState == this)
            {
                SquawkTick();
            }
        }

        private void SquawkTick()
        {
            if (Physics.OverlapSphereNonAlloc(_seabird.transform.position, _settings.SquawkRange, _squawkCollidersNonAlloc, _settings.SquawkMask) > 0)
            {
                Vector3 direction = _squawkCollidersNonAlloc[0].transform.position - _seabird.transform.position;
                direction.y = 0f;
                direction.Normalize();
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                Tween.RigidbodyMoveRotation(_seabird.EntityPhysicsLogic.Rigidbody, endValue: rotation, duration: 0.2f);

                _subStateMachine.ChangeState(ESeabirdGroundState.Squawk);
            }
        }

        public override void Exit()
        {
            base.Exit();

            _subStateMachine.ChangeState(ESeabirdGroundState.None);
        }
    }

    public abstract class SeabirdGroundSubState : State<ESeabirdGroundState, ENone>
    {
        protected Seabird _seabird;
        protected GameplayContext _context;

        public SeabirdGroundSubState(StateMachine<ESeabirdGroundState> parent, Seabird seabird) : base(parent)
        {
            _seabird = seabird;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class SeabirdGroundIdleState : SeabirdGroundSubState
    {
        private SeabirdGroundIdleSettings _settings;

        private float _idleDuration;

        public SeabirdGroundIdleState(StateMachine<ESeabirdGroundState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.GroundSettings.IdleSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _idleDuration = _settings.IdleRange.RandomRange();
        }

        public override void Tick()
        {
            base.Tick();

            if (_stateTimer >= _idleDuration)
            {
                _parentStateMachine.ChangeState(ESeabirdGroundState.Roam);
            }
        }
    }

    public class SeabirdGroundRoamState : SeabirdGroundSubState
    {
        private SeabirdGroundRoamSettings _settings;

        private Vector2 _roamPosition;
        private PathNavigator _pathNavigator;

        public SeabirdGroundRoamState(StateMachine<ESeabirdGroundState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.GroundSettings.RoamSettings;
        }

        public override void Enter()
        {
            base.Enter();

            Vector2Int tileCell = _context.Raft.Queries.WorldPositionToTileCell(_seabird.transform.position);

            int size = 1;

            List<Vector2Int> structureCells = ListPool<Vector2Int>.Get();

            try
            {
                // Choose tiles that are nearby and have at least one structure cell available
                for (int i = -size; i <= size; i++)
                {
                    for (int j = -size; j <= size; j++)
                    {
                        if (!_context.Raft.Tiles.TryGetValue(tileCell + new Vector2Int(i, j), out RaftTile tile))
                        {
                            continue;
                        }

                        if (tile.TileDefinitionData.IsScaffold)
                        {
                            continue;
                        }

                        Vector2Int structureCell = _context.Raft.Queries.TileCellToStructureCell(tile.Cell);

                        for (int k = 0; k <= 1; k++)
                        {
                            for (int l = 0; l <= 1; l++)
                            {
                                Vector2Int cell = structureCell + new Vector2Int(k, l);

                                if (!_context.Raft.Structures.TryGetValue(cell, out Structure structure) || structure.StructureDefinitionData.IsScaffold)
                                {
                                    structureCells.Add(cell);
                                }
                            }
                        }
                    }
                }

                if (structureCells.Count == 0)
                {
                    _parentStateMachine.ChangeState(ESeabirdGroundState.Idle);
                    return;
                }

                _roamPosition = structureCells[Random.Range(0, structureCells.Count)];
                _roamPosition += new Vector2(Random.value - 0.75f, Random.value - 0.75f);

                _pathNavigator = new PathNavigator(_context.Raft, _seabird.CharacterPhysicsLogic.CapsuleCollider.radius);

                _context.Raft.OnTileChanged += HandleTileChanged;
                _context.Raft.OnStructureChanged += HandleStructureChanged;
            }
            finally
            {
                ListPool<Vector2Int>.Release(structureCells);
            }
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            _pathNavigator.ClearPath();
        }

        private void HandleStructureChanged(Vector2Int cell, Structure previous, Structure current)
        {
            _pathNavigator.ClearPath();
        }

        public override void Tick()
        {
            base.Tick();

            Vector2 position = _context.Raft.Queries.WorldPositionToStructurePosition(_seabird.transform.position);

            if (!_pathNavigator.HasPath())
            {
                if (_pathNavigator.TrySetPath(position, _roamPosition))
                {
                    _seabird.EntityModel.Animator.SetBool(Seabird.IsWalkingBoolName, true);
                }
                else
                {
                    _parentStateMachine.ChangeState(ESeabirdGroundState.Idle);
                    return;
                }
            }

            _pathNavigator.Tick(position);

            if (_pathNavigator.AtDestination())
            {
                _parentStateMachine.ChangeState(ESeabirdGroundState.Idle);
            }
        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (_pathNavigator.HasPath())
            {
                Vector2 cellPosition = _pathNavigator.GetNextPosition();
                Vector3 worldPosition = _context.Raft.Queries.StructurePositionToWorldPosition(cellPosition);

                Vector3 direction = (worldPosition - _seabird.transform.position);
                direction.y = 0f;
                direction.Normalize();

                _seabird.CharacterPhysicsLogic.Move(direction, _settings.MoveSpeed, _settings.MoveAcceleration);
                _seabird.CharacterPhysicsLogic.Look(direction, _settings.RotateSpeed);
            }
        }

        public override void Exit()
        {
            base.Exit();

            _seabird.EntityModel.Animator.SetBool(Seabird.IsWalkingBoolName, false);

            _context.Raft.OnTileChanged -= HandleTileChanged;
            _context.Raft.OnStructureChanged -= HandleStructureChanged;
        }
    }

    public class SeabirdGroundSquawkState : SeabirdGroundSubState
    {
        private HitboxManager _hitboxManager;
        private AudioManager _audioManager;

        private SeabirdGroundSquawkSettings _settings;

        public SeabirdGroundSquawkState(StateMachine<ESeabirdGroundState> parent, Seabird seabird) : base(parent, seabird)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _settings = _seabird.DefinitionData.GroundSettings.SquawkSettings;

            _seabird.SquawkStateAnimationEvents.Add(new StateAnimationEvent(0.3f, () =>
            {
                if (_seabird.isOwner)
                {
                    _hitboxManager.SpawnHitbox(_settings.HitboxData, _seabird, new SpawnParams() { Position = _seabird.transform.position });
                    _seabird.EntityPhysicsLogic.Rigidbody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
                }

                _audioManager.PlaySound(SoundId.SeagullSquawk, 0f);
            }));

            _seabird.SquawkStateAnimationEvents.Add(new StateAnimationEvent(1f, () =>
            {
                if (_seabird.isOwner)
                {
                    _parentStateMachine.ChangeState(ESeabirdGroundState.Idle);
                }
            }));
        }

        public override void Enter()
        {
            _seabird.EntityModel.SetAnimatorTrigger(Seabird.SquawkTriggerName);
        }
    }

    public class SeabirdWaterState : SeabirdState<ENone>
    {
        private SeabirdWaterSettings _settings;

        private float _idleDuration;

        public SeabirdWaterState(StateMachine<ESeabirdState> parent, Seabird seabird) : base(parent, seabird)
        {
            _settings = _seabird.DefinitionData.WaterSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _idleDuration = _settings.IdleRange.RandomRange();
        }

        public override void Tick()
        {
            base.Tick();

            _seabird.EvaluateState();

            if (_parentStateMachine.CurrentState != this)
            {
                return;
            }

            if (_stateTimer >= _idleDuration)
            {
                _parentStateMachine.ChangeState(ESeabirdState.Air);
            }
        }
    }

    public class SeabirdStunState : SeabirdState<ENone>
    {
        public SeabirdStunState(StateMachine<ESeabirdState> parent, Seabird seabird) : base(parent, seabird)
        { }
    }
}