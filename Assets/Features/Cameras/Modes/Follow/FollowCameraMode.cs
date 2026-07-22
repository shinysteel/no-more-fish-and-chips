using Unity.Cinemachine;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public class FollowCameraMode : CameraMode<FollowCameraModeSettings>
    {
        public FollowCameraMode(FollowCameraModeSettings settings, Transform targetTransform) : base(settings, targetTransform)
        { }
    }
}