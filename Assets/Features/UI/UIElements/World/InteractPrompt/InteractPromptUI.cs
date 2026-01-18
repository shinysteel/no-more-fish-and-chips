using TMPro;
using UnityEngine;

namespace FishFlingers.UI
{
    public class InteractPromptUI : WorldUI
    {
        public TMP_Text _text;

        public void Setup(string text)
        {
            _text.text = text;
        }
    }
}