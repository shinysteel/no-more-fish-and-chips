using NoMoreFishAndChips.States;
using ShinyOwl.Common.Framework;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public enum ETentacleState
    {
        None,
        Arrive,
        Slam,
        Approach,
        Stun
    }

    public abstract class TentacleState : State<ETentacleState, ENone>
    {
        protected Tentacle _tentacle;
        protected GameplayContext _context;

        public TentacleState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent)
        {
            _tentacle = tentacle;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class TentacleArriveState : TentacleState
    {
        public TentacleArriveState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        { }
    }

    public class TentacleSlamState : TentacleState
    {
        public TentacleSlamState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        { }
    }

    public class TentacleApproachState : TentacleState
    {
        public TentacleApproachState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        { }
    }

    public class TentacleStunState : TentacleState
    {
        public TentacleStunState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        { }
    }
}