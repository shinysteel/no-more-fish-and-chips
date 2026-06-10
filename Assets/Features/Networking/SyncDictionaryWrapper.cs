using PurrNet;
using PurrNet.Packing;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.Networking
{
    [Serializable]
    public class SyncDictionaryWrapper<TKey, TValue> : SyncDictionary<TKey, TValue>
    {
        // When the wrapped SyncDictionary is safe to use
        private bool _isReady;

        public bool IsReady => _isReady;

        private List<KeyValuePair<TKey, TValue>> _pendingAdds = new();

        // Pseudo override of the onChanged callback
        public new event SyncDictionaryChanged<TKey, TValue> onChanged;

        public SyncDictionaryWrapper(bool ownerAuth = false, bool useForceSend = false) : base(ownerAuth, useForceSend)
        { }

        public override void OnSpawn()
        {
            base.OnSpawn();

            base.onChanged += HandleOnChanged;
            
            if (isServer == isOwner)
            {
                _isReady = true;
                FlushPendingAdds();
            }
        }

        public override void OnDespawned()
        {
            base.onChanged -= HandleOnChanged;

            base.OnDespawned();
        }

        public new void Add(KeyValuePair<TKey, TValue> item)
        {
            if (_isReady)
            {
                base.Add(item);
            }
            else
            {
                // Allows immediate calls to Add without having to worry about timing
                _pendingAdds.Add(item);
            }
        }

        public new void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        private void HandleOnChanged(SyncDictionaryChange<TKey, TValue> change)
        {
            if (_isReady)
            {
                onChanged?.Invoke(change);
            }
            // SyncDictionaries spawned by clients are unsafe to use until an initial clear operation is received
            else if (change.operation == SyncDictionaryOperation.Cleared)
            {
                _ = ReadyAsync();
            }         
        }

        private async Task ReadyAsync()
        {
            // Wait one frame for the SyncDictionary to do an internal sync
            await Task.Yield();

            _isReady = true;

            FlushPendingAdds();
        }

        private void FlushPendingAdds()
        {
            foreach (KeyValuePair<TKey, TValue> add in _pendingAdds)
            {
                base.Add(add.Key, add.Value);
            }

            _pendingAdds = null;
        }
    }
}