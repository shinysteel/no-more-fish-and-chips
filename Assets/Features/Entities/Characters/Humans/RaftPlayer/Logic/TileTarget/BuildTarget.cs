using NoMoreFishAndChips.States;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    public abstract class BuildTarget
    {
        protected GameplayContext _context;

        protected RaftPlayerBuildTargetSettings _settings;

        protected Vector3 _position;
        protected Vector2Int _cell;

        public event Action OnChanged;

        public BuildTarget(GameplayContext context, RaftPlayerBuildTargetSettings settings, Vector3 position)
        {
            _context = context;
            _settings = settings;

            SetPosition(position);
        }

        public virtual void Dispose()
        { }

        public abstract void SetPosition(Vector3 position);

        protected abstract bool CanBuild();

        protected void RaiseChanged()
        {
            OnChanged?.Invoke();
        }
    }
}