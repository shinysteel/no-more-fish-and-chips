using UnityEngine;
using System;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterActLogic : CharacterLogic
    {
        private CharacterActSettings _settings;

        private float _poise;
        private float _regenTimer;
        private float _staggerTimer;

        private bool IsStaggered => _staggerTimer > 0f;
        public virtual bool CanAct => !_character.EntityDefeatLogic.IsDefeated && !IsStaggered;

        public event Action OnStaggered;

        public CharacterActLogic(Character character) : base(character)
        {
            _settings = character.CharacterDefinitionData.ActSettings;

            _poise = _settings.Poise;
        }

        public override void Tick()
        {
            if (_character.isOwner)
            {
                RegenTick();
                StaggerTick();
            }
        }

        private void RegenTick()
        {
            if (IsStaggered)
            {
                return;
            }

            _regenTimer = Mathf.Max(_regenTimer - Time.deltaTime, 0f);

            if (_regenTimer == 0f)
            {
                _poise = _settings.Poise;
            }
        }

        private void StaggerTick()
        {
            _staggerTimer = Mathf.Max(_staggerTimer - Time.deltaTime, 0f);
        }

        public void ChangePoise(float change)
        {
            if (IsStaggered)
            {
                return;
            }

            _poise += change;
            _poise = Mathf.Max(_poise, 0f);

            if (_poise > 0f)
            {
                _regenTimer = _settings.RegenDelay;
            }
            else
            {
                Stagger();
            }
        }

        private void Stagger()
        {
            _staggerTimer = _settings.StaggerDuration;
            _regenTimer = 0f;

            OnStaggered?.Invoke();
        }
    }
}