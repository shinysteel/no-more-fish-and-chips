using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterLogicFactory : EntityLogicFactory
    {
        public CharacterLogicFactory() : base()
        { }

        public override EntityDefeatLogic CreateDefeatLogic(Entity entity, SyncVar<bool> netIsDefeated)
        {
            return new CharacterDefeatLogic((Character)entity, netIsDefeated);
        }

        public override EntityEffectsLogic CreateEffectsLogic(Entity entity)
        {
            return new CharacterEffectsLogic((Character)entity);
        }

        public override EntityPhysicsLogic CreatePhysicsLogic(Entity entity, Rigidbody rigidbody, Collider collider)
        {
            return new CharacterPhysicsLogic((Character)entity, rigidbody, collider);
        }

        public virtual CharacterRagdollLogic CreateRagdollLogic(Character character)
        {
            return new CharacterRagdollLogic(character);
        }

        public virtual CharacterActLogic CreateActLogic(Character character)
        {
            return new CharacterActLogic(character);
        }
    }
}