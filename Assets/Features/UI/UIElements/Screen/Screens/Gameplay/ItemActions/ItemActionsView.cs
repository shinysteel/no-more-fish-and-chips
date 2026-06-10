using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using NUnit.Framework;
using PrimeTween;
using ShinyOwl.Common.Utils;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using NoMoreFishAndChips.Pools;

namespace NoMoreFishAndChips.UI
{
    public class ItemActionsView : MonoBehaviour
    {
        private PoolManager _poolManager;

        private GameplayContext _context;

        private List<ItemActionView> _views = new();

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Setup(GameplayContext context)
        {
            _context = context;

            HandleHotbarSelectedChanged(_context.LocalPlayer.Hotbar.SelectedSlot);
            _context.LocalPlayer.Hotbar.OnSelectedChanged += HandleHotbarSelectedChanged;
        }

        private void OnDestroy()
        {
            if (_context.LocalPlayer?.Hotbar != null)
            {
                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleHotbarSelectedChanged;
            }
        }

        private void HandleHotbarSelectedChanged(HotbarSlot slot)
        {
            Utils.Collections.ResizeList(_views, slot.InventoryItem?.ItemInstance.Data.ActionDatas.Length ?? 0,
                createElement: () => _poolManager.GetTypedPoolable<ItemActionView>(new SpawnParams() { Parent = transform }),
                removeElement: (ItemActionView view) => _poolManager.ReturnTypedPoolable(view),
                processElement: (ItemActionView view, int index) => view.Setup(_context, slot.InventoryItem.ItemInstance.Data.ActionDatas[index]));
        }
    }
}