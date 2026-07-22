using Unity.Cinemachine;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public class OrbitCameraMode : TargetCameraMode<OrbitCameraModeSettings>
    {
        private CinemachineOrbitalFollow _orbitalFollow;

        public OrbitCameraMode(OrbitCameraModeSettings settings, Transform target) : base(settings, target)
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