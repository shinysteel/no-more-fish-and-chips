using TMPro;
using UnityEngine;

namespace FishFlingers.UI
{
    public class InteractPromptUI : WorldUI
    {
        public TextMeshProUGUI _text;

        public void Setup(string text)
        {
            _text.text = text;
        }
    }
}