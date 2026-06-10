using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Effects
{
    public class EnvironmentMarker : GameplayBehaviour
    {
        private SyncDictionaryWrapper<int, Vector2Int[]> _netMarkedCells = new SyncDictionaryWrapper<int, Vector2Int[]>(ownerAuth: true);

        private Dictionary<int, DangerMarker> _markedCells = new();

        private int _idCounter;

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            foreach (KeyValuePair<int, Vector2Int[]> kvp in _netMarkedCells)
            {
                SyncDictionaryChange<int, Vector2Int[]> change = new SyncDictionaryChange<int, Vector2Int[]>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value);
                HandleNetMarkedCellsChanged(change);
            }

            _netMarkedCells.onChanged += HandleNetMarkedCellsChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netMarkedCells.onChanged -= HandleNetMarkedCellsChanged;

            foreach (DangerMarker marker in _markedCells.Values)
            {
                _poolManager.ReturnTypedPoolable(marker);
            }
        }

        public int AddNetMarkedCells(params Vector2Int[] cells)
        {
            int id = _idCounter++;
            _netMarkedCells.Add(id, cells);

            return id;
        }

        public void RemoveNetMarkedCells(int id)
        {
            _netMarkedCells.Remove(id);
        }

        private void HandleNetMarkedCellsChanged(SyncDictionaryChange<int, Vector2Int[]> change)
        {
            if (change.operation == SyncDictionaryOperation.Added)
            {
                DangerMarker marker = _poolManager.GetTypedPoolable<DangerMarker>(new SpawnParams());
                marker.Initialise(_context, change.value);
                _markedCells.Add(change.key, marker);
            }
            else if (change.operation == SyncDictionaryOperation.Removed)
            {
                _poolManager.ReturnTypedPoolable(_markedCells[change.key]);
                _markedCells.Remove(change.key);
            }
        }
    }
}