using NoMoreFishAndChips.UI;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterHealthLogic : EntityHealthLogic
    {
        private UIManager _uiManager;

        private Character _character;

        private CharacterHealthSettings _settings;

        private HealthBarUI _healthBarUI;

        public CharacterHealthLogic(Character character, SyncVar<int> netHealth) : base(character, netHealth)
        { 
            _uiManager = GameManager.Instance.Get<UIManager>();

            _character = character;

            _settings = _character.CharacterDefinitionData.CharacterHealthSettings;
        }

        protected override void HandleNetHealthChanged(int previous, int current)
        {
            base.HandleNetHealthChanged(previous, current);

            if (current < _maxHealth)
            {
                _healthBarUI ??= _uiManager.CreateWorldUI(_uiManager.Config.HealthBarUIPrefab, Vector3.zero);
                _healthBarUI.SetMaxHealth(_maxHealth);
                _healthBarUI.SetFillAmount((float)current / _maxHealth);
            }
            else
            {
                DestroyHealthBar();
            }
        }

        public override void OnDespawned()
        {
            base.OnDespawned();

            DestroyHealthBar();
        }

        private void DestroyHealthBar()
        {
            if (_healthBarUI != null)
            {
                _uiManager.DestroyWorldUI(_healthBarUI);
                _healthBarUI = null;
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (_healthBarUI != null)
            {
                _healthBarUI.transform.position = _character.transform.position + Vector3.up * _settings.HealthBarOffset;
            }
        }
    }
}