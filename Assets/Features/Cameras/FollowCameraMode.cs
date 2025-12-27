using UnityEngine;

namespace FishFlingers.Cameras
{
    public class FollowCameraMode : ICameraMode
    {
        private Transform _target;
        private Vector3 _offset;

        public FollowCameraMode(Transform target, Vector3 offset)
        {
            _target = target;
            _offset = offset;
        }

        public void LateUpdate(Camera camera)
        {
            if (_target == null)
            {
                return;
            }

            camera.transform.position = _target.position + _offset;
            camera.transform.LookAt(_target);
        }

        public void Enter(Camera camera)
        { }

        public void Exit(Camera camera)
        { }
    }
}