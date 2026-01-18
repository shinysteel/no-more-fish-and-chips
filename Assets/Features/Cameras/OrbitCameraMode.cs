using UnityEngine;

namespace FishFlingers.Cameras
{
    public class OrbitCameraMode : ICameraMode
    {
        private Vector3 _center;
        private float _radius;
        private float _yOffset;
        private float _speed;
        private float _timer;

        public OrbitCameraMode(Vector3 center, float radius, float yOffset, float speed)
        {
            _center = center;
            _radius = radius;
            _yOffset = yOffset;
            _speed = speed;
        }

        public void Enter(Camera camera) 
        {
            _timer = 0f;
        }

        public void LateTick(Camera camera)
        {
            _timer += Time.deltaTime;

            float angle = -_speed * _timer;
            float x = _center.x - _radius * Mathf.Cos(angle);
            float z = _center.z + _radius * Mathf.Sin(angle);

            camera.transform.position = new Vector3(x, _center.y + _yOffset, z);

            camera.transform.LookAt(_center);
        }

        public void Exit(Camera camera) { }
    }
}