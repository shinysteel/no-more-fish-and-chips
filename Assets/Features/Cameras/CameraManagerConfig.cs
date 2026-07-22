using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    [CreateAssetMenu(fileName = "CameraManagerConfig", menuName = "Configs/Managers/CameraManagerConfig")]
    public class CameraManagerConfig : ScriptableObject
    {
        [SerializeField] private CinemachineBrain _cinemachineBrainPrefab;

        public CinemachineBrain CinemachineBrainPrefab => _cinemachineBrainPrefab;

        // Modes
        [SerializeField] private OrbitCameraModeSettings _environmentMenusOrbitCameraModeSettings;
        [SerializeField] private FollowCameraModeSettings _raftPlayerFollowCameraModeSettings;

        public OrbitCameraModeSettings EnvironmentMenusOrbitCameraModeSettings => _environmentMenusOrbitCameraModeSettings;
        public FollowCameraModeSettings RaftPlayerFollowCameraModeSettings => _raftPlayerFollowCameraModeSettings;
    }
}