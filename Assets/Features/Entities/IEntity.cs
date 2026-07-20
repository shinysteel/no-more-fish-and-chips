using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    // Implemented by NetEntity and Entity
    public interface IEntity : ISurface
    {
        void InitialiseContext(GameplayContext context);

        EntityDefinitionData EntityDefinitionData { get; }
        EntityModel EntityModel { get; }

        // Named like this to reuse existing properties from NetBehaviour and Monobehaviour
        bool isSpawned { get; }
        bool isOwner { get; }
        Transform transform { get; }

        SurfaceType ISurface.SurfaceType => EntityDefinitionData.SurfaceType;

        EntityHealthModule EntityHealthModule { get; }
        EntityDefeatModule EntityDefeatModule { get; }
        EntityLifecycleModule EntityLifecycleModule { get; }
        EntityEffectsModule EntityEffectsModule { get; }
        EntityPhysicsModule EntityPhysicsModule { get; }
    }
}