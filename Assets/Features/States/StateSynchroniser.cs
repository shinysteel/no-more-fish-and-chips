using NoMoreFishAndChips.Networking;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.States
{
    public class StateSynchroniser : GameplayBehaviour, IStateManagerListener
    {
        private StateManager _stateManager;

        private SyncList<int> _netStatePathEnumValues = new SyncList<int>(ownerAuth: true);

        protected override void Awake()
        {
            base.Awake();

            _stateManager = GameManager.Instance.Get<StateManager>();
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            // It's only safe to start syncing once context is ready
            HandleNetStatePathChanged(default);

            _stateManager.AddListener(this);

            _netStatePathEnumValues.onChanged += HandleNetStatePathChanged;
        }

        protected override void OnDespawned()
        {
            _netStatePathEnumValues.onChanged -= HandleNetStatePathChanged;

            _stateManager.RemoveListener(this);
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