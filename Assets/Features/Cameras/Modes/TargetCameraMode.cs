using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public class TargetCameraMode<T> : CameraMode<T> where T : CameraModeSettings
    {
        private Transform _target;

        public TargetCameraMode(T settings, Transform target) : base(settings)
        {
            _target = target;
        }

        public override void Enter()
        {
            base.Enter();

            _camera.Target.TrackingTarget = _target;

            _camera.CancelDamping();
        }
    }
}