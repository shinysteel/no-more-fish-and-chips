using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class SeaChartBoard : MonoBehaviour, IInteractable
    {
        [SerializeField] private IInteractableSettings _iInteractableSettings;

        private UIManager _uiManager;

        private PanelInstance<SeaChartPanel> _seaChartPanelInstance;

        public IInteractableSettings IInteractableSettings => _iInteractableSettings;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _seaChartPanelInstance = new PanelInstance<SeaChartPanel>(_uiManager.Config.SeaChartPanelPrefab);
        }

        public bool CanInteract()
        {
            return true;
        }

        public bool CanPrompt()
        {
            return true;
        }

        public WorldUI CreatePromptUI()
        {
            InteractPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(_iInteractableSettings.Hotkey);
            return ui;
        }

        public void Interact()
        {
            _seaChartPanelInstance.Toggle(null);
        }
    }
}