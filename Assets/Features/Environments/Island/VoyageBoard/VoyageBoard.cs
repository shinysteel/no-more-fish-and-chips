using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.UI;
using System.Collections.Generic;
using UnityEngine;
using NoMoreFishAndChips.Interactions;

namespace NoMoreFishAndChips.Environments
{
    public class VoyageBoard : MonoBehaviour, IInteractable
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        private UIManager _uiManager;

        private MeshRenderer[] _meshRenderers;

        private PanelInstance<VoyageBoardPanel> _voyageBoardPanelInstance;
        public PanelInstance<VoyageBoardPanel> VoyageBoardPanelInstance => _voyageBoardPanelInstance;
        
        Vector3 IInteractable.Position => transform.position;
        IEnumerable<Renderer> IInteractable.Renderers => _meshRenderers;
        IInteractableSettings IInteractable.IInteractableSettings => _iInteractableSettings;
        private List<Interaction> _interactions;
        List<Interaction> IInteractable.Interactions => _interactions;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _meshRenderers = GetComponentsInChildren<MeshRenderer>();

            _voyageBoardPanelInstance = new PanelInstance<VoyageBoardPanel>(_uiManager.Config.VoyageBoardPanelPrefab);

            _interactions = new List<Interaction>() { new ViewVoyageBoardInteraction(this) };
        }
    }
}