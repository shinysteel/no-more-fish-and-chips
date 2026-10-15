using NoMoreFishAndChips.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Localisation;

namespace NoMoreFishAndChips.UI
{
    public class InteractPromptUI : WorldUI
    {
        [SerializeField] private ActionHotkeyView _actionHotkeyView;
        [SerializeField] private TextMeshProUGUI _actionText;

        private LocalisationManager _localisationManager;

        protected virtual void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
        }

        public void SetupInteract(ActionHotkey hotkey, LocalisationTerm term)
        {
            _actionHotkeyView.Set(hotkey);
            _actionText.text = _localisationManager.GetString(term);
        }
    }
}