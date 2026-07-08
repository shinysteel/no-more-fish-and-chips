using NoMoreFishAndChips.Networking;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Island : NetBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;
    }
}