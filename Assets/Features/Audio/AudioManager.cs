using FishFlingers.Pools;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Audio
{
    public interface IAudioManagerListener
    { }

    public enum SoundId
    {
        Jump
    }

    public class AudioManager : GameSystem<IAudioManagerListener>
    {
        private PoolManager _poolManager;

        private AudioManagerConfig _config;

        private Dictionary<SoundId, SoundCueData> _idDataMap = new();

        public override void Initialise(GameManagerConfig config)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.AudioManagerConfig;

            foreach (SoundMapping mapping in _config.SoundMappings)
            {
                _idDataMap.Add(mapping.Id, mapping.Data);
            }

            base.Initialise(config);
        }

        public void PlaySound(SoundId id)
        {
            SoundCue cue = _poolManager.GetPoolable<SoundCue>(new SpawnParams());
            cue.Initialise(_idDataMap[id]);
        }
    }
}