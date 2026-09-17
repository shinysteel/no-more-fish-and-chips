using NoMoreFishAndChips.States;
using UnityEngine;
using System;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.Entities
{
    public abstract class BuildTarget
    {
        protected GameplayContext _context;

        protected BuildTargetSettings _settings;

        protected EntityId _entityId;

        protected Vector3 _position = Vector3.positiveInfinity;
        protected Vector2Int _cell = Vector2Int.one * int.MaxValue;

        protected int _rotations;

        public BuildTarget(GameplayContext context, BuildTargetSettings settings, EntityId entityId)
        {
            _context = context;
            _settings = settings;
            _entityId = entityId;
        }

        public virtual void Dispose()
        { }
        
        public virtual void ChangeRotations(int amount)
        {
            _rotations = Utils.Math.EuclideanModulo(_rotations + amount, 4);
        }

        public abstract void SetPosition(Vector3 position);
        
        public virtual void Tick()
        { }

        protected abstract bool CanBuild();
    }
}