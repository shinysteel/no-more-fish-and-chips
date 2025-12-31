using FishFlingers.Entities;
using PurrNet;
using UnityEngine;

namespace FishFlingers.Environments
{
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private float _waveInterval = 2.5f;

        [SerializeField] private FlyingFish _flyingFishPrefab;

        private void Update()
        {
            //state.WaveTimer += delta;

            //if (state.WaveTimer < _waveInterval)
            //{
            //    return;
            //}

            //state.WaveTimer -= _waveInterval;

            //_predictionManager.Spawn(_flyingFishPrefab.gameObject, PlayerID.Server);
        }
    }
}