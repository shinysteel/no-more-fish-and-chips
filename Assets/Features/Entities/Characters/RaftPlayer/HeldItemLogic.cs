using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.UI;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeldItemLogic
{
    private UIManager _uiManager;

    private SyncVar<NetInventoryItem> _netHeldInventoryItem;

    private InputLogic _inputLogic;

    private PointerEventData _pointerEventData;
    private List<RaycastResult> _raycastResults = new();

    private InventoryItem _heldInventoryItem;
    public InventoryItem HeldInventoryItem => _heldInventoryItem;

    public event Action<InventoryItem> OnChanged;

    public HeldItemLogic(SyncVar<NetInventoryItem> netHeldInventoryItem, InputLogic inputLogic)
    {
        _uiManager = GameManager.Instance.Get<UIManager>();

        _netHeldInventoryItem = netHeldInventoryItem;
        _netHeldInventoryItem.onChanged += HandleNetHeldInventoryItemChanged;

        _inputLogic = inputLogic;

        _pointerEventData = new PointerEventData(EventSystem.current);
    }

    public void Dispose()
    {
        _netHeldInventoryItem.onChanged -= HandleNetHeldInventoryItemChanged;
    }

    public void Tick()
    {
        if (!_inputLogic.Click)
        {
            return;
        }

        GetTargetViews(out InventoryItemView targetItemView, out InventorySlotView targetSlotView);

        if (_netHeldInventoryItem.value == null)
        {
            if (targetItemView != null)
            {
                Grab(targetItemView);
            }
        }
        else if (targetSlotView != null)
        {
            Place(targetSlotView);
        }
        else
        {
            Drop();
        }
    }

    private void GetTargetViews(out InventoryItemView itemView, out InventorySlotView slotView)
    {
        itemView = null;
        slotView = null;

        _pointerEventData.Reset();
        _pointerEventData.position = Input.mousePosition;

        _raycastResults.Clear();

        _uiManager.ScreenGraphicRaycaster.Raycast(_pointerEventData, _raycastResults);

        foreach (RaycastResult result in _raycastResults)
        {
            if (itemView == null)
            {
                result.gameObject.TryGetComponent(out itemView);
            }

            if (slotView == null)
            {
                result.gameObject.TryGetComponent(out slotView);
            }

            if (itemView != null && slotView != null)
            {
                return;
            }
        }
    }

    private void Grab(InventoryItemView itemView)
    {
        string instanceId = itemView.View.InventoryItem.ItemInstance.InstanceId;

        SetHeldItem(itemView.InventoryWidget.Inventory.GetNetInventoryItem(instanceId));
        itemView.InventoryWidget.Inventory.RemoveItem(instanceId);
    }

    private void Place(InventorySlotView slotView)
    {
        if (slotView.InventoryWidget.Inventory.TryPlaceItems(slotView.Cell, _netHeldInventoryItem.value.InstanceId, _netHeldInventoryItem.value.ItemId, _netHeldInventoryItem.value.Count, true, out int overflow))
        {
            if (overflow > 0)
            {
                _netHeldInventoryItem.value.SetCount(overflow);
            }
            else
            {
                SetHeldItem(null);
            }
        }
    }

    private void Drop()
    {
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
}
