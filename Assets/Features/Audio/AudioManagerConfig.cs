using System;
using UnityEngine;

namespace FishFlingers.Audio
{
    [CreateAssetMenu(fileName = "AudioManagerConfig", menuName = "Configs/Managers/AudioManagerConfig")]
    public class AudioManagerConfig : ScriptableObject
    {
        [SerializeField] private SoundCueDataScanner _soundCueDataScanner;

        public SoundCueDataScanner SoundCueDataScanner => _soundCueDataScanner;
    }
}