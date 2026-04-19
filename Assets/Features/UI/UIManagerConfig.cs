using FishFlingers.UI.Transitions;
using System;
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

        [Header("Layers")]
        [SerializeField] private Layer _layerPrefab;

        public Layer LayerPrefab => _layerPrefab;

        [Header("ScreenUI - Screens")]
        [SerializeField] private MainMenuScreen _mainMenuScreenPrefab;
        [SerializeField] private GameplayScreen _gameplayScreenPrefab;

        public MainMenuScreen MainMenuScreenPrefab => _mainMenuScreenPrefab;
        public GameplayScreen GameplayScreenPrefab => _gameplayScreenPrefab;

        [Header("ScreenUI - Panels")]
        [SerializeField] private BrowseGamesPanel _browseGamesPanelPrefab;
        [SerializeField] private HostGamePanel _hostGamePanelPrefab;
        [SerializeField] private FishingBagPanel _fishingBagPanelPrefab;
        [SerializeField] private BuildingKitPanel _buildingKitPanelPrefab;
        [SerializeField] private ClamChestPanel _clamChestPanel;
        [SerializeField] private WorldPanel _worldPanel;

        public BrowseGamesPanel BrowseGamesPanelPrefab => _browseGamesPanelPrefab;
        public HostGamePanel HostGamePanelPrefab => _hostGamePanelPrefab;
        public FishingBagPanel FishingBagPanelPrefab => _fishingBagPanelPrefab;
        public BuildingKitPanel BuildingKitPanelPrefab => _buildingKitPanelPrefab;
        public ClamChestPanel ClamChestPanel => _clamChestPanel;
        public WorldPanel WorldPanel => _worldPanel;

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