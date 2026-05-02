using System;
using UnityEngine;

namespace FishFlingers.Audio
{
    [Serializable]
    public class SoundMapping
    {
        [SerializeField] private SoundId _id;
        [SerializeField] private SoundCueData _data;

        public SoundId Id => _id;
        public SoundCueData Data => _data;
    }

    [CreateAssetMenu(fileName = "AudioManagerConfig", menuName = "Configs/Managers/AudioManagerConfig")]
    public class AudioManagerConfig : ScriptableObject
    {
        [SerializeField] private SoundMapping[] _soundMappings;

        public SoundMapping[] SoundMappings => _soundMappings;
    }
}