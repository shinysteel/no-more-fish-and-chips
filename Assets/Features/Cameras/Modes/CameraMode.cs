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
        protected CinemachineCamera _camera;

        public CameraMode(T settings)
        {
            _settings = settings;
        }

        public virtual void Enter()
        {
            _camera = Object.Instantiate(_settings.CinemachineCameraPrefab);
            Object.DontDestroyOnLoad(_camera.gameObject);
        }

        public virtual void Tick()
        { }

        public virtual void Exit()
        {
            Object.Destroy(_camera.gameObject);
        }
    }
}