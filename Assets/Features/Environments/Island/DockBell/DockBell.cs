using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class DockBell : MonoBehaviour, IInteractable
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        private UIManager _uiManager;
        private NetworkManager _networkManager;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        bool IInteractable.CanInteract()
        {
            return !_networkManager.LocalPurrnetPlayer.RaftPlayer.ReadyLogic.IsReady;
        }

        bool IInteractable.CanPrompt()
        {
            return ((IInteractable)(this)).CanInteract();
        }

        WorldUI IInteractable.CreatePromptUI()
        {
            InteractPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(_iInteractableSettings.Hotkey);
            return ui;
        }

        void IInteractable.Interact()
        {
            if (_networkManager.LocalPurrnetPlayer.RaftPlayer.ReadyLogic.IsReady)
            {
                return;
            }

            _networkManager.LocalPurrnetPlayer.RaftPlayer.ReadyLogic.SetNetIsReady(true);

            AudioManager.PlaySoundRpc(SoundId.DockBellRing);
        }
    }
}