using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    public class Lobby
    {
        public string Name { get; private set; }
        public string LobbyId { get; private set; }
        public string OwnerId { get; private set; }
        public int MemberLimit { get; private set; }
        public List<LobbyMember> Members { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        public Lobby(string name, string lobbyId, string ownerId, int memberLimit, List<LobbyMember> members, Dictionary<string, string> properties)
        {
            Name = name;
            LobbyId = lobbyId;
            OwnerId = ownerId;
            MemberLimit = memberLimit;
            Members = members;
            Properties = properties;
        }
    }

    public interface ILobbyService
    {
        Task<Lobby[]> SearchLobbiesAsync();
        Task<Lobby> CreateLobbyAsync();
        Task<Lobby> JoinLobbyAsync(string lobbyId);
        void StartLobby();
        void LeaveLobby();
    }
}