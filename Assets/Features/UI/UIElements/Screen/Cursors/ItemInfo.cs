using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Rarities;
using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class ItemInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _rarityText;

        private LocalisationManager _localisationManager;
        private RarityManager _rarityManager;

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
            _rarityManager = GameManager.Instance.Get<RarityManager>();
        }

        public void Set(ItemDefinitionData data)
        {
            Color rarityColor = _rarityManager.GetColor(data.Rarity);
            _nameText.color = rarityColor;
            _rarityText.color = rarityColor;

            _nameText.text = _localisationManager.GetString(data.NameTerm);
            _descriptionText.text = _localisationManager.GetString(data.DescriptionTerm);
            _rarityText.text = $"{data.Rarity} {data.ItemType}";
        }
    }
}