using FishFlingers.Pools;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Audio
{
    public interface IAudioManagerListener
    { }

    public enum SoundId
    {
        HumanJump,
        HumanFootstep,
        PaddleAttack,
        SeagullAttack,
        HumanSwim
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

            foreach (SoundCueData data in _config.SoundCueDataScanner.GetAssets())
            {
                _idDataMap.Add(data.Id, data);
            }

            base.Initialise(config);
        }

        public void PlaySound(SoundId id)
        {
            SoundCue cue = _poolManager.GetTypedPoolable<SoundCue>(new SpawnParams());
            cue.Initialise(_idDataMap[id]);
        }
    }
}