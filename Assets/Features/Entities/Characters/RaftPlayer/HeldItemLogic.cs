using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.UI;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using NetworkManager = FishFlingers.Networking.NetworkManager;

public class HeldItemLogic
{
    private UIManager _uiManager;
    private NetworkManager _networkManager;

    private RaftPlayer _player;

    private SyncVar<NetInventoryItem> _netHeldInventoryItem;

    private PointerEventData _pointerEventData;
    private List<RaycastResult> _raycastResults = new();

    private InventoryItem _heldInventoryItem;
    public InventoryItem HeldInventoryItem => _heldInventoryItem;

    private InventoryItemView _grabbedInventoryItemView;
    
    private const float GrabAlpha = 0.5f;

    public event Action<InventoryItem> OnChanged;

    public HeldItemLogic(RaftPlayer player, SyncVar<NetInventoryItem> netHeldInventoryItem)
    {
        _uiManager = GameManager.Instance.Get<UIManager>();
        _networkManager = GameManager.Instance.Get<NetworkManager>();

        _player = player;

        _netHeldInventoryItem = netHeldInventoryItem;
        _netHeldInventoryItem.onChanged += HandleNetHeldInventoryItemChanged;

        _pointerEventData = new PointerEventData(EventSystem.current);
    }

    public void Dispose()
    {
        _netHeldInventoryItem.onChanged -= HandleNetHeldInventoryItemChanged;
    }

    public void Tick()
    {
        if (_player.InputLogic.Rotate)
        {
            Rotate();
        }

        if (_player.InputLogic.Click)
        {
            Click();
        }
    }

    private void Rotate()
    {
        if (_heldInventoryItem == null)
        {
            return;
        }

        _networkManager.ChangeSyncVar(_netHeldInventoryItem, () => _netHeldInventoryItem.value.ChangeRotations(1));
    }

    private void Click()
    {
        GetTargetViews(out InventoryItemView targetItemView, out HotbarWidgetSlot targetHotbarSlot, out InventorySlotView targetInventorySlot);

        if (_heldInventoryItem == null)
        {
            // If the item and slot is linked, grab it
            if (targetItemView != null && targetInventorySlot != null && targetItemView.View.InventoryItem.ItemInstance.InstanceId == targetInventorySlot.InventoryItem?.ItemInstance.InstanceId)
            {
                Grab(targetItemView, targetInventorySlot);
            }
        }
        else if (targetHotbarSlot != null)
        {
            Assign(targetHotbarSlot);
        }
        else if (targetInventorySlot != null)
        {
            Place(targetInventorySlot);
        }
        else
        {
            Drop();
        }
    }

    private void GetTargetViews(out InventoryItemView targetItemView, out HotbarWidgetSlot targetHotbarSlot, out InventorySlotView targetInventorySlot)
    {
        targetItemView = null;
        targetHotbarSlot = null;
        targetInventorySlot = null;

        _pointerEventData.Reset();
        _pointerEventData.position = Input.mousePosition;

        _raycastResults.Clear();

        _uiManager.ScreenGraphicRaycaster.Raycast(_pointerEventData, _raycastResults);

        foreach (RaycastResult result in _raycastResults)
        {
            if (targetItemView == null)
            {
                result.gameObject.TryGetComponent(out targetItemView);
            }

            if (targetHotbarSlot == null)
            {
                result.gameObject.TryGetComponent(out targetHotbarSlot);
            }

            if (targetInventorySlot == null)
            {
                result.gameObject.TryGetComponent(out targetInventorySlot);
            }

            if (targetItemView != null && targetHotbarSlot != null && targetInventorySlot != null)
            {
                return;
            }
        }
    }

    private void Grab(InventoryItemView itemView, InventorySlotView slotView)
    {
        _grabbedInventoryItemView = itemView;
        _grabbedInventoryItemView.View.SetAlpha(GrabAlpha);

        _grabbedInventoryItemView.InventoryWidget.Inventory.OnInventoryItemChanged += HandleInventoryItemChanged;

        string instanceId = _grabbedInventoryItemView.View.InventoryItem.ItemInstance.InstanceId;

        // The item needs to be a clone so that rotating it doesn't affect the original
        NetInventoryItem item = _grabbedInventoryItemView.InventoryWidget.Inventory.NetInventoryItems[instanceId].DeepClone();
         
        Vector2Int origin = item.Cell - Utils.Math.RotateCell(item.Pivot, item.Rotations, true);
        Vector2Int offset = slotView.Cell - origin;
        Vector2Int pivot = Utils.Math.RotateCell(offset, item.Rotations, false);
        item.SetPivot(pivot);

        SetHeldItem(item);
    }

    private void Assign(HotbarWidgetSlot slot)
    {
        _player.Hotbar.SetSlot(slot.Index, _heldInventoryItem);

        Release();
    }

    private void Place(InventorySlotView slotView)
    {
        if (slotView.InventoryWidget.Inventory.TryPlaceItems(slotView.Cell, _netHeldInventoryItem.value.Pivot, new RotationParams() { Rotations = _netHeldInventoryItem.value.Rotations }, 
            _grabbedInventoryItemView.View.InventoryItem.ItemInstance.InstanceId, _grabbedInventoryItemView.View.InventoryItem.ItemInstance.Data.ItemId, _grabbedInventoryItemView.View.InventoryItem.ItemInstance.Count, true, out int overflow))
        {
            if (overflow > 0)
            {
                _grabbedInventoryItemView.InventoryWidget.Inventory.NetInventoryItems[_netHeldInventoryItem.value.InstanceId].SetCount(overflow);
                _grabbedInventoryItemView.InventoryWidget.Inventory.NetInventoryItems.SetDirty(_netHeldInventoryItem.value.InstanceId);
            }
            else
            {
                Release();
            }
        }
    }

    private void Drop()
    {
        _grabbedInventoryItemView.InventoryWidget.Inventory.RemoveItem(_heldInventoryItem.ItemInstance.InstanceId);
        Release();
    }

    private void Release()
    {
        _grabbedInventoryItemView.InventoryWidget.Inventory.OnInventoryItemChanged -= HandleInventoryItemChanged;

        _grabbedInventoryItemView.View.ResetAlpha();
        _grabbedInventoryItemView = null;

        SetHeldItem(null);
    }

    private void SetHeldItem(NetInventoryItem item)
    {
        if (_netHeldInventoryItem.value == item)
        {
            return;
        }

        _netHeldInventoryItem.value = item;
    }

    private void HandleNetHeldInventoryItemChanged(NetInventoryItem item)
    {
        _heldInventoryItem = item != null ? new InventoryItem(item) : null;

        OnChanged?.Invoke(_heldInventoryItem);
    }

    private void HandleInventoryItemChanged(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
    {
        if (instanceId != _netHeldInventoryItem.value.InstanceId)
        {
            return;
        }

        if (newInventoryItem == null)
        {
            return;
        }
        
        _networkManager.ChangeSyncVar(_netHeldInventoryItem, () => _netHeldInventoryItem.value.SetCount(newInventoryItem.ItemInstance.Count));
    }
}
