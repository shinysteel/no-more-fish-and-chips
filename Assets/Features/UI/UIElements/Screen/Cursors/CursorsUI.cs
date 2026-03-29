using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.States;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
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

            SyncCursors();

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
                createElement: () => _poolManager.Get<Cursor>(new SpawnParams() { Parent = transform }),
                removeElement: (Cursor cursor) => _poolManager.Return(cursor),
                processElement: (Cursor cursor, int index) => cursor.SetOwner(_context.Players[index]));
        }

        void IEntityManagerListener.OnEntitySpawned(IEntity entity)
        {
            if (entity is not RaftPlayer)
            {
                return;
            }
            
            SyncCursors();
        }

        void IEntityManagerListener.OnEntityDespawned(IEntity entity)
        { 
            if (entity is not RaftPlayer)
            {
                return;
            }

            SyncCursors();
        }
    }
}