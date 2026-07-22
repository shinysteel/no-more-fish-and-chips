using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    [CreateAssetMenu(fileName = "OrbitCameraModeSettings", menuName = "Settings/Cameras/OrbitCameraModeSettings")]
    public class OrbitCameraModeSettings : CameraModeSettings
    {
        [SerializeField] private float _speed = 1f;

        public float Speed => _speed;
    }
}