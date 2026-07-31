using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Extensions;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class WaveSign : Structure<WaveSignDefinitionData>
    {
        [SerializeField] private TextMeshPro _countText;

        private SyncVar<int> _netCount = new SyncVar<int>(ownerAuth: true);

        private Sequence _sequence;

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            HandleNetCountChanged(_netCount.value);
            _netCount.onChanged += HandleNetCountChanged;

            _context.VoyageRunner.OnWaveIndexChanged += HandleWaveIndexChanged;
            _context.VoyageRunner.OnStageComplete += HandleStageComplete;
        }

        protected override void OnDespawned()
        {
            _netCount.onChanged -= HandleNetCountChanged;

            _context.VoyageRunner.OnWaveIndexChanged -= HandleWaveIndexChanged;
            _context.VoyageRunner.OnStageComplete -= HandleStageComplete;

            base.OnDespawned();
        }

        private void HandleNetCountChanged(int count)
        {
            string text = count.ToString();

            if (_countText.text == text)
            {
                return;
            }

            _countText.text = text;

            Tween.CompleteAll(_countText.transform);
            Tween.PunchScale(_countText.transform, strength: Vector3.one * 0.5f, duration: 0.1f, frequency: 1);
        }

        private void HandleWaveIndexChanged(int index)
        {
            if (isOwner)
            {
                Jump();
            }
        }

        private void HandleStageComplete()
        {
            if (isOwner)
            {
                Jump();
            }
        }

        private void Jump()
        {
            AudioManager.PlaySoundRpc(SoundId.WaveSignJump);

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
                .ChainCallback(() => _netCount.value++)
                .ChainCallback(Slam);
        }

        private void Slam()
        { 
            _hitboxManager.SpawnHitbox(DefinitionData.SlamHitboxData, new SpawnParams() { Position = transform.position });

            AudioManager.PlaySoundRpc(SoundId.WaveSignSlam);
        }
    }
}