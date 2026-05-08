using FishFlingers.Instantiating;
using FishFlingers.Localisation;
using FishFlingers.Networking;
using FishFlingers.Pools;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using Object = UnityEngine.Object;

namespace FishFlingers.UI
{
    public class BrowseGamesPanel : Panel
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GameObject _loadingSpinnerGameObject;

        [SerializeField] private LobbyContainer _lobbyContainerPrefab;

        [SerializeField] private LobbyContainerModel[] _lobbyContainerModels;

        private PoolManager _poolManager;
        private LobbyManager _lobbyManager;

        private Dictionary<ELobbyService, LobbyContainerModel> _serviceModelMap = new();

        private float _searchTimer;

        private const float SearchInterval = 2.5f;

        [Serializable]
        private class LobbyContainerModel
        {
            [SerializeField] private ELobbyService _lobbyService;
            [SerializeField] private LocalisationTerm _titleTerm;

            private LobbyContainer _container;

            public ELobbyService LobbyService => _lobbyService;
            public LocalisationTerm TitleTerm => _titleTerm;
            public LobbyContainer Container => _container;

            public List<LobbyEntry> Entries { get; private set; } = new();

            public void SetContainer(LobbyContainer container)
            {
                _container = container;
            }
        }

        public override void Load(Canvas canvas)
        {
            base.Load(canvas);

            _poolManager = GameManager.Instance.Get<PoolManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();

            foreach (LobbyContainerModel model in _lobbyContainerModels)
            {
                LobbyContainer container = Object.Instantiate(_lobbyContainerPrefab, _scrollRect.content);
                container.Setup(model.TitleTerm, 0);
                container.gameObject.SetActive(false);

                model.SetContainer(container);

                _serviceModelMap.Add(model.LobbyService, model);
            }
        }

        public override void Show(Action onComplete)
        {
            base.Show(onComplete);

            _ = SearchAsync();
        }

        public override void Unload()
        {
            foreach (LobbyContainerModel model in _lobbyContainerModels)
            {
                foreach (LobbyEntry entry in model.Entries)
                {
                    _poolManager.ReturnTypedPoolable(entry);
                }
            }
        }

        private void Update()
        {
            AutoSearchUpdate();
        }

        private void AutoSearchUpdate()
        {
            if (!_isShowing)
            {
                return;
            }

            _searchTimer += Time.deltaTime;
            if (_searchTimer < SearchInterval)
            {
                return;
            }

            _ = SearchAsync();
        }

        private async Task SearchAsync()
        {
            _searchTimer = 0f;

            Dictionary<ELobbyService, Lobby[]> lobbies = await _lobbyManager.SearchLobbies();

            _loadingSpinnerGameObject.SetActive(false);

            foreach (ELobbyService service in lobbies.Keys)
            {
                SyncLobbyEntries(_serviceModelMap[service], lobbies[service]);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
        }

        // Use pooling once we allow the scroll rect to display only what is on screen
        private void SyncLobbyEntries(LobbyContainerModel model, Lobby[] lobbies)
        {
            Utils.Collections.ResizeList(model.Entries, lobbies.Length,
                createElement: () => _poolManager.GetTypedPoolable<LobbyEntry>(new SpawnParams() { Parent = model.Container.transform }),
                removeElement: (LobbyEntry entry) => _poolManager.ReturnTypedPoolable(entry),
                processElement: (LobbyEntry entry, int index) => entry.Setup(lobbies[index]));

            model.Container.gameObject.SetActive(true);
            model.Container.Setup(model.TitleTerm, lobbies.Length);
        }
    }
}