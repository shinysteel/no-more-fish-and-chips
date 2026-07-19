using NoMoreFishAndChips.Networking;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.States
{
    public class StateSynchroniser : NetBehaviour, IStateManagerListener
    {
        private SyncList<int> _netStatePathEnumValues = new SyncList<int>(ownerAuth: true);
        
        protected override void OnSpawned()
        {
            _stateManager.AddListener(this);

            HandleNetStatePathChanged(default);

            _netStatePathEnumValues.onChanged += HandleNetStatePathChanged;
        }

        protected override void OnDespawned()
        {
            _netStatePathEnumValues.onChanged -= HandleNetStatePathChanged;

            _stateManager?.RemoveListener(this);
        }

        void IStateManagerListener.OnStatePathChanged(StatePath previous, StatePath current)
        {
            if (!isOwner)
            {
                return;
            }

            _netStatePathEnumValues.Clear();

            foreach (Enum enumValue in current)
            {
                _netStatePathEnumValues.Add(Convert.ToInt32(enumValue));
            }
        }

        private void HandleNetStatePathChanged(SyncListChange<int> change)
        {
            if (isOwner)
            {
                return;
            }

            _stateManager.ReadStatePathEnumValues(_netStatePathEnumValues.ToList());
        }
    }
}