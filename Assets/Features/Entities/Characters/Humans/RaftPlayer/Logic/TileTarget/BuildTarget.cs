using NoMoreFishAndChips.States;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    public abstract class BuildTarget
    {
        protected GameplayContext _context;

        protected BuildTargetSettings _settings;

        protected Vector3 _position;
        protected Vector2Int _cell;

        public BuildTarget(GameplayContext context, BuildTargetSettings settings, Vector3 position)
        {
            _context = context;
            _settings = settings;

            SetPosition(position);
        }

        public virtual void Dispose()
        { }

        public abstract void SetPosition(Vector3 position);

        protected abstract bool CanBuild();
    }
}