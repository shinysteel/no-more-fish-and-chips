using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityLogicFactory
    {
        public virtual EntityHealthLogic CreateHealthLogic(Entity entity, SyncVar<int> netHealth)
        {
            return new EntityHealthLogic(entity, netHealth);
        }

        public virtual EntityDefeatLogic CreateDefeatLogic(Entity entity, SyncVar<bool> netIsDefeated)
        {
            return new EntityDefeatLogic(entity, netIsDefeated);
        }

        public virtual EntityLifecycleLogic CreateLifecycleLogic(Entity entity)
        {
            return new EntityLifecycleLogic(entity);
        }

        public virtual EntityEffectsLogic CreateEffectsLogic(Entity entity)
        {
            return new EntityEffectsLogic(entity);
        }

        public virtual EntityPhysicsLogic CreatePhysicsLogic(Entity entity, Rigidbody rigidbody, Collider collider)
        {
            return new EntityPhysicsLogic(entity, rigidbody, collider);
        }
    }
}