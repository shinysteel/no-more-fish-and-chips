using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Entities
{
    public enum EFlyingFishState
    {
        None,
        Surface,
        Fly
    }

    public abstract class FlyingFishState : State<EFlyingFishState, ENone>
    {
        protected FlyingFish _fish;
        protected GameplayContext _context;

        public FlyingFishState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent)
        {
            _fish = fish;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class FlyingFishSurfaceState : FlyingFishState
    {
        private RaftEdge _edge;

        public FlyingFishSurfaceState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent, fish)
        { }

        public override void Enter()
        {
            base.Enter();

            Surface();
            ChooseLandingPosition();
            WiggleThenFly();
        }

        // Surface away from the edge
        private void Surface()
        {
            _edge = Random.value <= 0.5f ? _fish.SpawnInfo.RaftLine.MinEdge : _fish.SpawnInfo.RaftLine.MaxEdge;

            Vector2Int cell = _edge.Node.Cell;
            cell += Utils.Math.DirectionToVector2Int(_edge.Direction) * Random.Range(2, 4);

            Vector3 position = _context.Raft.Queries.CellToWorldPosition(cell);
            position += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            position += Vector3.down * 0.5f;

            _fish.EntityPhysicsLogic.Rigidbody.position = position;

            EffectManager.SpawnVfxRpc(VfxId.WaterSplash, new Vector3(position.x, 0f, position.z));
        }

        // Choose between the edge and the tile behind it
        private void ChooseLandingPosition()
        {
            List<RaftTile> tiles = ListPool<RaftTile>.Get();

            tiles.Add(_context.Raft.Tiles[_edge.Node.Cell]);

            if (_context.Raft.Tiles.TryGetValue(_edge.Node.Cell - Utils.Math.DirectionToVector2Int(_edge.Direction), out RaftTile tile))
            {
                tiles.Add(tile);
            }

            Vector3 position = tiles.OrderBy(tile => tile.EntityDefeatLogic.IsDefeated).ThenBy(tile => Random.value).First().transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));

            _fish.SetLandingPosition(position);

            ListPool<RaftTile>.Release(tiles);
        }

        private void WiggleThenFly()
        {
            Quaternion getRotationToLandingPosition()
            {
                Vector3 direction = (_fish.LandingPosition - _fish.EntityPhysicsLogic.Rigidbody.position).normalized;
                direction.y = 0f;
                direction.Normalize();

                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                return rotation;
            }

            Sequence.Create(updateType: UpdateType.FixedUpdate)
                // Wiggle while facing the target
                .Chain(Tween.Custom(startValue: 0f, endValue: 1f, duration: 1f, onValueChange: _ =>
                {
                    Quaternion rotation = getRotationToLandingPosition();
                    rotation *= Quaternion.AngleAxis(-15f, Vector3.right);

                    _fish.EntityPhysicsLogic.Rigidbody.MoveRotation(rotation);
                }))
                // Tilt up
                .Chain(Tween.Custom(startValue: 0f, endValue: 1f, duration: 0.25f, onValueChange: (float value) =>
                {
                    Quaternion startRotation = getRotationToLandingPosition();
                    startRotation *= Quaternion.AngleAxis(-15f, Vector3.right);

                    Quaternion endRotation = getRotationToLandingPosition();
                    endRotation *= Quaternion.AngleAxis(-75f, Vector3.right);

                    Quaternion rotation = Quaternion.Slerp(startRotation, endRotation, value);

                    _fish.EntityPhysicsLogic.Rigidbody.MoveRotation(rotation);
                }))
                .ChainCallback(() => _parentStateMachine.ChangeState(EFlyingFishState.Fly));
        }
    }

    public class FlyingFishFlyState : FlyingFishState
    {
        private HitboxManager _hitboxManager;
        private EntityManager _entityManager;

        public FlyingFishFlyState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent, fish)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public override void Enter()
        {
            base.Enter();

            _fish.EntityModel.Animator.SetBool(FlyingFish.IsFlyingBoolName, true);

            _fish.EntityPhysicsLogic.Rigidbody.AddForce(_fish.EntityPhysicsLogic.Rigidbody.rotation * Vector3.forward * 50f, ForceMode.Impulse);

            Quaternion rotation = Quaternion.FromToRotation(_fish.EntityPhysicsLogic.Rigidbody.rotation * Vector3.forward, Vector3.down) * _fish.EntityPhysicsLogic.Rigidbody.rotation;

            PrimeTweenFix.RigidbodyMoveRotation(_fish.EntityPhysicsLogic.Rigidbody, endValue: rotation, duration: 1f, ease: Ease.InOutQuad);
        }
    }
}