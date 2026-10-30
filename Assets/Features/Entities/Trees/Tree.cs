using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Tree : Entity
    {
        private StateManager _stateManager;

        private SyncVar<int> _netStageIndex = new SyncVar<int>(ownerAuth: true);
        private float _stageInterval;
        private float _timer;

        public TreeDefinitionData TreeDefinitionData => (TreeDefinitionData)_entityDefinitionData;
        public TreeModel TreeModel => (TreeModel)_entityModel;

        protected override void Awake()
        {
            base.Awake();

            _stateManager = GameManager.Instance.Get<StateManager>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner)
            {
                _stageInterval = TreeDefinitionData.GrowthDuration / (TreeModel.StageGameObjects.Length - 1);
            }

            _netStageIndex.onChanged += HandleNetStageIndexChanged;

            HandleNetStageIndexChanged(_netStageIndex.value);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netStageIndex.onChanged -= HandleNetStageIndexChanged;
        }

        private void HandleNetStageIndexChanged(int index)
        {
            for (int i = 0; i < TreeModel.StageGameObjects.Length; i++)
            {
                TreeModel.StageGameObjects[i].SetActive(i == index);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isOwner)
            {
                GrowthUpdate();
            }
        }

        private void GrowthUpdate()
        {
            if (!_stateManager.CurrentStatePath.Contains(EGameplayState.Stage))
            {
                return;
            }

            _timer += Time.deltaTime;
            _timer = Mathf.Min(_timer, TreeDefinitionData.GrowthDuration);

            int index = Mathf.FloorToInt(_timer / _stageInterval);

            if (_netStageIndex.value != index)
            {
                _netStageIndex.value = index;
            }
        }
    }
}