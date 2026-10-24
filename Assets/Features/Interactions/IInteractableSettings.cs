using NoMoreFishAndChips.Environments;
using UnityEngine;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Localisation;

namespace NoMoreFishAndChips.Interactions
{
    [CreateAssetMenu(fileName = "IInteractableSettings", menuName = "Settings/IInteractableSettings")]
    public class IInteractableSettings : ScriptableObject
    {
        [SerializeField] private int _priority;
        [SerializeField] private float _maxAngle = 45f;
        [SerializeField] private float _maxDistance = 0.5f;

        public int Priority => _priority;
        public float MaxAngle => _maxAngle;
        public float MaxDistance => _maxDistance;
    }
}