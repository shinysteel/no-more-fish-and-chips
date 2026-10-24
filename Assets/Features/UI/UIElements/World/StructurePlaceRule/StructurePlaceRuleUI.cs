using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class StructurePlaceRuleUI : WorldUI
    {
        [SerializeField] private Image _tileImage;
        [SerializeField] private TextMeshProUGUI _slashText;
        [SerializeField] private Image _waterImage;

        public void Setup(bool tile, bool water)
        {
            _tileImage.gameObject.SetActive(tile);
            _waterImage.gameObject.SetActive(water);
            _slashText.gameObject.SetActive(tile && water);
        }
    }
}