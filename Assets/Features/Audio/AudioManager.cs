using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using PurrNet;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Audio
{
    public interface IAudioManagerListener
    { }

    public class AudioManager : GameSystem<IAudioManagerListener>
    {
        private PoolManager _poolManager;

        private AudioManagerConfig _config;

        private Dictionary<SoundId, SoundCueData> _idDataMap = new();

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.AudioManagerConfig;

            foreach (SoundCueData data in _config.SoundCueDataScanner.GetAssets())
            {
                _idDataMap.Add(data.Id, data);
            }

            base.InitialiseConfig(config);
        }

        public void PlaySound(SoundId id, Vector3 position, float pitchOffset)
        {
            SoundCue cue = _poolManager.GetTypedPoolable<SoundCue>(new SpawnParams());
            cue.Initialise(_idDataMap[id], position, pitchOffset);
        }

        [ObserversRpc]
        public static void PlaySoundRpc(SoundId id, Vector3 position, float pitchOffset)
        {
            AudioManager audioManager = GameManager.Instance.Get<AudioManager>();
            audioManager.PlaySound(id, position, pitchOffset);
        }
    }
}