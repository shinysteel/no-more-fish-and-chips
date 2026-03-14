using PurrNet;
using PurrNet.Packing;
using PurrNet.Transports;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using ParrelSync;
using FishFlingers.Scenes;

namespace FishFlingers.Networking
{
    // These packets are 1:1 with the base class Lobby & LobbyMember. It's only here to use the LANLobby
    // constructor, which is nice as it initialises TimeCreated and whatever else we add later
    public struct LANLobbyMemberPacket : IPackedAuto
    {
        public string Id;
        public string DisplayName;

        public static LANLobbyMemberPacket FromMember(LobbyMember member)
        {
            return new LANLobbyMemberPacket()
            {
                Id = member.Id,
                DisplayName = member.DisplayName
            };
        }

        public static LobbyMember ToMember(LANLobbyMemberPacket packet)
        {
            return new LobbyMember(packet.Id, packet.DisplayName);
        }
    }

    public struct LANLobbyPacket : IPackedAuto
    {
        public string Name;
        public string LobbyId;
        public string OwnerId;
        public int MemberLimit;
        public List<LANLobbyMemberPacket> Members;
        public Dictionary<string, string> Properties;

        public static LANLobbyPacket FromLobby(LANLobby lobby)
        {
            return new LANLobbyPacket()
            {
                Name = lobby.Name,
                LobbyId = lobby.LobbyId,
                OwnerId = lobby.OwnerId,
                MemberLimit = lobby.MemberLimit,
                Members = lobby.Members.Select(member => LANLobbyMemberPacket.FromMember(member)).ToList(),
                Properties = lobby.Properties
            };
        }

        public static LANLobby ToLobby(LANLobbyPacket packet)
        {
            return new LANLobby(new LobbyParams()
            {
                Name = packet.Name,
                LobbyId = packet.LobbyId,
                OwnerId = packet.OwnerId,
                MemberLimit = packet.MemberLimit,
                Members = packet.Members.Select(member => LANLobbyMemberPacket.ToMember(member)).ToList(),
                Properties = packet.Properties
            });
        }
    }

    public class LANLobby : Lobby
    {
        public DateTime TimeCreated { get; private set; }

        public LANLobby(LobbyParams parameters) : base(parameters)
        {
            TimeCreated = DateTime.UtcNow;
        }
    }

    public class LANLobbyService : LobbyService, INetworkManagerListener
    {
        private LobbyManager _lobbyManager;
        private NetworkManager _networkManager;

        private Dictionary<string, LANLobby> _knownLobbies = new();

        private string _lanId;

        private UdpClient _broadcastClient;
        private bool _isBroadcasting;

        private UdpClient _listenerClient;
        private bool _isListening;

        private const int BroadcastInterval = 2500; // ms

        private const float LobbyTimeout = 5f;

        private const string AddressKey = "address";

        public LANLobbyService()
        {
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _networkManager.AddListener(this);

            _lanId = $"{Environment.UserName}-{Guid.NewGuid().ToString().Substring(0, 5)}";

            _broadcastClient = new();
            _broadcastClient.EnableBroadcast = true;

            // Allow testing on the same computer. The clone just won't be able to listen, but
            // they will still broadcast to our main editor
            if (!ClonesManager.IsClone())
            {
                _listenerClient = new UdpClient(_lobbyManager.Config.BroadcastPort);

                StartListening();
            }
        }

        public override void Shutdown()
        {
            _networkManager?.RemoveListener(this);

            StopBroadcasting();
            StopListening();

            _broadcastClient?.Close();
            _broadcastClient?.Dispose();

            _listenerClient?.Close();
            _listenerClient?.Dispose();
        }

        public override Task<Lobby[]> SearchLobbiesAsync()
        {
            List<string> expiredLobbies = _knownLobbies
                .Where(kvp => (DateTime.UtcNow - kvp.Value.TimeCreated).TotalSeconds >= LobbyTimeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string key in expiredLobbies)
            {
                _knownLobbies.Remove(key);
            }

            return Task.FromResult(_knownLobbies.Values.Cast<Lobby>().ToArray());
        }

        public override Task<Lobby> CreateLobbyAsync()
        {
            string address = Utils.Network.GetLocalIpAddress();
            string ownerId = _lanId;

            CurrentLobby = new LANLobby(new LobbyParams() 
            { 
                Name = $"{ownerId}'s Lobby",
                LobbyId = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                MemberLimit = DefaultMemberLimit,
                Members = new List<LobbyMember>() { new LobbyMember(ownerId, ownerId) },
                Properties = new Dictionary<string, string>() { { AddressKey, address }, { StartedKey, false.ToString() } }
            });

            _networkManager.SetClientTransport<UDPTransport>();
            _networkManager.TryGetClientTransport(out UDPTransport transport);
            transport.address = address;

            _networkManager.StartServer();
            _networkManager.StartClient();

            StartBroadcasting();

            RaiseOnLobbyCreated(CurrentLobby);
            RaiseOnLobbyEnter(CurrentLobby);

            return Task.FromResult(CurrentLobby);
        }

        public override Task<Lobby> JoinLobbyAsync(string lobbyId)
        {
            if (!_knownLobbies.TryGetValue(lobbyId, out LANLobby lobby))
            {
                return Task.FromException<Lobby>(new Exception("Could not find a lobby with matching id"));
            }

            if (!lobby.Properties.TryGetValue(AddressKey, out string address))
            {
                return Task.FromException<Lobby>(new Exception("This lobby is not providing an address to join to"));
            }

            CurrentLobby = lobby;

            _networkManager.SetClientTransport<UDPTransport>();
            _networkManager.TryGetClientTransport(out UDPTransport transport);
            transport.address = address;

            _networkManager.StartClient();

            RaiseOnLobbyEnter(lobby);

            RaiseLobbyEvents(null, lobby);

            return Task.FromResult((Lobby)lobby);
        }

        public override void StartLobby()
        {
            if (CurrentLobby.OwnerId != _lanId)
            {
                Log.Error("You need to be the host to start the lobby");
                return;
            }

            CurrentLobby.Properties[StartedKey] = true.ToString();

            RaiseOnLobbyStart(CurrentLobby);
        }

        public override void LeaveLobby()
        {
            CurrentLobby = null;

            StopBroadcasting();

            RaiseOnLobbyLeave();
        }

        public override bool IsLobbyOwner(Lobby lobby)
        {
            return lobby.OwnerId == _lanId;
        }

        private void StartBroadcasting()
        {
            if (CurrentLobby.OwnerId != _lanId)
            {
                Log.Error("Tried to broadcast a lobby we do not own");
                return;
            }

            if (_isBroadcasting)
            {
                Log.Error("Only one broadcast task should be active at a time");
                return;
            }

            _isBroadcasting = true;

            Task.Run(async () =>
            {
                while (_isBroadcasting)
                {
                    LANLobbyPacket packet = LANLobbyPacket.FromLobby((LANLobby)CurrentLobby);
                    using BitPacker writer = BitPackerPool.Get();
                    Packer<LANLobbyPacket>.Write(writer, packet);

                    byte[] bytes = writer.buffer;

                    await _broadcastClient.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, _lobbyManager.Config.BroadcastPort));
                    await Task.Delay(BroadcastInterval);
                }
            });
        }

        private void StartListening()
        {
            if (_isListening)
            {
                Log.Error("Only one listener task should be active at a time");
                return;
            }

            _isListening = true;

            Task.Run(async () =>
            {
                while (_isListening)
                {
                    try
                    {
                        UdpReceiveResult result = await _listenerClient.ReceiveAsync();
                        byte[] bytes = result.Buffer;
                        using BitPacker reader = BitPackerPool.Get(bytes);
                        LANLobbyPacket packet = default;
                        Packer<LANLobbyPacket>.Read(reader, ref packet);

                        LANLobby lobby = LANLobbyPacket.ToLobby(packet);

                        RaiseLobbyEvents(_knownLobbies.GetValueOrDefault(lobby.LobbyId), lobby);

                        _knownLobbies[lobby.LobbyId] = lobby;
                    }
                    catch { } // Preserve the loop and ignore
                }
            });
        }

        /// <summary>
        /// Since this is LAN, we have to manually relay changes to lobby properties that
        /// would normally raise events for all clients, such as OnLobbyStart
        /// </summary>
        /// <param name="previous">The lobby we are in, in its previous state</param>
        /// <param name="current">The lobby we are in, in its current state</param>
        private void RaiseLobbyEvents(LANLobby previous, LANLobby current)
        {
            // We only care if we are in this lobby
            if (current == null || CurrentLobby == null || current.LobbyId != CurrentLobby.LobbyId)
            {
                return;
            }

            // Ignore our own broadcasts, since we as the host raise them locally
            if (current.OwnerId == _lanId)
            {
                return;
            }

            // Detects when the 'started' property goes from false to true and relays it
            if (GetBool(previous, StartedKey) == false && GetBool(current, StartedKey) == true)
            {
                RaiseOnLobbyStart(current);
            }
        }

        private static bool GetBool(LANLobby lobby, string key)
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

        private void StopBroadcasting()
        {
            _isBroadcasting = false;
        }

        private void StopListening()
        {
            _isListening = false;
        }

        void INetworkManagerListener.OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer)
        {
            if (asServer)
            {
                return;
            }

            if (_networkManager.IsServer == false || _networkManager.LocalPlayerId == id)
            {
                return;
            }

            if (CurrentLobby.Members.Count >= CurrentLobby.MemberLimit)
            {
                _networkManager.KickPlayer(id);
                return;
            }

            LobbyMember member = new LobbyMember(id.ToString(), $"Player {id}");
            CurrentLobby.Members.Add(member);
        }

        void INetworkManagerListener.OnPlayerLeft(PlayerID id, bool asServer) 
        {
            if (asServer)
            {
                return;
            }

            if (_networkManager.IsServer == false || _networkManager.LocalPlayerId == id)
            {
                return;
            }

            CurrentLobby.Members.RemoveAll(member => member.Id == id.ToString());
        }
    }
}