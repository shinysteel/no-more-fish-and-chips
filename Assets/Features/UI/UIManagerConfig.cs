using FishFlingers.UI.Transitions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FishFlingers.UI
{
    [CreateAssetMenu(fileName = "UIManagerConfig", menuName = "Configs/Managers/UI/UIManagerConfig")]
    public class UIManagerConfig : ScriptableObject
    {
        [Header("Canvases")]
        [SerializeField] private Canvas _screenCanvasPrefab;
        [SerializeField] private Canvas _worldCanvasPrefab;
        [SerializeField] private EventSystem _eventSystemPrefab;

        public Canvas ScreenCanvasPrefab => _screenCanvasPrefab;
        public Canvas WorldCanvasPrefab => _worldCanvasPrefab;
        public EventSystem EventSystemPrefab => _eventSystemPrefab;

        [Header("ScreenUI - Screens")]
        [SerializeField] private MainMenuScreen _mainMenuScreenPrefab;
        [SerializeField] private GameplayScreen _gameplayScreenPrefab;

        public MainMenuScreen MainMenuScreenPrefab => _mainMenuScreenPrefab;
        public GameplayScreen GameplayScreenPrefab => _gameplayScreenPrefab;

        [Header("ScreenUI - Panels")]
        [SerializeField] private BrowseGamesPanel _browseGamesPanelPrefab;
        [SerializeField] private FishingBagPanel _fishingBagPanelPrefab;
        [SerializeField] private BuildingKitPanel _buildingKitPanelPrefab;

        public BrowseGamesPanel BrowseGamesPanelPrefab => _browseGamesPanelPrefab;
        public FishingBagPanel FishingBagPanelPrefab => _fishingBagPanelPrefab;
        public BuildingKitPanel BuildingKitPanelPrefab => _buildingKitPanelPrefab;

        [Header("ScreenUI - Cursors")]
        [SerializeField] private CursorsUI _cursorsUIPrefab;

        public CursorsUI CursorsUIPrefab => _cursorsUIPrefab;

        [Header("ScreenUI - Overlays")]
        [SerializeField] private FadeOverlay _fadeOverlayPrefab;

        public FadeOverlay FadeOverlayPrefab => _fadeOverlayPrefab;

        [Header("WorldUI")]
        [SerializeField] private InteractPromptUI _interactPromptUIPrefab;
        public InteractPromptUI InteractPromptUIPrefab => _interactPromptUIPrefab;
    }
}