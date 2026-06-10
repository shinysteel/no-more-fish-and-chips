using NoMoreFishAndChips.Localisation;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class LobbyContainer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;

        private LocalisationManager _localisationManager;

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
        }

        public void Setup(LocalisationTerm titleTerm, int entryCount)
        {
            _titleText.text = $"{_localisationManager.GetString(titleTerm)} ({entryCount})";
        }
    }
}