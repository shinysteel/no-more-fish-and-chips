using NoMoreFishAndChips.Entities;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.Pools;

namespace NoMoreFishAndChips.UI
{
    public class ContextActionsUI : WorldUI
    {
        private PoolManager _poolManager;

        private List<ContextAction> _actions = new();

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Setup(ActionData[] datas)
        {
            Utils.Collections.ResizeList(_actions, datas.Length,
                createElement: () => _poolManager.GetTypedPoolable<ContextAction>(new SpawnParams() { Parent = transform }),
                removeElement: (ContextAction action) => _poolManager.ReturnTypedPoolable(action),
                processElement: (ContextAction action, int index) => action.Setup(datas[index]));
        }
    }
}