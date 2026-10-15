using NoMoreFishAndChips.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NoMoreFishAndChips.Entities;

namespace NoMoreFishAndChips.UI
{
    public class InteractPromptUI : WorldUI
    {
        [SerializeField] private ActionHotkeyView _actionHotkeyView;
        [SerializeField] private TextMeshProUGUI _actionText;

        private PoolManager _poolManager;

        protected virtual void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void SetupInteract(ActionHotkey hotkey, string text)
        {
            _actionHotkeyView.Set(hotkey);
            _actionText.text = text;
        }
    }
}