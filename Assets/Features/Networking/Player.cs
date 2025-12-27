using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishFlingers.Entities;

namespace FishFlingers.Networking
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Character _humanPrefab;

        private Character _human;

        protected override void OnSpawned()
        {
            Instantiate(_humanPrefab);
        }

        protected override void OnOwnerDisconnected(PlayerID ownerId)
        {
            Destroy(gameObject);
        }
    }
}