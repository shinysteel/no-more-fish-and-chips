using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityEffectsLogic : EntityLogic
    {
        public EntityEffectsLogic(Entity entity) : base(entity)
        {
            _entity.EntityHealthLogic.OnChanged += HandleHealthChanged;
        }

        public override void OnDespawned()
        {
            if (_entity != null)
            {
                _entity.EntityHealthLogic.OnChanged -= HandleHealthChanged;
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