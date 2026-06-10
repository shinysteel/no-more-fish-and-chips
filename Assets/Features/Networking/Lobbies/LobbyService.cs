using PurrNet.Packing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.Networking
{
    public class LobbyMember
    {
        public string Id { get; private set; }
        public string DisplayName { get; private set; }

        // public Texture2D Avatar { get; private set; }

        public LobbyMember(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }

    // DTO (Data Transfer Object)
    public class LobbyParams
    {
        public string Name { get; set; } = "Lobby";
        public string LobbyId { get; set; } = string.Empty;
        public string OwnerId { get; set; } = "Owner";
        public int MemberLimit { get; set; } = LobbyService.DefaultMemberLimit;
        public List<LobbyMember> Members { get; set; } = new();
        public Dictionary<string, string> Properties { get; set; } = new();
        public ELobbyService Service { get; set; } = ELobbyService.None;
    }

    public class Lobby
    {
        public string Name { get; private set; }
        public string LobbyId { get; private set; }
        public string OwnerId { get; private set; }
        public int MemberLimit { get; private set; }
        public List<LobbyMember> Members { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }
        public ELobbyService Service { get; private set; }

        public Lobby(LobbyParams parameters)
        {
            Name = parameters.Name;
            LobbyId = parameters.LobbyId;
            OwnerId = parameters.OwnerId;
            MemberLimit = parameters.MemberLimit;

            // DTO best practice involves copying collections to ensure ownership
            Members = new List<LobbyMember>(parameters.Members);
            SetProperties(new Dictionary<string, string>(parameters.Properties));

            Service = parameters.Service;
        }

        public void SetProperties(Dictionary<string, string> properties)
        {
            Properties = properties;
        }
    }

    public abstract class LobbyService
    {
        public Lobby CurrentLobby { get; protected set; }

        public const int DefaultMemberLimit = 4;

        public const string StartedKey = "started";

        public event Action<Lobby> OnLobbyCreated;
        public event Action<Lobby> OnLobbyEnter;
        public event Action<Lobby> OnLobbyStart;
        public event Action OnLobbyLeave;

        public abstract void Shutdown();
        public abstract Task<Lobby[]> SearchLobbiesAsync();
        public abstract Task<Lobby> CreateLobbyAsync();
        public abstract Task<Lobby> JoinLobbyAsync(string lobbyId);
        public abstract void StartLobby();
        public abstract void LeaveLobby();
        public abstract bool IsLobbyOwner(Lobby lobby);

        protected void RaiseLobbyCreated(Lobby lobby) => OnLobbyCreated?.Invoke(lobby);
        protected void RaiseLobbyEnter(Lobby lobby) => OnLobbyEnter?.Invoke(lobby);
        protected void RaiseLobbyStart(Lobby lobby) => OnLobbyStart?.Invoke(lobby);
        protected void RaiseLobbyLeave() => OnLobbyLeave?.Invoke();

        /// <summary>
        /// We have to manually relay changes to lobby properties
        /// </summary>
        /// <param name="previous">The lobby we are in, in its previous state</param>
        /// <param name="current">The lobby we are in, in its current state</param>
        protected void RaiseLobbyEvents(Lobby previous, Lobby current)
        {
            // We only care if we are in this lobby
            if (current == null || CurrentLobby == null || current.LobbyId != CurrentLobby.LobbyId)
            {
                return;
            }

            // Ignore our own broadcasts, since we as the host raise them locally
            if (IsLobbyOwner(current))
            {
                return;
            }

            // Detects when the 'started' property goes from false to true and relays it
            if (GetBool(previous, StartedKey) == false && GetBool(current, StartedKey) == true)
            {
                RaiseLobbyStart(current);
            }
        }

        private bool GetBool(Lobby lobby, string key)
        {
            if (lobby == null)
            {
                return false;
            }

            if (!lobby.Properties.TryGetValue(key, out string value))
            {
                return false;
            }

            if (!bool.TryParse(value, out bool result))
            {
                return false;
            }

            return result;
        }
    }
}