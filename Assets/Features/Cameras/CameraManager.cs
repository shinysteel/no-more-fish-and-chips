using NoMoreFishAndChips.Instantiating;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using System.Threading.Tasks;
using ShinyOwl.Common;
using System;

using Object = UnityEngine.Object;

namespace NoMoreFishAndChips.Cameras
{
    public interface ICameraManagerListener
    { }

    public class CameraManager : GameSystem<ICameraManagerListener>
    {
        private CameraManagerConfig _config;
        public CameraManagerConfig Config => _config;

        private CinemachineBrain _cinemachineBrain;
        public CinemachineBrain CinemachineBrain => _cinemachineBrain;

        private List<ICameraMode> _modes = new();

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _config = config.CameraManagerConfig;

            _cinemachineBrain = Object.Instantiate(_config.CinemachineBrainPrefab);

            Object.DontDestroyOnLoad(_cinemachineBrain.gameObject);

            base.InitialiseConfig(config);
        }

        public override void Tick()
        {
            foreach (ICameraMode mode in _modes)
            {
                mode.Tick();
            }
        }

        public void AddMode(ICameraMode mode)
        {
            if (_modes.Contains(mode))
            {
                return;
            }

            _modes.Add(mode);

            mode.Enter();
        }

        public void RemoveMode(ICameraMode mode)
        {
            if (!_modes.Contains(mode))
            {
                return;
            }

            mode.Exit();

            _modes.Remove(mode);
        }
    }
}