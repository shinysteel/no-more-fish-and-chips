using FishFlingers.Environments;
using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Entities
{
    public interface IEntity
    {
        // Ordered to simplify NetEntity and Entity's implementations

        void Initialise(GameplayContext context);

        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        void SetHealth(int health);

        public Rigidbody Rigidbody { get; }
    }
}