using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.UI;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class VoyageBoard : MonoBehaviour, IInteractable
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        private UIManager _uiManager;

        private MeshRenderer[] _meshRenderers;

        private PanelInstance<VoyageBoardPanel> _voyageBoardPanelInstance;
        
        Vector3 IInteractable.Position => transform.position;
        IInteractableSettings IInteractable.IInteractableSettings => _iInteractableSettings;
        IEnumerable<Renderer> IInteractable.Renderers => _meshRenderers;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _meshRenderers = GetComponentsInChildren<MeshRenderer>();

            _voyageBoardPanelInstance = new PanelInstance<VoyageBoardPanel>(_uiManager.Config.VoyageBoardPanelPrefab);
        }

        bool IInteractable.CanInteract()
        {
            return true;
        }

        bool IInteractable.CanPrompt()
        {
            return true;
        }

        WorldUI IInteractable.CreatePromptUI()
        {
            InteractPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(_iInteractableSettings.Hotkey);
            return ui;
        }

        void IInteractable.Interact()
        {
            _voyageBoardPanelInstance.Toggle(null);
        }
    }
}