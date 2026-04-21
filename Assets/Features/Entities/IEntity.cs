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
        EntityModel EntityModel { get; }

        EntityHealthModule HealthModule { get; }

        Transform Transform { get; }
        Rigidbody Rigidbody { get; }
    }
}