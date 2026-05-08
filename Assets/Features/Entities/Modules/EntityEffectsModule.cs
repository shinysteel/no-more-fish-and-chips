using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class EntityEffectsModule
    {
        protected IEntity _entity;

        public EntityEffectsModule(IEntity entity)
        {
            _entity = entity;

            _entity.EntityHealthModule.OnChanged += HandleHealthChanged;
        }

        ~EntityEffectsModule()
        {
            if (_entity != null)
            {
                _entity.EntityHealthModule.OnChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(int previous, int current)
        {
            if (current == 0)
            {
                return;
            }

            if (current < previous)
            {
                AnimateHurt();
            }
        }

        // For a time, AnimateHurt was intentionally not linked to change in health, since some entities aren't damageable like RaftPlayer
        public virtual void AnimateHurt()
        { }
    }
}