using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Items
{
    public abstract class ItemActionData : ScriptableObject
    {
        [SerializeField] private InteractHotkey _interactHotkey;
        [SerializeField] private Sprite _hotkeySprite;

        public InteractHotkey InteractHotkey => _interactHotkey;
        public Sprite HotkeySprite => _hotkeySprite;

        public abstract void Execute(GameplayContext context);
    }
}