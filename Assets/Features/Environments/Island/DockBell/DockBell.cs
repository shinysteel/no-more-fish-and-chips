using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class DockBell : MonoBehaviour, IInteractable
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        private UIManager _uiManager;
        private NetworkManager _networkManager;

        private MeshRenderer[] _meshRenderers;

        Vector3 IInteractable.Position => transform.position;
        IInteractableSettings IInteractable.IInteractableSettings => _iInteractableSettings;
        IEnumerable<Renderer> IInteractable.Renderers => _meshRenderers;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
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

            AudioManager.PlaySoundRpc(SoundId.DockBellRing, 0f);
        }
    }
}