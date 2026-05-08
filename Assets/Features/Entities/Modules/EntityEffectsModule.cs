using UnityEngine;

namespace FishFlingers.Entities
{
    public class EntityEffectsModule
    {
        protected IEntity _entity;

        protected EntityEffectsSettings _entityEffectsSettings;

        public EntityEffectsModule(IEntity entity)
        {
            _entity = entity;

            _entityEffectsSettings = _entity.EntityDefinitionData.EntityEffectsSettings;

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

            if (current < previous || _entityEffectsSettings.AlwaysAnimateHurt)
            {
                AnimateHurt();
            }
        }

        // For a time, AnimateHurt was intentionally not linked to change in health, since some entities aren't damageable like RaftPlayer
        public virtual void AnimateHurt()
        { }
    }
}