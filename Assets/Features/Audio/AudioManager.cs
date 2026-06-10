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

    public enum SoundId
    {
        None,
        HumanJump,
        HumanFootstep,
        PaddleAttack,
        SeagullAttack,
        HumanSwim,
        WaterSplash,
        ClamChestOpen,
        ClamChestClose,
        SeagullFlap,
        UIPositiveClick,
        UINegativeClick,
        WaveSignJump,
        WaveSignSlam
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

        [ObserversRpc]
        public static void PlaySoundRpc(SoundId id)
        {
            AudioManager audioManager = GameManager.Instance.Get<AudioManager>();
            audioManager.PlaySound(id);
        }
    }
}