using NoMoreFishAndChips.Networking;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Island : NetBehaviour, IFloatable
    {
        [SerializeField] private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;

        bool IFloatable.CanFloat()
        {
            return isOwner && isFullySpawned;
        }
    }
}