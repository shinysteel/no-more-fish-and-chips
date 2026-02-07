using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.States;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using NetworkManager = FishFlingers.Networking.NetworkManager;

namespace FishFlingers.UI
{
    public class CursorsUI : ScreenUI, INetworkManagerListener
    {
        private PoolManager _poolManager;
        private NetworkManager _networkManager;

        private GameplayContext _context;

        private List<Cursor> _cursors = new();

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        public void Setup(GameplayContext context)
        {
            _context = context;

            SyncCursors();

            _networkManager.AddListener(this);
        }

        private void OnDestroy()
        {
            _networkManager?.RemoveListener(this);
        }

        private void Update()
        {
            CursorsUpdate();
        }

        /// <summary>
        /// Positions a cursor per player
        /// </summary>
        private void CursorsUpdate()
        {
            for (int i = 0; i < _context.Players.Count; i++)
            {
                Vector2 normalisedPos = _context.Players[i].MousePositionNormalised;
                Vector2 screenPos = new Vector2(Screen.width * normalisedPos.x, Screen.height * normalisedPos.y);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, null, out Vector2 localPos);

                _cursors[i].RectTransform.localPosition = localPos;
            }
        }

        // Cursor lifecycle
        private void SyncCursors()
        {
            int numCursors = _context.Players.Count;

            for (int i = _cursors.Count; i < numCursors; i++)
            {
                Cursor cursor = _poolManager.Get<Cursor>(new SpawnParams() { Parent = transform });
                _cursors.Add(cursor);
            }

            for (int i = _cursors.Count - 1; i >= numCursors; i--)
            {
                _poolManager.Return(_cursors[i]);
                _cursors.RemoveAt(i);
            }

            for (int i = 0; i < numCursors; i++)
            {
                _cursors[i].SetOwner(_context.Players[i]);
            }
        }

        public void OnNetworkSpawn(NetBehaviour behaviour) 
        { 
            if (behaviour is not RaftPlayer)
            {
                return;
            }

            SyncCursors();
        }

        public void OnNetworkDespawn(NetBehaviour behaviour) 
        { 
            if (behaviour is not RaftPlayer)
            {
                return;
            }

            SyncCursors();
        }

        public void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer) { }
        public void OnPlayerLeft(PlayerID id, bool asServer) { }
        public void OnNetworkStarted(bool asServer) { }
        public void OnNetworkShutdown(bool asServer) { }
        public void OnClientConnectionState(ConnectionState state) { }
    }
}