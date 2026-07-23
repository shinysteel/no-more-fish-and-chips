using UnityEngine;
using UnityEngine.UI;
using System;

namespace NoMoreFishAndChips.UI
{
    public class VoyageResultsScreen : ScreenUI
    {
        [SerializeField] private Button _returnToLobbyButton;

        private Action _handleReturnToLobbyPressed;

        private void Awake()
        {
            _returnToLobbyButton.onClick.AddListener(HandleReturnToLobbyPressed);
        }

        public void Setup(Action handleReturnToLobbyPressed)
        {
            _handleReturnToLobbyPressed = handleReturnToLobbyPressed;
        }

        private void HandleReturnToLobbyPressed()
        {
            _handleReturnToLobbyPressed?.Invoke();
        }
    }
}