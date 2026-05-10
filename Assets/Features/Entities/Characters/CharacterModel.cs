using FishFlingers.Pools;
using PrimeTween;
using PurrNet;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace FishFlingers.Entities
{
    public class CharacterModel : EntityModel
    {
        [SerializeField] private Transform _itemLocator;

        private float _blinkTimer = 0f;
        private float _minBlinkInterval = 2.5f;
        private float _maxBlinkInterval = 7.5f;

        private Tween _hurtTween;

        public Transform ItemLocator => _itemLocator;

        // Animator
        private const string BlinkAnimatorTriggerName = "Blink";
        private const string IsDefeatedAnimatorBoolName = "IsDefeated";
        private const string HurtAnimatorTriggerName = "Hurt";

        // Shader
        private const string HurtBlendShaderPropertyName = "_HurtBlend";
        private const string DefeatBlendShaderPropertyName = "_DefeatBlend";

        private void Start()
        {
            ResetBlinkTimer();
        }

        private void Update()
        {
            BlinkUpdate();
        }

        private void BlinkUpdate()
        {
            _blinkTimer -= Time.deltaTime;

            if (_blinkTimer > 0f)
            {
                return;
            }

            _animator.SetTrigger(BlinkAnimatorTriggerName);

            ResetBlinkTimer();
        }

        private void ResetBlinkTimer()
        {
            _blinkTimer = Random.Range(_minBlinkInterval, _maxBlinkInterval);
        }

        public void FlashRed()
        {
            _hurtTween.Stop();   

            // Flash red
            _hurtTween = Tween.Custom(startValue: 1f, endValue: 0f, duration: 0.5f, onValueChange: (float value) =>
            {
                _material.SetFloat(HurtBlendShaderPropertyName, value);
            });
        }

        public void AdditiveHurt()
        {
            // Additive animation
            SetTrigger(HurtAnimatorTriggerName);

            ResetBlinkTimer();
        }

        public void SetDefeated(bool defeated)
        {
            // Tint grey
            _material.SetFloat(DefeatBlendShaderPropertyName, defeated ? 1f : 0f);

            _animator.SetBool(IsDefeatedAnimatorBoolName, defeated);
        }
    }
}