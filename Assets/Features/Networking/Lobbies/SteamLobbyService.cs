using PurrLobby;
using ShinyOwl.Common;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishFlingers.Networking
{
    public class SteamLobbyMember
    {
        public CSteamID Id { get; private set; }
        public string DisplayName { get; private set; }

        // public Texture2D Avatar { get; private set; }

        public SteamLobbyMember(CSteamID id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }

    public class SteamLobby
    {
        public string Name { get; private set; }
        public CSteamID LobbyId { get; private set; } = CSteamID.Nil;
        public CSteamID OwnerId { get; private set; } = CSteamID.Nil;
        public int MemberLimit { get; private set; }
        public SteamLobbyMember[] Members { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        public SteamLobby(string name, CSteamID lobbyId, CSteamID ownerId, int memberLimit, SteamLobbyMember[] members, Dictionary<string, string> properties)
        {
            Name = name;
            LobbyId = lobbyId;
            OwnerId = ownerId;
            MemberLimit = memberLimit;
            Members = members;
            Properties = properties;
        } 
    }

    public class SteamLobbyService
    {
        private CallResult<LobbyMatchList_t> _lobbyMatchListListener;
        private CallResult<LobbyCreated_t> _lobbyCreatedListener;
        private CallResult<LobbyEnter_t> _lobbyEnterListener;

        private SteamLobby _currentLobby;
        public SteamLobby CurrentLobby => _currentLobby;

        public event Action<SteamLobby> OnLobbyCreated;
        public event Action<SteamLobby> OnLobbyEnter;
        public event Action OnLobbyLeave;
        public event Action OnLobbyGameServerSet;

        // Replace these once the player can specify what lobby they want to create
        private const ELobbyType DefaultLobbyType = ELobbyType.k_ELobbyTypePublic;
        private const int DefaultMemberLimit = 4;

        private const string NameKey = "name";

        public void Shutdown()
        {
            _lobbyCreatedListener?.Dispose();
            _lobbyEnterListener?.Dispose();

            LeaveLobby();
        }

        private bool IsAvailable()
        {
            try
            {
                InteropHelp.TestIfAvailableClient();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<SteamLobby[]> SearchLobbiesAsync()
        {
            if (!IsAvailable())
            {
                return null;
            }

            // Tell steam to search for lobbies
            SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();

            // Listen for the result
            TaskCompletionSource<SteamLobby[]> tcs = new();
            _lobbyMatchListListener ??= CallResult<LobbyMatchList_t>.Create();
            _lobbyMatchListListener.Set(call, (LobbyMatchList_t lobbyMatchList, bool ioFailure) =>
            {
                SteamLobby[] lobbies = new SteamLobby[lobbyMatchList.m_nLobbiesMatching];

                for (int i = 0; i < lobbies.Length; i++)
                {
                    CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                    string name = SteamMatchmaking.GetLobbyData(lobbyId, NameKey);
                    CSteamID ownerId = SteamMatchmaking.GetLobbyOwner(lobbyId);
                    int memberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);
                    SteamLobbyMember[] members = GetLobbyMembers(lobbyId);
                    Dictionary<string, string> properties = GetLobbyProperties(lobbyId);

                    lobbies[i] = new SteamLobby(name, lobbyId, ownerId, memberLimit, members, properties);
                }

                tcs.SetResult(lobbies);
            });

            return await tcs.Task;
        }

        public async Task<SteamLobby> CreateLobbyAsync()
        {
            if (!IsAvailable())
            {
                return null;
            }

            CSteamID lobbyId = CSteamID.Nil;

            // Tell steam to create the lobby
            SteamAPICall_t call = SteamMatchmaking.CreateLobby(DefaultLobbyType, DefaultMemberLimit);

            // Listen for the result
            TaskCompletionSource<bool> tcs = new();
            _lobbyCreatedListener ??= CallResult<LobbyCreated_t>.Create();
            _lobbyCreatedListener.Set(call, (LobbyCreated_t lobbyCreated, bool ioFailure) =>
            {
                bool result = lobbyCreated.m_eResult == EResult.k_EResultOK && !ioFailure;
                if (result)
                {
                    lobbyId = new CSteamID(lobbyCreated.m_ulSteamIDLobby);
                }

                tcs.SetResult(result);
            });

            if (!await tcs.Task)
            {
                return null;
            }

            string lobbyName = $"{SteamFriends.GetPersonaName()}'s Lobby";
            SteamMatchmaking.SetLobbyData(lobbyId, NameKey, lobbyName);

            CSteamID ownerId = SteamMatchmaking.GetLobbyOwner(lobbyId);
            SteamLobbyMember[] members = GetLobbyMembers(lobbyId);
            Dictionary<string, string> properties = GetLobbyProperties(lobbyId);

            _currentLobby = new SteamLobby(lobbyName, lobbyId, ownerId, DefaultMemberLimit, members, properties);

            OnLobbyCreated?.Invoke(_currentLobby);
            OnLobbyEnter?.Invoke(_currentLobby);
            return _currentLobby;
        }

        public async Task<SteamLobby> JoinLobbyAsync(CSteamID lobbyId)
        {
            if (!IsAvailable())
            {
                return null;
            }

            // Tell steam to join a lobby
            SteamAPICall_t call = SteamMatchmaking.JoinLobby(lobbyId);

            // Listen for the result
            TaskCompletionSource<bool> tcs = new();
            _lobbyEnterListener ??= CallResult<LobbyEnter_t>.Create();
            _lobbyEnterListener.Set(call, (LobbyEnter_t lobbyEnter, bool ioFailure) =>
            {
                bool result = lobbyEnter.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess;
                tcs.SetResult(result);
            });

            if (!await tcs.Task)
            {
                return null;
            }

            string lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, NameKey);
            CSteamID ownerId = SteamMatchmaking.GetLobbyOwner(lobbyId);
            int memberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);
            SteamLobbyMember[] members = GetLobbyMembers(lobbyId);
            Dictionary<string, string> properties = GetLobbyProperties(lobbyId);

            _currentLobby = new SteamLobby(lobbyName, lobbyId, ownerId, memberLimit, members, properties);

            OnLobbyEnter?.Invoke(_currentLobby);
            return _currentLobby;
        }

        public void StartLobby()
        {
            if (!IsAvailable())
            {
                return;
            }

            if (_currentLobby == null)
            {
                return;
            }

            SteamMatchmaking.SetLobbyGameServer(_currentLobby.LobbyId, 0, 0, SteamUser.GetSteamID());
            OnLobbyGameServerSet?.Invoke();
        }

        public void LeaveLobby()
        {
            if (!IsAvailable())
            {
                return;
            }

            if (_currentLobby == null)
            {
                return;
            }

            SteamMatchmaking.LeaveLobby(_currentLobby.LobbyId);
            _currentLobby = null;
            OnLobbyLeave?.Invoke();
        }

        private SteamLobbyMember[] GetLobbyMembers(CSteamID lobbyId)
        {
            SteamLobbyMember[] members = new SteamLobbyMember[SteamMatchmaking.GetNumLobbyMembers(lobbyId)];

            for (int i = 0; i < members.Length; i++)
            {
                CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
                string displayName = SteamFriends.GetFriendPersonaName(memberId);

                members[i] = new SteamLobbyMember(memberId, displayName);
            }

            return members;
        }

        private Dictionary<string, string> GetLobbyProperties(CSteamID lobbyId)
        {
            Dictionary<string, string> properties = new();
            int count = SteamMatchmaking.GetLobbyDataCount(lobbyId);

            int keySize = 256;
            int valueSize = 256;

            for (int i = 0; i < count; i++)
            {
                if (SteamMatchmaking.GetLobbyDataByIndex(lobbyId, i, out string key, keySize, out string value, valueSize))
                {
                    key = key.TrimEnd('\0');
                    value = value.TrimEnd('\0');
                    properties[key] = value;
                }
            }

            return properties;
        }
    }
}