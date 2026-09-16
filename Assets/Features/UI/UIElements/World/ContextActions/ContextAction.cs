using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class ContextAction : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private ActionHotkeyView _hotkeyView;
        [SerializeField] private Image _image;
        
        public void Setup(ActionData data)
        {
            _hotkeyView.Set(data.Hotkey);
            _image.sprite = data.Sprite;
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}