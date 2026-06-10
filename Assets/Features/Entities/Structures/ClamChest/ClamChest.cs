using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using Newtonsoft.Json;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using UnityEngine;
using NoMoreFishAndChips.Audio;

namespace NoMoreFishAndChips.Entities
{
    public class ClamChest : Structure<ClamChestDefinitionData>, IInteractable, IHasInventory, INetworkManagerListener
    {
        [SerializeField] private Transform _hingeTransform;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private BoolGrid _inventoryLayout;

        private PanelInstance<ClamChestPanel> _clamChestPanelInstance;

        // The count of players who have this chest open
        private int _openCount;

        private bool _isOpen;
        private Tween _openTween;
        private Tween _closeTween;

        public Inventory Inventory => _inventory;

        private const float OpenDuration = 0.4f;

        IInteractableSettings IInteractable.Settings => DefinitionData.IInteractableSettings;

        protected override void Awake()
        {
            base.Awake();

            _inventory.SetLayout(_inventoryLayout);
        }

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            _clamChestPanelInstance = new PanelInstance<ClamChestPanel>(_uiManager.Config.ClamChestPanelPrefab);

            foreach (RaftPlayer player in context.Players)
            {
                ((INetworkManagerListener)this).OnNetBehaviourSpawned(player);
            }

            _networkManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _networkManager?.RemoveListener(this);
        }

        public override string GetJsonData()
        {
            return JsonConvert.SerializeObject(new InventorySave(_inventory));
        }

        public override void LoadJsonData(string json)
        {
            _ = JsonConvert.DeserializeObject<InventorySave>(json).LoadToAsync(_inventory);
        }

        bool IInteractable.CanPrompt()
        {
            return true;
        }

        WorldUI IInteractable.CreatePromptUI()
        {
            InteractPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(DefinitionData.IInteractableSettings.Hotkey);
            return ui;
        }

        bool IInteractable.CanInteract()
        {
            return true;
        }

        void IInteractable.Interact()
        {
            _context.LocalPlayer.SetNetOpenObjectNetworkId(this);

            _clamChestPanelInstance.Toggle((ClamChestPanel panel) => panel.Setup(_context, _inventory));
        }

        void INetworkManagerListener.OnNetBehaviourSpawned(NetBehaviour behaviour)
        {
            if (behaviour is not RaftPlayer player)
            {
                return;
            }

            HandleOpenNetBehaviourChanged(null, player.OpenNetBehaviourLogic.Behaviour);

            player.OpenNetBehaviourLogic.OnBehaviourChanged += HandleOpenNetBehaviourChanged;
        }

        void INetworkManagerListener.OnNetBehaviourDespawned(NetBehaviour behaviour)
        {
            if (behaviour is not RaftPlayer player)
            {
                return;
            }

            player.OpenNetBehaviourLogic.OnBehaviourChanged -= HandleOpenNetBehaviourChanged;
        }

        private void HandleOpenNetBehaviourChanged(NetBehaviour oldBehaviour, NetBehaviour newBehaviour)
        {
            if (oldBehaviour == this)
            {
                _openCount--;
            }

            if (newBehaviour == this)
            {
                _openCount++;
            }

            if (_openCount > 0 == _isOpen)
            {
                return;
            }

            _isOpen = _openCount > 0;

            if (_isOpen)
            {
                _closeTween.Stop();

                _openTween = Tween.LocalRotation(_hingeTransform, endValue: Quaternion.AngleAxis(-90f, Vector3.right), duration: OpenDuration);

                _audioManager.PlaySound(SoundId.ClamChestOpen);
            }
            else
            {
                _openTween.Stop();

                _closeTween = Tween.LocalRotation(_hingeTransform, endValue: Quaternion.identity, duration: OpenDuration, ease: Ease.InQuad);

                _audioManager.PlaySound(SoundId.ClamChestClose);
            }
        }
    }
}