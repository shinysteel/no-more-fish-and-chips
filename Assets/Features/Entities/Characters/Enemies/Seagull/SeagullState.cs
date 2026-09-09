using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using Steamworks;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Entities
{
    public enum ESeagullState
    {
        None,
        Arrive,
        Air,
        Ground,
        Water,
        Stun
    }

    public interface ISeagullState
    {
        void InitialiseContext(GameplayContext context);
    }

    public abstract class SeagullState<T> : State<ESeagullState, T>, ISeagullState where T : Enum
    {
        protected Seagull _seagull;
        protected GameplayContext _context;

        public SeagullState(StateMachine<ESeagullState> parent, Seagull seagull) : base(parent)
        {
            _seagull = seagull;
        }

        public virtual void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class SeagullArriveState : SeagullState<ENone>
    {
        private SeagullArriveSettings _settings;

        public SeagullArriveState(StateMachine<ESeagullState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.ArriveSettings;
        }

        public override void Enter()
        {
            base.Enter();

            Vector3 position = _seagull.SpawnInfo.Tile.transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            position += Vector3.up * _settings.StartAltitude;

            _seagull.EntityPhysicsLogic.Rigidbody.position = position;

            _seagull.EntityPhysicsLogic.Rigidbody.linearVelocity = Vector3.zero;
        }
        
        public override void FixedTick()
        {
            base.FixedTick();

            if (_stateTimer < _settings.Delay)
            {
                return;
            }

            // Float down
            _seagull.StabiliseAltitude(_settings.DampingStrength);

            if (_stateTimer >= _settings.Duration)
            {
                _parentStateMachine.ChangeState(ESeagullState.Air);
            }
        }
    }

    public enum ESeagullAirState
    {
        None,
        Takeoff,
        Strafe,
        Land
    }

    public class SeagullAirState : SeagullState<ESeagullAirState>
    {
        public SeagullAirState(StateMachine<ESeagullState> parent, Seagull seagull) : base(parent, seagull)
        { }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (SeagullAirSubState state in _subStateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        public override void Enter()
        {
            base.Enter();

            if (!_seagull.CharacterPhysicsLogic.InAir)
            {
                _subStateMachine.ChangeState(ESeagullAirState.Takeoff);
            }
            else
            {
                _subStateMachine.ChangeState(ESeagullAirState.Strafe);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (_subStateMachine.CurrentStateEnum != ESeagullAirState.Takeoff)
            {
                _seagull.EvaluateState();
            }
        }

        public override void Exit()
        {
            base.Exit();

            _subStateMachine.ChangeState(ESeagullAirState.None);
        }
    }

    public abstract class SeagullAirSubState : State<ESeagullAirState, ENone>
    {
        protected Seagull _seagull;
        protected GameplayContext _context;

        public SeagullAirSubState(StateMachine<ESeagullAirState> parent, Seagull seagull) : base(parent)
        {
            _seagull = seagull;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class SeagullAirTakeoffState : SeagullAirSubState
    {
        private SeagullAirTakeoffSettings _settings;

        public SeagullAirTakeoffState(StateMachine<ESeagullAirState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.AirSettings.TakeoffSettings;
        }
    }

    public class SeagullAirStrafeState : SeagullAirSubState
    {
        private SeagullAirStrafeSettings _settings;

        private int _strafeCount;
        private Strafe _strafe;

        private class Strafe
        {
            private SeagullAirStrafeState _state;

            private Vector3 _direction;

            private float _timer;

            public Vector3 Direction => _direction;
            public bool IsComplete => _timer >= _state._settings.StrafeDuration + _state._settings.BrakeDuration;

            public Strafe(SeagullAirStrafeState state, Vector3 direction)
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
                    strength = Mathf.Abs(_state._seagull.EntityPhysicsLogic.Rigidbody.linearVelocity.x) * _state._settings.BrakeStrength;
                }

                _state._seagull.EntityPhysicsLogic.Rigidbody.AddForce(direction * strength, ForceMode.Acceleration);
                _state._seagull.EntityPhysicsLogic.Rigidbody.MoveRotation(Quaternion.Lerp(_state._seagull.EntityPhysicsLogic.Rigidbody.rotation, rotation, _state._settings.RotateSpeed * Time.fixedDeltaTime));
            }
        }

        public SeagullAirStrafeState(StateMachine<ESeagullAirState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.AirSettings.StrafeSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _strafeCount = 0;

            Vector3 centerPosition = _context.Raft.Queries.GetCenterPosition();
            Vector3 strafeDirection = new Vector3(centerPosition.x - _seagull.transform.position.x, 0f, 0f).normalized;

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
                _parentStateMachine.ChangeState(ESeagullAirState.Land);
            }
        }
        
        public override void FixedTick()
        {
            base.FixedTick();

            _seagull.StabiliseAltitude(_settings.DampingStrength);

            _strafe.FixedTick();
        }
    }

    public class SeagullAirLandState : SeagullAirSubState
    {
        private SeagullAirLandSettings _settings;

        public SeagullAirLandState(StateMachine<ESeagullAirState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.AirSettings.LandSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _seagull.EntityModel.Animator.SetBool(Seagull.IsFlappingBoolName, true);
        }

        public override void FixedTick()
        {
            base.FixedTick();

            _seagull.EntityPhysicsLogic.Rigidbody.AddForce(Vector3.up * _settings.FlapStrength, ForceMode.Acceleration);
        }

        public override void Exit()
        {
            base.Exit();

            _seagull.EntityModel.Animator.SetBool(Seagull.IsFlappingBoolName, false);
        }
    }

    public enum ESeagullGroundState
    {
        None,
        Idle,
        Roam,
        Attack
    }

    public class SeagullGroundState : SeagullState<ESeagullGroundState>
    {
        public SeagullGroundState(StateMachine<ESeagullState> parent, Seagull seagull) : base(parent, seagull)
        { }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (SeagullGroundSubState state in _subStateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        public override void Tick()
        {
            base.Tick();

            _seagull.EvaluateState();
        }
    }

    public abstract class SeagullGroundSubState : State<ESeagullGroundState, ENone>
    {
        protected Seagull _seagull;
        protected GameplayContext _context;

        public SeagullGroundSubState(StateMachine<ESeagullGroundState> parent, Seagull seagull) : base(parent)
        {
            _seagull = seagull;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class SeagullGroundIdleState : SeagullGroundSubState
    {
        private SeagullGroundIdleSettings _settings;

        public SeagullGroundIdleState(StateMachine<ESeagullGroundState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.GroundSettings.IdleSettings;
        }
    }

    public class SeagullGroundRoamState : SeagullGroundSubState
    {
        private SeagullGroundRoamSettings _settings;

        public SeagullGroundRoamState(StateMachine<ESeagullGroundState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.GroundSettings.RoamSettings;
        }
    }

    public class SeagullGroundAttackState : SeagullGroundSubState
    {
        private HitboxManager _hitboxManager;
        private AudioManager _audioManager;

        private SeagullGroundAttackSettings _settings;

        public SeagullGroundAttackState(StateMachine<ESeagullGroundState> parent, Seagull seagull) : base(parent, seagull)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _settings = _seagull.DefinitionData.GroundSettings.AttackSettings;

            _seagull.AttackStateAnimationEvents.Add(new StateAnimationEvent(0.3f, () =>
            {
                if (_seagull.isOwner)
                {
                    _hitboxManager.SpawnHitbox(_settings.HitboxData, _seagull, new SpawnParams() { Position = _seagull.transform.position });
                    _seagull.EntityPhysicsLogic.Rigidbody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
                }

                _audioManager.PlaySound(SoundId.SeagullAttack);
            }));

            _seagull.AttackStateAnimationEvents.Add(new StateAnimationEvent(1f, () =>
            {
                _parentStateMachine.ChangeState(ESeagullGroundState.Idle);
            }));
        }
    }

    public class SeagullWaterState : SeagullState<ENone>
    {
        private SeagullWaterSettings _settings;

        public SeagullWaterState(StateMachine<ESeagullState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.WaterSettings;
        }
    }

    public class SeagullStunState : SeagullState<ENone>
    {
        public SeagullStunState(StateMachine<ESeagullState> parent, Seagull seagull) : base(parent, seagull)
        { }
    }
}