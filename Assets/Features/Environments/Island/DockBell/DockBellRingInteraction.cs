using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Localisation;
using NoMoreFishAndChips.Networking;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class DockBellRingInteraction : Interaction
    {
        private NetworkManager _networkManager;

        private DockBell _bell;

        public override ActionHotkey Hotkey => ActionHotkey.FKey;
        public override LocalisationTerm Term => LocalisationTerm.ActionsRing;

        public DockBellRingInteraction(DockBell bell) : base()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _bell = bell;
        }

        public override bool CanPrompt()
        {
            return !_networkManager.LocalPurrnetPlayer.RaftPlayer.ReadyLogic.IsReady;
        }

        public override void Interact()
        {
            if (_networkManager.LocalPurrnetPlayer.RaftPlayer.ReadyLogic.IsReady)
            {
                return;
            }

            _networkManager.LocalPurrnetPlayer.RaftPlayer.ReadyLogic.SetNetIsReady(true);

            AudioManager.PlaySoundRpc(SoundId.DockBellRing, _bell.transform.position, 0f);
        }
    }
}