using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class EntityLogic
    {
        protected Entity _entity;
        protected GameplayContext _context;

        public EntityLogic(Entity entity)
        {
            _entity = entity;
        }

        public virtual void OnSpawned()
        { }

        public virtual void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }

        public virtual void OnDespawned()
        { }

        public virtual void Tick()
        { }

        public virtual void FixedTick()
        { }
    }
}