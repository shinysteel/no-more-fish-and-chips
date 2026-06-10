using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Pools;
using UnityEngine;

namespace NoMoreFishAndChips.Effects
{
    public class VFX : MonoBehaviour, IPoolable
    {
        [SerializeField] private VfxId _vfxId;
        [SerializeField] private SoundId _soundId;
        [SerializeField] private ParticleSystem _particleSystem;

        private EffectManager _effectManager;
        private AudioManager _audioManager;

        private float _timer;

        public VfxId VfxId => _vfxId;

        private void Awake()
        {
            _effectManager = GameManager.Instance.Get<EffectManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();
        }

        public void OnTakenFromPool()
        {
            _particleSystem.Play();

            if (_soundId != SoundId.None)
            {
                _audioManager.PlaySound(_soundId);
            }

            _timer = 0f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer < _particleSystem.main.duration)
            {
                return;
            }

            _effectManager.ReturnVfx(this);
        }

        public void OnReturnedToPool()
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}