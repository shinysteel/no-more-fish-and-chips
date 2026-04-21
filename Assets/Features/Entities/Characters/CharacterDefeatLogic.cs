using UnityEngine;
using System;
using System.Threading.Tasks;

namespace FishFlingers.Entities
{
    public class CharacterDefeatLogic
    {
        private EntityManager _entityManager;

        private Character _character;

        private bool _isDefeated;

        public event Action OnDefeated;

        public CharacterDefeatLogic(Character character)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _character = character;

            _character.HealthModule.OnChanged += HandleHealthChanged;

            _character.CharacterModel.Material.SetFloat(EntityModel.DefeatBlendName, 0f);
        }

        ~CharacterDefeatLogic()
        {
            if (_character != null)
            {
                _character.HealthModule.OnChanged -= HandleHealthChanged;
            }
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

            _character.RagdollLogic.SetEnabled(true);

            _character.CharacterModel.Material.SetFloat(EntityModel.DefeatBlendName, 1f);

            OnDefeated?.Invoke();

            _ = DespawnAsync();
        }

        private async Task DespawnAsync()
        {
            await Task.Delay(Mathf.RoundToInt(_character.CharacterData.CharacterDefeatSettings.Duration * 1000f));

            _character.RagdollLogic.SetEnabled(false);

            _entityManager.Despawn(_character);
        }
    }
}