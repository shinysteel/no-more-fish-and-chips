using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoMoreFishAndChips.Entities
{
    public enum ETentacleState
    {
        None,
        Arrive,
        Approach,
        Slam,
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
        private TentacleArriveSettings _settings;

        public TentacleArriveState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        {
            _settings = _tentacle.DefinitionData.ArriveSettings;
        }

        public override void Enter()
        {
            base.Enter();

            Surface();
        }

        private void Surface()
        {
            _context.Raft.Queries.TryGetRandomLine(out RaftLine line);

            RaftEdge edge = line.GetRandomEdge();

            Vector2Int cell = edge.Node.Tile.Cell + edge.Direction2d;

            Vector3 position = _context.Raft.Queries.TileCellToWorldPosition(cell);

            Vector3 perpendicular = Vector3.Cross(edge.Direction3d, Vector3.up);
            position += perpendicular * (Random.value - 0.5f);

            position += Vector3.down * _settings.Depth;
            
            _tentacle.EntityPhysicsLogic.Rigidbody.position = position;

            _tentacle.EntityPhysicsLogic.Rigidbody.rotation = Quaternion.LookRotation(-edge.Direction3d, Vector3.up);
        }

        public override void Tick()
        {
            base.Tick();

            if (_stateTimer >= _settings.Duration)
            {
                _parentStateMachine.ChangeState(ETentacleState.Approach);
            }
        }
    }

    public class TentacleApproachState : TentacleState
    {
        private TentacleApproachSettings _settings;

        private Collider[] _tileOverlapsNonAlloc = new Collider[4];

        private Vector2 _approachPosition;
        private PathNavigator _pathNavigator;

        public TentacleApproachState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        {
            _settings = _tentacle.DefinitionData.ApproachSettings;
        }

        public override void Enter()
        {
            base.Enter();

            _context.Raft.OnTileChanged += HandleTileChanged;
            
            int overlaps = Physics.OverlapBoxNonAlloc(_tentacle.transform.position + _tentacle.transform.forward * 0.5f + Vector3.up * 0.25f, new Vector3(0.25f, 0.5f, 0.5f), _tileOverlapsNonAlloc, _tentacle.transform.rotation, _settings.TileMask);

            for (int i = 0; i < overlaps; i++)
            {
                if (!Utils.Physics.ColliderTryGetComponent(_tileOverlapsNonAlloc[i], out RaftTile tile))
                {
                    continue;
                }

                if (!tile.EntityDefeatLogic.IsDefeated)
                {
                    _tentacle.SetSlamDirection(_tentacle.transform.forward);
                    _parentStateMachine.ChangeState(ETentacleState.Slam);
                    return;
                }
            }

            _context.Raft.Queries.TryGetClosestPerimeterNode(_tentacle.transform.position, (RaftPerimeterNode node) => !node.Tile.EntityDefeatLogic.IsDefeated, out RaftPerimeterNode node);

            float closestDistance = Mathf.Infinity;
            Vector3 closestDirection3d = Vector3.zero;

            foreach (Vector3 direction in node.OpenDirections)
            {
                Vector3 position = node.Tile.transform.position;
                position.y = _tentacle.transform.position.y;
                position += direction * 0.5f;
                float distance = Vector3.Distance(position, _tentacle.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDirection3d = direction;
                }
            }

            Vector3 targetPosition = _context.Raft.Queries.TileCellToWorldPosition(node.Tile.Cell);

            targetPosition += closestDirection3d * 0.5f;
            targetPosition += closestDirection3d * _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius;

            Vector2Int closestDirection2d = new Vector2Int((int)closestDirection3d.x, (int)closestDirection3d.z);
            Vector2Int approachCell = node.Tile.Cell + closestDirection2d;

            Vector3 perpendicularDirection3d = Vector3.Cross(closestDirection3d, Vector3.up);
            Vector2Int perpendicularDirection2d = new Vector2Int((int)perpendicularDirection3d.x, (int)perpendicularDirection3d.z);

            float perpendicularMin = -0.5f;

            if (_context.Raft.Tiles.ContainsKey(approachCell - perpendicularDirection2d))
            {
                perpendicularMin += _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius;
            }

            float perpendicularMax = 0.5f;

            if (_context.Raft.Tiles.ContainsKey(approachCell + perpendicularDirection2d))
            {
                perpendicularMax -= _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius;
            }

            perpendicularMin = Mathf.Min(perpendicularMin, 0f);
            perpendicularMax = Mathf.Max(perpendicularMax, 0f);

            targetPosition += perpendicularDirection3d * Random.Range(perpendicularMin, perpendicularMax);

            _approachPosition = _context.Raft.Queries.WorldPositionToStructurePosition(targetPosition);

            _pathNavigator = new PathNavigator(_context.EnvironmentGameplayReferences.Ocean, _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius);

            _tentacle.SetSlamDirection(-closestDirection3d);
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {

        }

        public override void Tick()
        {
            base.Tick();

            Vector2 position = _context.Raft.Queries.WorldPositionToStructurePosition(_tentacle.transform.position);

            if (!_pathNavigator.HasPath())
            {
                _pathNavigator.TrySetPath(position, _approachPosition);
            }

            _pathNavigator.Tick(position);

            if (_pathNavigator.AtDestination())
            {
                _parentStateMachine.ChangeState(ETentacleState.Slam);
            }
        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (_pathNavigator.HasPath())
            {
                Vector2 cellPosition = _pathNavigator.GetNextPosition();
                Vector3 worldPosition = _context.Raft.Queries.StructurePositionToWorldPosition(cellPosition);

                Vector3 direction = (worldPosition - _tentacle.transform.position);
                direction.y = 0f;
                direction.Normalize();

                _tentacle.CharacterPhysicsLogic.Move(direction, _settings.MoveSpeed, _settings.MoveAcceleration);
                _tentacle.CharacterPhysicsLogic.Rotate(direction, _settings.RotateSpeed);
            }
        }

        public override void Exit()
        {
            base.Exit();

            _context.Raft.OnTileChanged -= HandleTileChanged;
        }
    }

    public class TentacleSlamState : TentacleState
    {
        private HitboxManager _hitboxManager;

        private TentacleSlamSettings _settings;

        public TentacleSlamState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();

            _settings = _tentacle.DefinitionData.SlamSettings;

            _tentacle.SlamImpactStateAnimationEvents.Add(new StateAnimationEvent(0.4f, () => _hitboxManager.SpawnHitbox(_settings.HitboxData, _tentacle, new SpawnParams() { Position = _tentacle.transform.position + _tentacle.transform.forward, Rotation = _tentacle.transform.rotation })));
            _tentacle.SlamImpactStateAnimationEvents.Add(new StateAnimationEvent(1f, () => _parentStateMachine.ChangeState(ETentacleState.Approach)));
        }

        public override void Enter()
        {
            base.Enter();

            _tentacle.EntityModel.Animator.SetBool(Tentacle.IsChargingBoolName, true);
        }

        public override void Tick()
        {
            base.Tick();

            if (_stateTimer - Time.deltaTime < _settings.ChargeDuration && _stateTimer >= _settings.ChargeDuration)
            {
                _tentacle.EntityModel.SetAnimatorTrigger(Tentacle.SlamTriggerName);
                _tentacle.EntityModel.Animator.SetBool(Tentacle.IsChargingBoolName, false);
            }
        }

        public override void FixedTick()
        {
            base.FixedTick();

            _tentacle.CharacterPhysicsLogic.Rotate(_tentacle.SlamDirection, _settings.RotateSpeed);
        }
    }

    public class TentacleStunState : TentacleState
    {
        public TentacleStunState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        { }
    }
}