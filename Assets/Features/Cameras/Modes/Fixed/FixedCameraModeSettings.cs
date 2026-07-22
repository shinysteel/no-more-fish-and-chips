using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    [CreateAssetMenu(fileName = "FixedCameraModeSettings", menuName = "Settings/Cameras/FixedCameraModeSettings")]
    public class FixedCameraModeSettings : CameraModeSettings
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _rotation;

        public Vector3 Position => _position;
        public Vector3 Rotation => _rotation;
    }
}