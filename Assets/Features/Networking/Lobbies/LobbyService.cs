using PurrNet.Packing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FishFlingers.Networking
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
    }

    public class Lobby
    {
        public string Name { get; private set; }
        public string LobbyId { get; private set; }
        public string OwnerId { get; private set; }
        public int MemberLimit { get; private set; }
        public List<LobbyMember> Members { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        public Lobby(LobbyParams parameters)
        {
            Name = parameters.Name;
            LobbyId = parameters.LobbyId;
            OwnerId = parameters.OwnerId;
            MemberLimit = parameters.MemberLimit;

            // DTO best practice involves copying collections to ensure ownership
            Members = new List<LobbyMember>(parameters.Members);
            Properties = new Dictionary<string, string>(parameters.Properties);
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

        protected void RaiseOnLobbyCreated(Lobby lobby) => OnLobbyCreated?.Invoke(lobby);
        protected void RaiseOnLobbyEnter(Lobby lobby) => OnLobbyEnter?.Invoke(lobby);
        protected void RaiseOnLobbyStart(Lobby lobby) => OnLobbyStart?.Invoke(lobby);
        protected void RaiseOnLobbyLeave() => OnLobbyLeave?.Invoke();
    }
}