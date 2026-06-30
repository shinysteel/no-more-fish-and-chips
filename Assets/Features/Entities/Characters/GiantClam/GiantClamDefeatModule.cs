using PrimeTween;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class GiantClamDefeatModule : CharacterDefeatModule
    {
        private GiantClam _clam;

        public GiantClamDefeatModule(GiantClam clam, Func<bool> isDefeatedGetter, Action<bool> isDefeatedSetter) : base(clam, isDefeatedGetter, isDefeatedSetter)
        {
            _clam = clam;
        }

        public override void Tick()
        { }

        public override void HandleIsDefeatedChanged(bool defeated)
        {
            base.HandleIsDefeatedChanged(defeated);

            if (_clam.isOwner && defeated)
            {
                AnimateAsync();
            }
        }

        private void AnimateAsync()
        {
            _clam.EntityPhysicsModule.Rigidbody.isKinematic = true;

            Vector3 startPosition = _clam.transform.position;
            Vector3 endPosition = startPosition + Vector3.up * 0.25f;
            Quaternion startRotation = _clam.transform.rotation;

            Sequence.Create()
                .Chain(Tween.Custom(startValue: 0f, endValue: 1f, duration: 0.75f, onValueChange: (float value) =>
                {
                    _clam.SetNetExplodedBlend(value);
                    _clam.transform.position = Vector3.Lerp(startPosition, endPosition, value);
                    _clam.transform.rotation = startRotation * Quaternion.AngleAxis(value * 360f, Vector3.up);
                })
                .Chain(Tween.Scale(_clam.transform, startValue: _clam.transform.localScale, endValue: Vector3.zero, duration: 0.1f, ease: Ease.InQuad)))
                .ChainCallback(Despawn);
        }

        protected override void Despawn()
        {
            if (_networkManager.IsServer)
            {
                _clam.Inventory.DropAllItems(_clam.transform.position);
            }

            base.Despawn();
        }
    }
}