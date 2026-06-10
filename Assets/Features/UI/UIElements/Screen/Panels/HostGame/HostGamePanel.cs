using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using ShinyOwl.Common.Utils;
using System.IO;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.Pools;
using System.Linq;

namespace NoMoreFishAndChips.UI
{
    public class HostGamePanel : Panel
    {
        [SerializeField] private Transform _saveEntryContainer;

        private SaveManager _saveManager;
        private PoolManager _poolManager;

        private List<SaveEntry> _saveEntries = new();

        public override void Load(Canvas canvas)
        {
            base.Load(canvas);

            _saveManager = GameManager.Instance.Get<SaveManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public override void Show(Action onComplete)
        {
            base.Show(onComplete);

            Utils.Collections.ResizeList(_saveEntries, _saveManager.SaveFiles.Count + 1,
                createElement: () => _poolManager.GetTypedPoolable<SaveEntry>(new SpawnParams() { Parent = _saveEntryContainer.transform }),
                removeElement: (SaveEntry entry) => _poolManager.ReturnTypedPoolable(entry),
                processElement: (SaveEntry entry, int index) => entry.Setup(_saveManager.SaveFiles.ElementAtOrDefault(index)));
        }
    }
}