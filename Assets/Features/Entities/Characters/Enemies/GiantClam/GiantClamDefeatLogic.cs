using PrimeTween;
using PurrNet;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class GiantClamDefeatLogic : CharacterDefeatLogic
    {
        private GiantClam _clam;

        public GiantClamDefeatLogic(GiantClam clam, SyncVar<bool> netIsDefeated) : base(clam, netIsDefeated)
        {
            _clam = clam;
        }

        public override void Tick()
        { }

        protected override void HandleNetIsDefeatedChanged(bool defeated)
        {
            base.HandleNetIsDefeatedChanged(defeated);

            if (_clam.isOwner && defeated)
            {
                AnimateAsync();
            }
        }

        private void AnimateAsync()
        {
            _clam.EntityPhysicsLogic.Rigidbody.isKinematic = true;

            Vector3 startPosition = _clam.transform.position;
            Vector3 endPosition = startPosition + Vector3.up * 0.25f;
            Quaternion startRotation = _clam.transform.rotation;

            Sequence.Create()
                .Chain(Tween.Custom(startValue: 0f, endValue: 1f, duration: 0.75f, onValueChange: (float value) =>
                {
                    _clam.SetNetExplodeBlend(value);
                    _clam.transform.position = Vector3.Lerp(startPosition, endPosition, value);
                    _clam.transform.rotation = startRotation * Quaternion.AngleAxis(value * 360f, Vector3.up);
                })
                .Chain(Tween.Scale(_clam.transform, startValue: _clam.transform.localScale, endValue: Vector3.zero, duration: 0.1f, ease: Ease.InQuad)))
                .ChainCallback(Despawn);
        }

        public override void Despawn()
        {
            if (_networkManager.IsServer)
            {
                _clam.Inventory.DropAllItems(_clam.transform.position);
            }

            base.Despawn();
        }
    }
}