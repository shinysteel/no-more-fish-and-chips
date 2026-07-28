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
        private CharacterDefeatSettings _settings;

        private Character _character;

        private float _tweenTimer;
        private Tween _defeatTween;

        public CharacterDefeatLogic(Character character, SyncVar<bool> netIsDefeated) : base(character, netIsDefeated)
        {
            _character = character;

            _settings = (CharacterDefeatSettings)_character.EntityDefinitionData.EntityDefeatSettings;
        }

        public override void Tick()
        {
            if (!_character.isOwner)
            {
                return;
            }

            if (_settings.DefeatsInWater)
            {
                DefeatsInWaterTick();
            }

            TweenTick();
        }

        private void DefeatsInWaterTick()
        {
            if (_character.CharacterPhysicsModule.InWater)
            {
                SetIsDefeated(true);
            }
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

            if (_character.CharacterPhysicsModule.GroundSurface == null && !_character.CharacterPhysicsModule.InWater)
            {
                return;
            }

            _tweenTimer += Time.deltaTime;

            if (_tweenTimer < _settings.DefeatDuration)
            {
                return;
            }

            _defeatTween = Tween.Scale(_character.transform, endValue: Vector3.zero, duration: _settings.TweenDuration, ease: Ease.InBack)
                .OnComplete(Despawn);
        }

        protected override void HandleNetIsDefeatedChanged(bool defeated)
        {
            _character.CharacterModel.SetDefeated(defeated);
            _character.EntityModel.Animator.Update(0f);

            if (_character.isOwner)
            {
                _character.CharacterRagdollLogic.SetEnabled(defeated);

                if (defeated)
                {
                    _tweenTimer = 0f;
                }
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