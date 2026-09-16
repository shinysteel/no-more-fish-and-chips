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

        protected PoolManager _poolManager;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void SetupInteract(ActionHotkey hotkey)
        {
            _actionHotkeyView.Set(hotkey);
        }
    }
}