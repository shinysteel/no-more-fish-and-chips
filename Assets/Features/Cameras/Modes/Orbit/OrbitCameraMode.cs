using Unity.Cinemachine;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public class OrbitCameraMode : CameraMode<OrbitCameraModeSettings>
    {
        private CinemachineOrbitalFollow _orbitalFollow;

        public OrbitCameraMode(OrbitCameraModeSettings settings, Transform targetTransform) : base(settings, targetTransform)
        { }

        public override void Enter()
        {
            base.Enter();

            _orbitalFollow = _camera.GetComponent<CinemachineOrbitalFollow>();
        }

        public override void Tick()
        {
            _orbitalFollow.HorizontalAxis.Value += _settings.Speed * Time.deltaTime;
        }
    }
}