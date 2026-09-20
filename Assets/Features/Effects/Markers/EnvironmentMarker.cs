using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common.Utils;
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
            Utils.Network.CacheSyncDictionaryChange(_markers, change, null,
                add: () =>
                {
                    Marker marker = _poolManager.GetTypedPoolable<Marker>(new SpawnParams());
                    marker.Initialise(_context, change.value);
                    return marker;
                },
                set: () =>
                {
                    _markers[change.key].SetNetMarker(change.value);
                    return _markers[change.key];
                },
                remove: () => _poolManager.ReturnTypedPoolable(_markers[change.key]),
                clear: (Marker marker) => _poolManager.ReturnTypedPoolable(marker));
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