using FishFlingers.Environments;
using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Entities
{
    // Implemented by NetEntity and Entity
    public interface IEntity
    {
        void Initialise(GameplayContext context);

        EntityDefinitionData EntityDefinitionData { get; }
        EntityModel EntityModel { get; }

        // Named like this to reuse existing properties from NetBehaviour and Monobehaviour
        bool isSpawned { get; }
        bool isOwner { get; }
        Transform transform { get; }

        EntityHealthModule EntityHealthModule { get; }
        EntityDefeatModule EntityDefeatModule { get; }
        EntityLifecycleModule EntityLifecycleModule { get; }
        EntityEffectsModule EntityEffectsModule { get; }
        EntityPhysicsModule EntityPhysicsModule { get; }
    }
}