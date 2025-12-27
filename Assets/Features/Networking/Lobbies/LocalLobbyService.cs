using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Net;
using PurrNet.Transports;
using ShinyOwl.Common.Utils;
using PurrNet.Packing;
using PurrNet;

namespace FishFlingers.Networking
{
    public class LocalLobbyService : ILobbyService, INetworkManagerListener
    {
        private NetworkManager _networkManager;

        private Dictionary<string, Lobby> _knownLobbies = new();
        private Lobby _currentLobby;

        private UdpClient _broadcastClient;
        private bool _isBroadcasting;

        private UdpClient _listenerClient;
        private bool _isListening;

        private const int BroadcastPort = 5000;
        private const int BroadcastInterval = 2500; // ms

        private const int DefaultMemberLimit = 4;

        private const string AddressKey = "address";

        private struct JoinAcceptMessage : IPackedAuto
        {
            public string lobbyId;
        }

        public LocalLobbyService()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _broadcastClient = new();
            _broadcastClient.EnableBroadcast = true;

            _listenerClient = new();

            StartListening();
        }

        public void Shutdown()
        {
            StopBroadcasting();
            StopListening();

            _broadcastClient?.Close();
            _broadcastClient?.Dispose();

            _listenerClient?.Close();
            _listenerClient?.Dispose();
        }

        public Task<Lobby[]> SearchLobbiesAsync()
        {
            return Task.FromResult(_knownLobbies.Values.ToArray());
        }

        public Task<Lobby> CreateLobbyAsync()
        {
            string ownerId = _networkManager.LocalPlayer.ToString();
            string name = $"{ownerId}'s Lobby";
            string lobbyId = Guid.NewGuid().ToString();
            List<LobbyMember> members = new() { new LobbyMember(ownerId, ownerId) };
            Dictionary<string, string> properties = new() { { AddressKey, Utils.Network.GetLocalIpAddress() } };

            _currentLobby = new Lobby(name, lobbyId, ownerId, DefaultMemberLimit, members, properties);

            StartBroadcasting();

            return Task.FromResult(_currentLobby);
        }

        public Task<Lobby> JoinLobbyAsync(string lobbyId)
        {
            if (_knownLobbies.TryGetValue(lobbyId, out Lobby lobby))
            {
                return Task.FromException<Lobby>(new Exception("Could not find a lobby with matching id"));
            }

            if (lobby.Properties.TryGetValue(AddressKey, out string address))
            {
                return Task.FromException<Lobby>(new Exception("This lobby is not providing an address to join to"));
            }

            _networkManager.Transport.address = address;
            _networkManager.StartClient();

            // await connection
            // await accept while listening for kick event (reject)

            return null;
        }

        public void StartLobby()
        {

        }

        public void LeaveLobby()
        {
            StopBroadcasting();
        }

        private void StartBroadcasting()
        {
            if (_currentLobby.OwnerId != _networkManager.LocalPlayer.ToString())
            {
                Debugger.LogError(this, "Tried to broadcast a lobby we do not own");
                return;
            }

            if (_isBroadcasting)
            {
                Debugger.LogError(this, "Only one broadcast task should be active at a time");
                return;
            }

            _isBroadcasting = true;

            Task.Run(async () =>
            {
                while (_isBroadcasting)
                {
                    string json = JsonUtility.ToJson(_currentLobby);
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    await _broadcastClient.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, BroadcastPort));
                    await Task.Delay(BroadcastInterval);
                }
            });
        }

        private void StartListening()
        {
            if (_isListening)
            {
                Debugger.LogError(this, "Only one listener task should be active at a time");
                return;
            }

            _isListening = true;

            Task.Run(async () =>
            {
                while (_isListening)
                {
                    UdpReceiveResult result = await _listenerClient.ReceiveAsync();
                    string json = Encoding.UTF8.GetString(result.Buffer);
                    Lobby lobby = JsonUtility.FromJson<Lobby>(json);
                    _knownLobbies[lobby.LobbyId] = lobby;
                }
            });
        }

        private void StopBroadcasting()
        {
            _isBroadcasting = false;
        }

        private void StopListening()
        {
            _isListening = false;
        }

        public void OnNetworkStarted(bool asServer)
        {
            if (asServer)
            {
                return;
            }

            _networkManager.Subscribe<JoinAcceptMessage>(HandleJoinAcceptMessage, _networkManager.IsServer);
        }

        public void OnNetworkShutdown(bool asServer)
        {
            if (asServer)
            {
                return;
            }

            _networkManager.Unsubscribe<JoinAcceptMessage>(HandleJoinAcceptMessage);
        }

        public void OnPlayerLeft(PlayerID id) 
        {
            _currentLobby.Members.RemoveAll(member => member.Id == id.ToString());
        }

        public void OnPlayerJoined(PlayerID id, bool isReconnect) 
        { 
            if (_currentLobby.Members.Count >= _currentLobby.MemberLimit)
            {
                _networkManager.KickPlayer(id);
                return;
            }

            LobbyMember member = new LobbyMember(id.ToString(), $"Player {id}");
            _currentLobby.Members.Add(member);
            _networkManager.Send(id, new JoinAcceptMessage() { lobbyId = _currentLobby.LobbyId });
        }

        private void HandleJoinAcceptMessage(PlayerID id, JoinAcceptMessage message, bool asServer)
        {
            Debugger.Log(this, $"Received join accept message. Id: {id}, message.lobbyyId: {message.lobbyId}, asServer: {asServer}");
        }

        public void OnLobbyCreated(SteamLobby lobby) { }
        public void OnLobbyEnter(SteamLobby lobby) { }
        public void OnLobbyLeave() { }
        public void OnLobbyGameServerSet() { }
        public void OnClientConnectionState(ConnectionState state) { }
    }
}