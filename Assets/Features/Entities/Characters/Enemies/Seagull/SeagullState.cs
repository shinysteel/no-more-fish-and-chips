using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Hitboxes;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class SeagullState
    { }

    public enum ESeagullState
    {
        None,
        Arrive,
        Air,
        Ground,
        Water,
        Stun
    }

    public abstract class SeagullState<T> : State<ESeagullState, T> where T : Enum
    {
        protected Seagull _seagull;

        public SeagullState(StateMachine<ESeagullState> parent, Seagull seagull) : base(parent)
        {
            _seagull = seagull;
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

            _seagull.transform.position = _seagull.SpawnInfo.Tile.transform.position + Vector3.up * 5f;

            _parentStateMachine.ChangeState(ESeagullState.Air);
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

        public override void Enter()
        {
            base.Enter();

            if (!_seagull.CharacterPhysicsModule.InAir)
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

            _subStateMachine.Tick();
        }
    }

    public abstract class SeagullAirSubState : State<ESeagullAirState, ENone>
    {
        protected Seagull _seagull;

        public SeagullAirSubState(StateMachine<ESeagullAirState> parent, Seagull seagull) : base(parent)
        {
            _seagull = seagull;
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

        public SeagullAirStrafeState(StateMachine<ESeagullAirState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.AirSettings.StrafeSettings;
        }
    }

    public class SeagullAirLandState : SeagullAirSubState
    {
        private SeagullAirLandSettings _settings;

        public SeagullAirLandState(StateMachine<ESeagullAirState> parent, Seagull seagull) : base(parent, seagull)
        {
            _settings = _seagull.DefinitionData.AirSettings.LandSettings;
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
    }

    public abstract class SeagullGroundSubState : State<ESeagullGroundState, ENone>
    {
        protected Seagull _seagull;

        public SeagullGroundSubState(StateMachine<ESeagullGroundState> parent, Seagull seagull) : base(parent)
        {
            _seagull = seagull;
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