using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Floatable : MonoBehaviour, IFloatable
    {
        [SerializeField] private Rigidbody _rigidbody;

        Rigidbody IFloatable.Rigidbody => _rigidbody;

        bool IFloatable.CanFloat()
        {
            return true;
        }
    }
}