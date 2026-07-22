using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public class FixedCameraMode : CameraMode<FixedCameraModeSettings>
    {
        public FixedCameraMode(FixedCameraModeSettings settings) : base(settings)
        { }

        public override void Enter()
        {
            base.Enter();

            _camera.transform.position = _settings.Position;
            _camera.transform.eulerAngles = _settings.Rotation;
        }
    }
}