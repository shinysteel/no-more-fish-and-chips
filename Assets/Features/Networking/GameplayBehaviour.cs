using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.Networking
{
    public abstract class GameplayBehaviour : NetBehaviour
    {
        protected GameplayContext _context;

        protected bool _isInitialised;
        public bool IsInitialised => _isInitialised;

        public virtual void Initialise(GameplayContext context)
        {
            _context = context;

            _isInitialised = true;

            if (!isServer && TryGetComponent<NetworkTransform>(out _))
            {
                _ = RequestForceSyncAsync();
            }
        }

        // The raft needs to initialise before it creates it's tiles. This can cause desync for some NetworkTransforms
        // parented to those tiles. This is also only relevant to GameplayBehaviours that implement NetworkTransform
        private async Task RequestForceSyncAsync()
        {
            while (!_context.Raft.IsInitialised)
            {
                await Task.Yield();
            }

            RequestForceSyncRpc();
        }

        [ServerRpc(requireOwnership: false)]
        private void RequestForceSyncRpc()
        {
            _ = ForceSync();
        }

        // We can cause OnTransformParentChanged to be invoked by waiting some frames in between updating the parent
        private async Task ForceSync()
        {
            Transform parent = transform.parent;

            transform.SetParent(null);

            int frames = 10;
            for (int i = 0; i < frames; i++)
            {
                await Task.Yield();
            }

            transform.SetParent(parent);
        }
    }
}