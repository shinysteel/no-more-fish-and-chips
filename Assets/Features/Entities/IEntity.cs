using FishFlingers.Environments;
using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Entities
{
    public interface IEntity
    {
        // Ordered to simplify NetEntity and Entity's implementations

        void Initialise(GameplayContext context);

        EntityData EntityData { get; }

        EntityHealthModule HealthModule { get; }
        EntityDefeatModule DefeatModule { get; }
        EntityRagdollModule RagdollModule { get; }

        Rigidbody Rigidbody { get; }
    }
}