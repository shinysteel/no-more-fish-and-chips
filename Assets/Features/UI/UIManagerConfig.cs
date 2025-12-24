using FishFlingers.UI.Transitions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FishFlingers.UI
{
    [CreateAssetMenu(fileName = "UIManagerConfig", menuName = "Configs/Managers/UIManagerConfig")]
    public class UIManagerConfig : ScriptableObject
    {
        [Header("Canvas & Event System")]
        [SerializeField] private Canvas _gameCanvasPrefab;
        [SerializeField] private EventSystem _eventSystemPrefab;

        public Canvas GameCanvasPrefab => _gameCanvasPrefab;
        public EventSystem EventSystemPrefab => _eventSystemPrefab;

        [Header("Screens")]
        [SerializeField] private MainMenuScreen _mainMenuScreen;
        [SerializeField] private BrowseGamesScreen _browseGamesScreen;
        [SerializeField] private GameplayScreen _gameplayScreen;

        public MainMenuScreen MainMenuScreen => _mainMenuScreen;
        public BrowseGamesScreen BrowseGamesScreen => _browseGamesScreen;
        public GameplayScreen GameplayScreen => _gameplayScreen;

        [Header("Overlays")]
        [SerializeField] private FadeOverlay _fadeOverlay;

        public FadeOverlay FadeOverlay => _fadeOverlay;
    }
}