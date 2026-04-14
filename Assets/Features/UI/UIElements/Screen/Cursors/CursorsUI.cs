using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.States;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.UI
{
    public class CursorsUI : ScreenUI, IEntityManagerListener
    {
        private PoolManager _poolManager;
        private EntityManager _entityManager;

        private GameplayContext _context;

        private List<Cursor> _cursors = new();

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public void Setup(GameplayContext context)
        {
            _context = context;

            foreach (RaftPlayer player in _context.Players)
            {
                ((IEntityManagerListener)this).OnEntitySpawned(player);
            }

            _entityManager.AddListener(this);
        }

        private void OnDestroy()
        {
            _entityManager?.RemoveListener(this);
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
            Utils.Collections.ResizeList(_cursors, _context.Players.Count,
                createElement: () => _poolManager.GetPoolable<Cursor>(new SpawnParams() { Parent = transform }),
                removeElement: (Cursor cursor) => _poolManager.ReturnPoolable(cursor),
                processElement: (Cursor cursor, int index) => cursor.SetOwner(_context.Players[index]));
        }

        void IEntityManagerListener.OnEntitySpawned(IEntity entity)
        {
            if (entity is not RaftPlayer player)
            {
                return;
            }
            
            SyncCursors();

            HandleOpenNetworkIdChanged(player.OpenNetworkIdLogic.Id);

            player.OpenNetworkIdLogic.OnIdChanged += HandleOpenNetworkIdChanged;
        }

        void IEntityManagerListener.OnEntityDespawned(IEntity entity)
        { 
            if (entity is not RaftPlayer player)
            {
                return;
            }

            SyncCursors();

            player.OpenNetworkIdLogic.OnIdChanged -= HandleOpenNetworkIdChanged;
        }

        private void HandleOpenNetworkIdChanged(NetworkID id)
        {
            foreach (Cursor cursor in _cursors)
            {
                cursor.SetVisualsActive(false);
            }

            if (_context.LocalPlayer.OpenNetworkIdLogic.Id == default)
            {
                return;
            }

            for (int i = 0; i < _context.Players.Count; i++)
            {
                if (_context.Players[i].IsLocalPlayer)
                {
                    continue;
                }

                if (_context.Players[i].OpenNetworkIdLogic.Id == _context.LocalPlayer.OpenNetworkIdLogic.Id)
                {
                    _cursors[i].SetVisualsActive(true);
                }
            }
        }
    }
}