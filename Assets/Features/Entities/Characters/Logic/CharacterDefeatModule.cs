using FishFlingers.Items;
using PrimeTween;
using ShinyOwl.Common;
using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace FishFlingers.Entities
{
    public class CharacterDefeatModule : EntityDefeatModule
    {
        private Character _character;
        private CharacterDefeatSettings _settings;


        private float _defeatTimer;

        private Tween _defeatTween;

        public CharacterDefeatModule(Character character, Func<bool> isDefeatedGetter, Action<bool> isDefeatedSetter) : base(character, isDefeatedGetter, isDefeatedSetter)
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
            if (!IsDefeated)
            {
                return;
            }

            if (_defeatTween.isAlive)
            {
                return;
            }

            if (!_character.CharacterPhysicsModule.IsGrounded && !_character.CharacterPhysicsModule.InWater)
            {
                return;
            }

            _defeatTimer += Time.deltaTime;

            if (_defeatTimer < _settings.DefeatDuration)
            {
                return;
            }

            _defeatTween = Tween.Scale(_character.transform, endValue: Vector3.zero, duration: _settings.TweenDuration, ease: Ease.InBack)
                .OnComplete(Despawn);
        }

        public override void HandleIsDefeatedChanged(bool defeated)
        {
            _character.CharacterModel.SetDefeated(defeated);
            _character.CharacterModel.Animator.Update(0f);
            _character.RagdollLogic.SetEnabled(defeated);

            if (_character.isOwner)
            {
                if (defeated)
                {
                    _defeatTimer = 0f;
                }
            }

            RaiseIsDefeatedChanged();
        }

        protected override void Despawn()
        {
            _character.RagdollLogic.SetEnabled(false);

            _character.CharacterModel.SetDefeated(false);

            // Simulate 1 second to have the character unblink
            _character.CharacterModel.Animator.Update(1f);

            base.Despawn();
        }
    }
}