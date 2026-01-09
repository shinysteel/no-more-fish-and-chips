using FishFlingers.Cameras;
using FishFlingers.Scenes;
using PurrLobby;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using FishFlingers.Entities;

namespace FishFlingers.Networking
{
    public class PurrnetPlayer : NetBehaviour, ILobbyManagerListener
    {
        private LobbyManager _lobbyManager;

        protected override void OnInitializeModules()
        {
            base.OnInitializeModules();

            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            // If we've missed the OnLobbyStart event, let's invoke it here
            if (_lobbyManager.CurrentLobby.Properties[LobbyService.StartedKey] == true.ToString())
            {
                OnLobbyStart(_lobbyManager.CurrentLobby);
            }

            // We deliberately subscribe after invoking missed events
            _lobbyManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _lobbyManager?.RemoveListener(this);
        }

        protected override void OnOwnerDisconnected(PlayerID ownerId)
        {
            Destroy(gameObject);
        }

        public void OnLobbyStart(Lobby lobby)
        {
            // There used to be code for spawning a 'human' to control here.
            // Since we moved to Purrdiction, that's handled separately from
            // Purrnet. I'm leaving the implementation here since it's a nice
            // reference to look back on
        }

        public void OnLobbyEnter(Lobby lobby) { }
        public void OnLobbyCreated(Lobby lobby) { }
        public void OnLobbyLeave() { }
    }
}