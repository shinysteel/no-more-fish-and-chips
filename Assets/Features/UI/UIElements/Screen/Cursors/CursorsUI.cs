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
                createElement: () => _poolManager.GetTypedPoolable<Cursor>(new SpawnParams() { Parent = transform }),
                removeElement: (Cursor cursor) => _poolManager.ReturnTypedPoolable(cursor),
                processElement: (Cursor cursor, int index) =>
                {
                    cursor.Setup(_context);
                    cursor.SetOwner(_context.Players[index]);
                });
        }

        void IEntityManagerListener.OnEntitySpawned(IEntity entity)
        {
            if (entity is not RaftPlayer player)
            {
                return;
            }
            
            SyncCursors();

            HandleOpenNetBehaviourChanged(null, player.OpenNetBehaviourLogic.Behaviour);

            player.OpenNetBehaviourLogic.OnBehaviourChanged += HandleOpenNetBehaviourChanged;
        }

        void IEntityManagerListener.OnEntityDespawned(IEntity entity)
        { 
            if (entity is not RaftPlayer player)
            {
                return;
            }

            SyncCursors();

            player.OpenNetBehaviourLogic.OnBehaviourChanged -= HandleOpenNetBehaviourChanged;
        }

        private void HandleOpenNetBehaviourChanged(NetBehaviour oldBehaviour, NetBehaviour newBehaviour)
        {
            // Cursors besides the local player have all visuals off by default
            foreach (Cursor cursor in _cursors)
            {
                if (cursor.Owner.IsLocalPlayer)
                {
                    continue;
                }

                cursor.SetVisualsActive(false);
            }

            // If the local player has no behaviour open, we can stop here
            if (_context.LocalPlayer.OpenNetBehaviourLogic.Behaviour == null)
            {
                return;
            }

            // Show the visuals of other player cursors if have the same behaviour open
            for (int i = 0; i < _context.Players.Count; i++)
            {
                if (_context.Players[i].IsLocalPlayer)
                {
                    continue;
                }

                if (_context.Players[i].OpenNetBehaviourLogic.Behaviour == _context.LocalPlayer.OpenNetBehaviourLogic.Behaviour)
                {
                    _cursors[i].SetVisualsActive(true);
                }
            }
        }
    }
}