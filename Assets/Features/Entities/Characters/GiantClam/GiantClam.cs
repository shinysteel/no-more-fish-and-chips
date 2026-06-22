using UnityEngine;
using ShinyOwl.Common.Framework;

namespace NoMoreFishAndChips.Entities
{
    public class GiantClam : Character<GiantClamDefinitionData>
    {
        private StateMachine<EState> _stateMachine;

        private enum EState
        {
            None,
            SpawnLaunch,
            AwaitItems,
            FullExplode,
            EmptyRage
        }

        private class State : State<EState, ENone>
        {
            protected GiantClam _clam;

            public State(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(GiantClam clam)
            {
                _clam = clam;
            }
        }

        // Choose a perimeter tile to target, and spawn in the water next to it. Then, launch out of the water onto it
        private class SpawnLaunchState : State
        {
            public SpawnLaunchState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                // choose a random perimeter tile
                // launch onto it from the edge its on
                // land in the center
            }
        }
        
        // Await items to be filled into the clam's inventory. Eventually respond depending on whether this was done or not
        private class AwaitItemsState : State
        {
            public AwaitItemsState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                // setup an inventory layout that must be filled
            }

            public override void Tick()
            {
                // await items to fill for x seconds
                // if full, go to full explode state
                // if empty after time elapsed, go to rage state
            }
        }

        // From becoming full of items, explode. This both returns the items and defeats the enemy
        private class FullExplodeState : State
        {
            public FullExplodeState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                // tween an explosion
                // spawn back all the items
                // defeat
            }
        }

        // From not receiving enough items in time, briefly go into a rage state before slamming down
        private class EmptyRageState : State
        {
            public EmptyRageState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                // tween a rage
                // jump, then slam down on the tile
                // deal x damage to the tile. if the tile is destroyed, keep going into the water
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            SpawnLaunchState launchState = new SpawnLaunchState(_stateMachine);
            AwaitItemsState itemsState = new AwaitItemsState(_stateMachine);
            FullExplodeState explodeState = new FullExplodeState(_stateMachine);
            EmptyRageState rageState = new EmptyRageState(_stateMachine);

            launchState.Initialise(this);
            itemsState.Initialise(this);
            explodeState.Initialise(this);
            rageState.Initialise(this);

            _stateMachine.AddState(EState.SpawnLaunch, launchState);
            _stateMachine.AddState(EState.AwaitItems, itemsState);
            _stateMachine.AddState(EState.FullExplode, explodeState);
            _stateMachine.AddState(EState.EmptyRage, rageState);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner)
            {
                _entityDefeatModule.OnIsDefeatedChanged += HandleIsDefeatedChanged;

                _stateMachine.ChangeState(EState.SpawnLaunch);
            }
        }

        protected override void OnDespawned()
        {
            if (isOwner)
            {
                _entityDefeatModule.OnIsDefeatedChanged -= HandleIsDefeatedChanged;
            }

            base.OnDespawned();
        }

        protected override void Update()
        {
            base.Update();

            if (isOwner)
            {
                _stateMachine.Tick();
            }
        }

        private void HandleIsDefeatedChanged(bool defeated)
        {
            if (_stateMachine.CurrentEnum != EState.None)
            {
                _stateMachine.ChangeState(EState.None);
            }
        }
    }
}