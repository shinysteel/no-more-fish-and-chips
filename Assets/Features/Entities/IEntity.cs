using FishFlingers.Environments;
using UnityEngine;

namespace FishFlingers.Entities
{
    public interface IEntity
    {
        // Ordered to simplify NetEntity and Entity's implementations

        void Initialise(Raft raft);

        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        void SetHealth(int health);

        public Rigidbody Rigidbody { get; }
    }
}