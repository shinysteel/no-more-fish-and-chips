using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    // The name ActionHotkey is taken by an enum
    public class ActionHotkeyView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _image;

        [SerializeField] private Sprite _leftClickSprite;
        [SerializeField] private Sprite _rightClickSprite;

        public void Set(ActionHotkey hotkey)
        {
            _text.gameObject.SetActive(false);
            _image.gameObject.SetActive(false);

            if (hotkey == ActionHotkey.FKey)
            {
                _text.text = "F";
                _text.gameObject.SetActive(true);
            }
            else if (hotkey == ActionHotkey.LeftClick)
            {
                _image.sprite = _leftClickSprite;
                _image.gameObject.SetActive(true);
            }
            else if (hotkey == ActionHotkey.RightClick)
            {
                _image.sprite = _rightClickSprite;
                _image.gameObject.SetActive(true);
            }
        }
    }
}