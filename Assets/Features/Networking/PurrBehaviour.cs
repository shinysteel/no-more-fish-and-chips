using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Networking
{
    // Mirrors PurrMonoBehaviour, but points to our NetworkManager
    public abstract class PurrBehaviour : MonoBehaviour
    {
        private NetworkManager _networkManager;

        private bool _isSubscribedServer;
        private bool _isSubscribedClient;

        protected virtual void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        private void OnEnable()
        {
            _networkManager.RegisterEvents(InternalSubscribe, InternalUnsubscribe);
        }

        private void OnDisable()
        {
            if (_isSubscribedServer)
            {
                InternalUnsubscribe(null, true);
            }

            if (_isSubscribedClient)
            {
                InternalUnsubscribe(null, false);
            }

            _networkManager.UnregisterEvents(InternalSubscribe, InternalUnsubscribe);
        }

        private void InternalSubscribe(PurrNet.NetworkManager manager, bool asServer)
        {
            if (asServer)
            {
                _isSubscribedServer = true;
            }
            else
            {
                _isSubscribedClient = true;
            }

            Subscribe(asServer);
        }

        private void InternalUnsubscribe(PurrNet.NetworkManager manager, bool asServer)
        {
            if (asServer)
            {
                _isSubscribedServer = false;
            }
            else
            {
                _isSubscribedClient = false;
            }

            Unsubscribe(asServer);
        }

        protected abstract void Subscribe(bool asServer);
        protected abstract void Unsubscribe(bool asServer);
    }
}