using NoMoreFishAndChips.Audio;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundCueData", menuName = "Data/Audio/SoundCueData")]
public class SoundCueData : ScriptableObject
{
    [SerializeField] private SoundId _id;
    [SerializeField] private float _volume = 1f;
    [SerializeField] private float _minPitch = 0.9f;
    [SerializeField] private float _maxPitch = 1.1f;
    [SerializeField] private AudioClip[] _audioClips = new AudioClip[0];

    public SoundId Id => _id;
    public float Volume => _volume;
    public float MinPitch => _minPitch;
    public float MaxPitch => _maxPitch;
    public AudioClip[] AudioClips => _audioClips;
}
