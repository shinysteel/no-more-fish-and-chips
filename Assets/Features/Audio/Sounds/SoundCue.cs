using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Audio
{
    public class SoundCue : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private AudioSource _audioSource;

        private PoolManager _poolManager;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Initialise(SoundCueData data)
        {
            _audioSource.volume = data.Volume;

            _audioSource.pitch = Random.Range(data.MinPitch, data.MaxPitch);

            _audioSource.clip = data.AudioClips[Random.Range(0, data.AudioClips.Length)];

            _audioSource.Play();
        }

        private void Update()
        {
            if (!_audioSource.isPlaying)
            {
                _poolManager.ReturnTypedPoolable(this);
            }
        }

        void IPoolable.OnReturnedToPool()
        {
            _audioSource.clip = null;
        }

        void IPoolable.OnTakenFromPool()
        { }
    }
}