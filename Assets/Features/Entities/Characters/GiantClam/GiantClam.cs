using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Extensions;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System.Linq;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class GiantClam : Character<GiantClamDefinitionData>, IInteractable, IHasInventory
    {
        [SerializeField] private Inventory _inventory;

        public Inventory Inventory => _inventory;

        private StateMachine<EState> _stateMachine;

        private PanelInstance<GiantClamsMouthPanel> _giantClamPanelInstance;

        private SyncVar<bool> _netCanOpenInventory = new SyncVar<bool>(ownerAuth: true);
        private SyncVar<float> _netExplodeBlend = new SyncVar<float>(ownerAuth: true);

        public IInteractableSettings IInteractableSettings => DefinitionData.IInteractableSettings;

        private const string ExplodeBlendName = "_ExplodeBlend";

        private enum EState
        {
            None,
            SpawnLaunch,
            AwaitItems,
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
                base.Enter();

                if (!_clam._context.Raft.Queries.TryGetRandomLine((RaftLine line) => line.Nodes.Count > 0, out RaftLine line))
                {
                    _clam._entityManager.Despawn(_clam);
                    return;
                }

                RaftEdge edge = Random.value < 0.5f ? line.MinEdge : line.MaxEdge;

                _clam.transform.rotation = Quaternion.LookRotation(Utils.Math.DirectionToVector3(Utils.Math.FlipDirection(edge.Direction)), Vector3.up);

                Vector2Int edgeDirection = Utils.Math.DirectionToVector2Int(edge.Direction);
                Vector3 startPosition = _clam._context.Raft.Queries.CellToWorldPosition(edge.Node.Cell + edgeDirection);
                Vector3 endPosition = _clam._context.Raft.Queries.CellToWorldPosition(edge.Node.Cell);

                Sequence.Create()
                    .Group(Tween.PositionY(_clam.transform, startValue: -1f, endValue: 1f, duration: 0.5f))
                    .Group(Tween.Custom(startValue: 0f, endValue: 1f, duration: 0.5f, ease: Ease.OutSine, onValueChange: (float value) =>
                    {
                        Vector3 position = Vector3.Lerp(startPosition, endPosition, value);
                        position.y = _clam.transform.position.y;
                        _clam.transform.position = position;
                    }))
                    .ChainCallback(() => _clam._rigidbody.isKinematic = false);
            }

            public override void Tick()
            {
                base.Tick();

                if (_clam._rigidbody.isKinematic)
                {
                    return;
                }

                if (_clam.CharacterPhysicsModule.IsGrounded)
                {
                    _parentStateMachine.ChangeState(EState.AwaitItems);
                }
            }
        }
        
        // Await items to be filled into the clam's inventory. Eventually respond depending on whether this was done or not
        private class AwaitItemsState : State
        {
            public AwaitItemsState(StateMachine<EState> parent) : base(parent)
            { }
            
            public override void Enter()
            {
                base.Enter();

                _clam._inventory.OnInventorySlotChanged += HandleInventorySlotChanged;

                _clam._netCanOpenInventory.value = true;
            }

            public override void Exit()
            {
                base.Exit();

                _clam._inventory.OnInventorySlotChanged -= HandleInventorySlotChanged;

                _clam._netCanOpenInventory.value = false;
            }

            private void HandleInventorySlotChanged(Vector2Int cell, InventorySlot slot)
            {
                CheckIfFull();
            }

            private void CheckIfFull()
            {
                bool full = !_clam._inventory.InventorySlots.Any(slot => slot.Value.InventoryItem == null);

                if (full)
                {
                    // From becoming full of items, explode. This both returns the items and defeats the enemy
                    _clam.EntityDefeatModule.SetIsDefeated(true);
                }
            }

            public override void Tick()
            {
                base.Tick();

                if (_stateTimer < _clam.DefinitionData.AwaitItemsSettings.Duration)
                {
                    return;
                }

                _parentStateMachine.ChangeState(EState.EmptyRage);
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

            _inventory.SetLayouts(DefinitionData.InventoryLayout, DefinitionData.InventoryLayout);

            _stateMachine = new();

            SpawnLaunchState launchState = new SpawnLaunchState(_stateMachine);
            AwaitItemsState itemsState = new AwaitItemsState(_stateMachine);
            EmptyRageState rageState = new EmptyRageState(_stateMachine);

            launchState.Initialise(this);
            itemsState.Initialise(this);
            rageState.Initialise(this);

            _stateMachine.AddState(EState.SpawnLaunch, launchState);
            _stateMachine.AddState(EState.AwaitItems, itemsState);
            _stateMachine.AddState(EState.EmptyRage, rageState);
        }

        protected override EntityDefeatModule CreateDefeatModule()
        {
            return new GiantClamDefeatModule(this, GetNetIsDefeated, SetNetIsDefeated);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _giantClamPanelInstance = new PanelInstance<GiantClamsMouthPanel>(_uiManager.Config.GiantClamsMouthPanelPrefab);

            if (isOwner)
            {
                _entityDefeatModule.OnIsDefeatedChanged += HandleIsDefeatedChanged;
                
                _stateMachine.ChangeState(EState.SpawnLaunch);
            }

            _netCanOpenInventory.onChanged += HandleNetCanOpenInventoryChanged;
            _netExplodeBlend.onChanged += HandleNetExplodeBlendChanged;
        }

        protected override void OnDespawned()
        {
            if (isOwner)
            {
                _entityDefeatModule.OnIsDefeatedChanged -= HandleIsDefeatedChanged;
            }

            _netCanOpenInventory.onChanged -= HandleNetCanOpenInventoryChanged;
            _netExplodeBlend.onChanged -= HandleNetExplodeBlendChanged;

            base.OnDespawned();
        }

        private void HandleNetCanOpenInventoryChanged(bool canOpenInventory)
        {
            if (!canOpenInventory)
            {
                _giantClamPanelInstance.Hide();
            }
        }

        public void SetNetExplodedBlend(float blend)
        {
            _netExplodeBlend.value = blend;
        }

        private void HandleNetExplodeBlendChanged(float blend)
        {
            _entityModel.Material.SetFloat(ExplodeBlendName, blend);
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

        public bool CanPrompt()
        {
            return _netCanOpenInventory.value;
        }

        public WorldUI CreatePromptUI()
        {
            InteractPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(DefinitionData.IInteractableSettings.Hotkey);
            return ui;
        }

        public bool CanInteract()
        {
            return _netCanOpenInventory.value;
        }

        public void Interact()
        {
            _giantClamPanelInstance.Toggle((GiantClamsMouthPanel panel) => panel.Setup(_context, this, _inventory));
        }
    }
}