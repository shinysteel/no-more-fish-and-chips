using UnityEngine;

[CreateAssetMenu(fileName = "SoundCueData", menuName = "Data/Audio/SoundCueData")]
public class SoundCueData : ScriptableObject
{
    [SerializeField] private AudioClip _audioClip;

    public AudioClip AudioClip => _audioClip;
}
