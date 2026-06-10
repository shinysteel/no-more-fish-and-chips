using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    public abstract class ItemActionData : ScriptableObject
    {
        [SerializeField] protected ActionHotkey _hotkey;
        [SerializeField] private Sprite _sprite;

        public ActionHotkey Hotkey => _hotkey;
        public Sprite Sprite => _sprite;

        public abstract void Execute(GameplayContext context);
    }
}