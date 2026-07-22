using ShinyOwl.Common;
using Unity.Cinemachine;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public interface ICameraMode
    {
        void Enter();
        void Tick();
        void Exit();
    }

    public abstract class CameraMode<T> : ICameraMode where T : CameraModeSettings
    {
        protected T _settings;
        private Transform _targetTransform;

        protected CinemachineCamera _camera;

        public CameraMode(T settings, Transform targetTransform)
        {
            _settings = settings;
            _targetTransform = targetTransform;
        }

        public virtual void Enter()
        {
            _camera = Object.Instantiate(_settings.CinemachineCameraPrefab);
            Object.DontDestroyOnLoad(_camera.gameObject);

            _camera.Target.TrackingTarget = _targetTransform;

            _camera.CancelDamping();
        }

        public virtual void Tick()
        { }

        public virtual void Exit()
        {
            Object.Destroy(_camera.gameObject);
        }
    }
}