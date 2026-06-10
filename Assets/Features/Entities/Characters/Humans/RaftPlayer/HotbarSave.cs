using NoMoreFishAndChips.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class HotbarSave
    {
        [JsonProperty] public List<string> Slots { get; private set; } = new();
        [JsonProperty] public int SelectedIndex { get; private set; } = 0;

        public HotbarSave()
        { }

        public void LoadTo(Hotbar hotbar)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                hotbar.SetSlot(i, Slots[i]);
            }

            hotbar.SetSelectedIndex(SelectedIndex);
        }

        public void SaveFrom(Hotbar hotbar)
        {
            Slots.Clear();

            foreach (HotbarSlot slot in hotbar.Slots)
            {
                Slots.Add(slot.InstanceId);
            }

            SelectedIndex = hotbar.SelectedSlot.Index;
        }
    }
}