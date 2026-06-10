using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using TMPro;
using PrimeTween;
using ShinyOwl.Common.Extensions;
using NoMoreFishAndChips.Audio;

namespace NoMoreFishAndChips.Entities
{
    public class WaveSign : Structure<WaveSignDefinitionData>
    {
        [SerializeField] private TextMeshPro _waveText;

        private int _index;

        private Sequence _sequence;

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            _index = _context.WaveSpawner.WaveIndex;
            RefreshWaveText();

            _context.WaveSpawner.OnWaveIndexChanged += HandleWaveIndexChanged;
        }

        protected override void OnDespawned()
        {
            _context.WaveSpawner.OnWaveIndexChanged -= HandleWaveIndexChanged;

            base.OnDespawned();
        }

        private void HandleWaveIndexChanged(int index)
        {
            if (_index == index)
            {
                return;
            }

            _index = index;

            _audioManager.PlaySound(SoundId.WaveSignJump);

            if (!isOwner)
            {
                return;
            }

            _sequence.Complete();

            float y = transform.localPosition.y;
            Quaternion rotation = transform.rotation;

            _sequence = Sequence.Create()
                .Group(Tween.LocalPositionY(transform, endValue: y + 0.25f, duration: 0.5f, ease: Ease.OutQuad))
                .Group(Tween.Custom(startValue: 0f, endValue: 1f, duration: 0.5f, onValueChange: (float value) =>
                {
                    transform.rotation = rotation * Quaternion.AngleAxis(value * 360f, Vector3.up);
                }))
                .Chain(Tween.LocalPositionY(transform, endValue: y, duration: 0.1f, ease: Ease.InQuad))
                .ChainCallback(RefreshWaveTextRpc)
                .ChainCallback(Slam);
        }

        [ObserversRpc]
        private void RefreshWaveTextRpc()
        {
            RefreshWaveText();
        }

        private void RefreshWaveText()
        {
            string text = _index.ToString();
            
            if (_waveText.text == text)
            {
                return;
            }

            _waveText.text = text;

            Tween.CompleteAll(_waveText.transform);
            Tween.PunchScale(_waveText.transform, strength: Vector3.one * 0.5f, duration: 0.1f, frequency: 1);
        }

        private void Slam()
        {
            _hitboxManager.SpawnHitbox(DefinitionData.SlamHitboxData, new SpawnParams() { Position = transform.position });

            AudioManager.PlaySoundRpc(SoundId.WaveSignSlam);
        }
    }
}