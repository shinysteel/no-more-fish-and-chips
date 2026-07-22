using Unity.Cinemachine;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public class FollowCameraMode : TargetCameraMode<FollowCameraModeSettings>
    {
        public FollowCameraMode(FollowCameraModeSettings settings, Transform target) : base(settings, target)
        { }
    }
}