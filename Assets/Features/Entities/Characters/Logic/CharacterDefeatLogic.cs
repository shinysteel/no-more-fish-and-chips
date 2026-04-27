using FishFlingers.Items;
using PrimeTween;
using ShinyOwl.Common;
using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class CharacterDefeatLogic
    {
        private EntityManager _entityManager;
        private ItemManager _itemManager;

        private Character _character;

        private CharacterDefeatSettings _settings;

        private bool _isDefeated;

        private float _defeatTimer;

        private Tween _defeatTween;

        public event Action OnDefeated;

        public CharacterDefeatLogic(Character character)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();

            _character = character;

            _settings = _character.CharacterData.CharacterDefeatSettings;

            _character.HealthModule.OnChanged += HandleHealthChanged;
        }

        ~CharacterDefeatLogic()
        {
            if (_character != null)
            {
                _character.HealthModule.OnChanged -= HandleHealthChanged;
            }
        }

        public void Tick()
        {
            if (!_isDefeated)
            {
                return;
            }

            if (_defeatTween.isAlive)
            {
                return;
            }

            if (!_character.PhysicsLogic.IsGrounded && !_character.PhysicsLogic.InWater)
            {
                return;
            }
            
            _defeatTimer += Time.deltaTime;

            if (_defeatTimer < _settings.DefeatDuration)
            {
                return;
            }

            _defeatTween = Tween.Scale(_character.transform, endValue: Vector3.zero, duration: _settings.TweenDuration, ease: Ease.InBack).OnComplete(() =>
            {
                _character.RagdollLogic.SetEnabled(false);

                _character.CharacterModel.SetDefeated(false);

                // Simulate 1 second to have the character unblink
                _character.CharacterModel.Animator.Update(1f);

                _itemManager.SpawnDrops(_character.transform.position, _character.CharacterData.DropTables);

                _entityManager.Despawn(_character);
            });
        }

        private void HandleHealthChanged(int previous, int current)
        {
            if (_isDefeated)
            {
                return;
            }

            if (current > 0)
            {
                return;
            }

            Defeat();
        }

        public void Defeat()
        {
            _isDefeated = true;

            _defeatTimer = 0f;

            _character.CharacterModel.SetDefeated(true);

            _character.CharacterModel.Animator.Update(0f);

            _character.RagdollLogic.SetEnabled(true);

            OnDefeated?.Invoke();
        }
    }
}