using NoMoreFishAndChips.Environments;
using UnityEngine;
using NoMoreFishAndChips.Entities;

namespace NoMoreFishAndChips
{
    [CreateAssetMenu(fileName = "IInteractableSettings", menuName = "Settings/IInteractableSettings")]
    public class IInteractableSettings : ScriptableObject
    {
        [SerializeField] private ActionHotkey _hotkey;
        [SerializeField] private int _priority;
        [SerializeField] private float _maxAngle = 45f;
        [SerializeField] private float _maxDistance = 0.5f;

        public ActionHotkey Hotkey => _hotkey;
        public int Priority => _priority;
        public float MaxAngle => _maxAngle;
        public float MaxDistance => _maxDistance;
    }
}