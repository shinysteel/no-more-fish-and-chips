using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Networking
{
    public enum ELobbyService
    {
        None,
        LAN,
        Steam,
    }

    public interface ILobbyManagerListener
    {
        void OnLobbyCreated(Lobby lobby) { }
        void OnLobbyEnter(Lobby lobby) { }
        void OnLobbyStart(Lobby lobby) { }
        void OnLobbyLeave() { }
    }

    public class LobbyManager : GameSystem<ILobbyManagerListener>
    {
        private LobbyManagerConfig _config;
        public LobbyManagerConfig Config => _config;

        private LANLobbyService _lanLobbyService;
        private SteamLobbyService _steamLobbyService;

        private Dictionary<ELobbyService, LobbyService> _lobbyServices = new();
        private LobbyService _currentLobbyService;

        public Lobby CurrentLobby => _currentLobbyService.CurrentLobby;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.LobbyManagerConfig;

            _lanLobbyService = new();
            _steamLobbyService = new();

            _lobbyServices.Add(ELobbyService.None, null);
            _lobbyServices.Add(ELobbyService.LAN, _lanLobbyService);
            _lobbyServices.Add(ELobbyService.Steam, _steamLobbyService);

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            foreach (LobbyService service in _lobbyServices.Values)
            {
                service?.Shutdown();
            }

            SetLobbyService(ELobbyService.None);

            base.Shutdown();
        }

        public void SetLobbyService(ELobbyService service)
        {
            if (_currentLobbyService != null)
            {
                _currentLobbyService.OnLobbyCreated -= HandleLobbyCreated;
                _currentLobbyService.OnLobbyEnter -= HandleLobbyEnter;
                _currentLobbyService.OnLobbyStart -= HandleLobbyStart;
                _currentLobbyService.OnLobbyLeave -= HandleLobbyLeave;
            }

            if (!_lobbyServices.TryGetValue(service, out _currentLobbyService))
            {
                Log.Error("Trying to set a lobby service that is not defined");
            }

            // Lobby service should never be null after the first time it's set 
            if (_currentLobbyService != null)
            {
                _currentLobbyService.OnLobbyCreated += HandleLobbyCreated;
                _currentLobbyService.OnLobbyEnter += HandleLobbyEnter;
                _currentLobbyService.OnLobbyStart += HandleLobbyStart;
                _currentLobbyService.OnLobbyLeave += HandleLobbyLeave;
            }
        }

        public async Task<Dictionary<ELobbyService, Lobby[]>> SearchLobbies()
        {
            List<Task<KeyValuePair<ELobbyService, Lobby[]>>> tasks = new();

            foreach (var kvp in _lobbyServices)
            {
                if (kvp.Key == ELobbyService.None)
                {
                    continue;
                }

                tasks.Add(Task.Run(async () => new KeyValuePair<ELobbyService, Lobby[]>(kvp.Key, await kvp.Value.SearchLobbiesAsync())));
            }

            KeyValuePair<ELobbyService, Lobby[]>[] kvps = await Task.WhenAll(tasks);

            return kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<Lobby> CreateLobbyAsync()
        {
            Lobby lobby = await _currentLobbyService.CreateLobbyAsync();
            return lobby;
        }

        public async Task<Lobby> JoinLobbyAsync(string lobbyId)
        {
            Lobby lobby = await _currentLobbyService.JoinLobbyAsync(lobbyId);
            return lobby;
        }

        public void StartLobby()
        {
            _currentLobbyService.StartLobby();
        }

        public void LeaveLobby()
        {
            _currentLobbyService.LeaveLobby();

            SetLobbyService(ELobbyService.None);
        }

        public bool IsLobbyOwner(Lobby lobby)
        {
            return _currentLobbyService.IsLobbyOwner(lobby);
        }

        private void HandleLobbyCreated(Lobby lobby) => NotifyLobbyCreated(lobby);
        private void HandleLobbyEnter(Lobby lobby) => NotifyLobbyEnter(lobby);
        private void HandleLobbyStart(Lobby lobby) => NotifyLobbyStart(lobby);
        private void HandleLobbyLeave() => NotifyLobbyLeave();

        private void NotifyLobbyCreated(Lobby lobby) => Listeners.Dispatch(listener => listener.OnLobbyCreated(lobby));
        private void NotifyLobbyEnter(Lobby lobby) => Listeners.Dispatch(listener => listener.OnLobbyEnter(lobby));
        private void NotifyLobbyStart(Lobby lobby) => Listeners.Dispatch(listener => listener.OnLobbyStart(lobby));
        private void NotifyLobbyLeave() => Listeners.Dispatch(listener => listener.OnLobbyLeave());
    }
}