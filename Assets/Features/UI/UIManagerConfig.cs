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
        [Header("Canvas & Event System")]
        [SerializeField] private Canvas _gameCanvasPrefab;
        [SerializeField] private EventSystem _eventSystemPrefab;

        public Canvas GameCanvasPrefab => _gameCanvasPrefab;
        public EventSystem EventSystemPrefab => _eventSystemPrefab;

        [Header("Screens")]
        [SerializeField] private MainMenuScreen _mainMenuScreen;
        [SerializeField] private GameplayScreen _gameplayScreen;

        public MainMenuScreen MainMenuScreen => _mainMenuScreen;
        public GameplayScreen GameplayScreen => _gameplayScreen;

        [Header("Panels")]
        [SerializeField] private BrowseGamesPanel _browseGamesPanel;
        [SerializeField] private FishingBagPanel _fishingBagPanel;
        [SerializeField] private BuildingKitPanel _buildingKitPanel;

        public BrowseGamesPanel BrowseGamesPanel => _browseGamesPanel;
        public FishingBagPanel FishingBagPanel => _fishingBagPanel;
        public BuildingKitPanel BuildingKitPanel => _buildingKitPanel;

        [Header("Overlays")]
        [SerializeField] private FadeOverlay _fadeOverlay;

        public FadeOverlay FadeOverlay => _fadeOverlay;
    }
}