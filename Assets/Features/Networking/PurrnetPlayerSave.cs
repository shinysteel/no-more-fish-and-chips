using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using Newtonsoft.Json;
using UnityEngine;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.Networking
{
    public class PurrnetPlayerSave
    {
        [JsonProperty] public int SaveId { get; private set; }
        [JsonProperty] public int ItemInstanceIdCounter { get; private set; } = 0;
        [JsonProperty] public RaftPlayerSave RaftPlayer { get; private set; } = new();

        public PurrnetPlayerSave()
        { }

        public PurrnetPlayerSave(int saveId, int itemInstanceIdCounter, RaftPlayerSave raftPlayer)
        {
            SaveId = saveId;
            ItemInstanceIdCounter = itemInstanceIdCounter;
            RaftPlayer = raftPlayer;
        }

        public async Task LoadToAsync(PurrnetPlayer purrnetPlayer)
        {
            // The raft player may not be created yet
            while (purrnetPlayer.GetNetRaftPlayer() == null)
            {
                await Task.Yield();
            }

            purrnetPlayer.SetNetSaveId(SaveId);
            purrnetPlayer.SetNetItemInstanceIdCounter(ItemInstanceIdCounter);

            await RaftPlayer.LoadToAsync(purrnetPlayer.GetNetRaftPlayer());
        }

        public void SaveFrom(PurrnetPlayer purrnetPlayer)
        {
            SaveId = purrnetPlayer.GetNetSaveId();
            ItemInstanceIdCounter = purrnetPlayer.GetNetItemInstanceIdCounter();
            RaftPlayer.SaveFrom(purrnetPlayer.GetNetRaftPlayer());
        }

        public void ApplyDefaults()
        {
            RaftPlayer.ApplyDefaults();
        }

        // We can't safely reference any managers in the default constructor, so this exists
        public void SetSaveId(int id)
        {
            SaveId = id;
        }
    }
}