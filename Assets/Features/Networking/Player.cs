using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishFlingers.Entities;
using FishFlingers.Cameras;

namespace FishFlingers.Networking
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Character _humanPrefab;

        private CameraManager _cameraManager;

        private Character _human;

        protected override void OnEarlySpawn()
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
        }

        protected override void OnSpawned()
        {
            if (!isOwner)
            {
                return;
            }

            _human = Instantiate(_humanPrefab);

            _cameraManager.SetMode(new FollowCameraMode(_human.transform, new Vector3(0f, 3f, -5f)));
        }

        protected override void OnOwnerDisconnected(PlayerID ownerId)
        {
            Destroy(gameObject);
        }
    }
}