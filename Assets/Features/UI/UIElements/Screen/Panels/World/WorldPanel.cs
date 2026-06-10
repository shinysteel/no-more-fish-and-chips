using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class WorldPanel : Panel
    {
        [SerializeField] private TMP_InputField _worldNameInputField;
        [SerializeField] private ToggleGroup _hostModeToggleGroup;
        [SerializeField] private Button _hostButton;

        private SaveManager _saveManager;
        private LobbyManager _lobbyManager;
        private LocalisationManager _localisationManager;

        private SaveFile _saveFile;

        private void Awake()
        {
            _saveManager = GameManager.Instance.Get<SaveManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _localisationManager = GameManager.Instance.Get<LocalisationManager>();
        }

        public void Setup(SaveFile file)
        {
            _saveFile = file;

            _worldNameInputField.text = _saveFile?.Name ?? _localisationManager.GetString(LocalisationTerm.WorldPanelDefaultName);
        }

        private void Start()
        {
            _hostButton.onClick.AddListener(HostPressed);
        }

        private void HostPressed()
        {
            SaveFile file = _saveFile;

            if (file == null)
            {
                file = new SaveFile(null);
                _saveManager.AddSaveFile(file);
            }

            _saveManager.SelectSaveFile(file);

            ELobbyService service = _hostModeToggleGroup.GetFirstActiveToggle().transform.GetSiblingIndex() switch
            {
                0 => ELobbyService.LAN,
                1 => ELobbyService.Steam,
                _ => ELobbyService.LAN
            };

            _lobbyManager.SetLobbyService(service);

            _ = _lobbyManager.CreateLobbyAsync();
        }
    }
}