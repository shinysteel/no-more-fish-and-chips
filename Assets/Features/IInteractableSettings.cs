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
        [SerializeField] private PropId _previewId;
        [SerializeField] private Vector3 _previewPosition;
        [SerializeField] private Vector3 _previewScale = Vector3.one;

        public ActionHotkey Hotkey => _hotkey;
        public int Priority => _priority;
        public float MaxAngle => _maxAngle;
        public float MaxDistance => _maxDistance;
        public PropId PreviewId => _previewId;
        public Vector3 PreviewPosition => _previewPosition;
        public Vector3 PreviewScale => _previewScale;
    }
}