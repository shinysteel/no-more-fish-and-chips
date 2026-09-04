using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Effects
{
    public class EnvironmentMarker : GameplayBehaviour
    {
        private SyncDictionaryWrapper<int, NetMarker> _netMarkers = new SyncDictionaryWrapper<int, NetMarker>(ownerAuth: true);
        private Dictionary<int, Marker> _markers = new();

        private int _idCounter;

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (KeyValuePair<int, NetMarker> kvp in _netMarkers)
            {
                SyncDictionaryChange<int, NetMarker> change = new SyncDictionaryChange<int, NetMarker>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value);
                HandleNetMarkersChanged(change);
            }

            _netMarkers.onChanged += HandleNetMarkersChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netMarkers.onChanged -= HandleNetMarkersChanged;

            foreach (Marker marker in _markers.Values)
            {
                _poolManager.ReturnTypedPoolable(marker);
            }
        }

        private void HandleNetMarkersChanged(SyncDictionaryChange<int, NetMarker> change)
        {
            if (change.operation == SyncDictionaryOperation.Added)
            {
                Marker marker = _poolManager.GetTypedPoolable<Marker>(new SpawnParams());
                marker.Initialise(_context, change.value);
                _markers.Add(change.key, marker);
            }
            else if (change.operation == SyncDictionaryOperation.Set)
            {
                _markers[change.key].SetNetMarker(change.value);
            }
            else if (change.operation == SyncDictionaryOperation.Removed)
            {
                _poolManager.ReturnTypedPoolable(_markers[change.key]);
                _markers.Remove(change.key);
            }
        }

        public NetMarkerHandle CreateNetMarker(Vector3 position, Vector3 scale, float blend)
        {
            int id = _idCounter++;
            NetMarker marker = new NetMarker(position, scale, blend);

            _netMarkers.Add(id, marker);

            NetMarkerHandle handle = new NetMarkerHandle(this, marker, id);
            return handle;
        }

        public void SetNetMarkerDirty(int id)
        {
            _netMarkers.SetDirty(id);
        }

        public void RemoveNetMarker(int id)
        {
            _netMarkers.Remove(id);
        }
    }
}