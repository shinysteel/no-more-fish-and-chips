using FishFlingers.Pools;
using UnityEngine;

namespace FishFlingers.Audio
{
    public class SoundCue : MonoBehaviour, IPoolable
    {
        [SerializeField] private AudioSource _audioSource;

        private PoolManager _poolManager;

        private float _timer;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        void IPoolable.OnTakenFromPool()
        {
            _timer = 0f;
        }

        public void Initialise(SoundCueData data)
        {
            _audioSource.clip = data.AudioClip;

            _audioSource.Play();
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _audioSource.clip.length)
            {
                _poolManager.ReturnPoolable(this);
            }
        }

        void IPoolable.OnReturnedToPool()
        {
            _audioSource.clip = null;
        }
    }
}