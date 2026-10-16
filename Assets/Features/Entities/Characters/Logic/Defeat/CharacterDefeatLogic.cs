using NoMoreFishAndChips.Items;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterDefeatLogic : EntityDefeatLogic
    {
        private CharacterDefeatSharedSettings _sharedSettings;

        private Character _character;

        private float _tweenTimer;
        private Tween _defeatTween;

        public CharacterDefeatLogic(Character character, SyncVar<bool> netIsDefeated) : base(character, netIsDefeated)
        {
            _character = character;

            _sharedSettings = _character.CharacterDefinitionData.CharacterDefeatSharedSettings;
        }

        public override void Tick()
        {
            if (!_character.isOwner)
            {
                return;
            }

            TweenTick();
        }

        private void TweenTick()
        {
            if (!_netIsDefeated.value)
            {
                return;
            }

            if (_defeatTween.isAlive)
            {
                return;
            }

            if (_character.CharacterPhysicsLogic.InAir)
            {
                return;
            }

            _tweenTimer += Time.deltaTime;

            if (_tweenTimer < _sharedSettings.DefeatDuration)
            {
                return;
            }

            _defeatTween = Tween.Scale(_character.transform, endValue: Vector3.zero, duration: _sharedSettings.TweenDuration, ease: Ease.InBack).OnComplete(Despawn);
        }

        protected override void HandleNetIsDefeatedChanged(bool defeated)
        {
            _character.CharacterModel.SetDefeated(defeated);
            _character.EntityModel.Animator.Update(0f);
            _character.CharacterRagdollLogic.SetEnabled(defeated);

            if (_character.isOwner && defeated)
            {
                _tweenTimer = 0f;
            }

            RaiseIsDefeatedChanged();
        }

        public override void Despawn()
        {
            _character.CharacterRagdollLogic.SetEnabled(false);

            _character.CharacterModel.SetDefeated(false);

            // Simulate 1 second to have the character unblink
            _character.EntityModel.Animator.Update(1f);

            base.Despawn();
        }
    }
}