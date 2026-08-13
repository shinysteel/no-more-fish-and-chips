using UnityEngine;
using UnityEngine.UI;
using System;
using NoMoreFishAndChips.States;
using TMPro;
using NoMoreFishAndChips.Localisation;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.UI
{
    public class VoyageResultsScreen : ScreenUI
    {
        [SerializeField] private TextMeshProUGUI _outcomeText;
        [SerializeField] private Button _returnToLobbyButton;

        private LocalisationManager _localisationManager;

        private GameplayContext _context;
        private Action _handleReturnToLobbyPressed;

        private void Awake()
        {
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();

            _returnToLobbyButton.onClick.AddListener(HandleReturnToLobbyPressed);
        }

        public void Setup(GameplayContext context, Action handleReturnToLobbyPressed)
        {
            _context = context;
            _handleReturnToLobbyPressed = handleReturnToLobbyPressed;

            _outcomeText.text = _localisationManager.GetString(_context.VoyageRunner.VoyageResult == VoyageResult.Victory ? LocalisationTerm.VoyageResultsScreenVictory : LocalisationTerm.VoyageResultsScreenDefeat);
        }

        private void HandleReturnToLobbyPressed()
        {
            _handleReturnToLobbyPressed?.Invoke();
        }
    }
}