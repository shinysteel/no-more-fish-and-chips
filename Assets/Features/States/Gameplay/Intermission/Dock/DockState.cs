using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common.Framework;
using System;
using UnityEngine;
using NoMoreFishAndChips.Networking;
using System.Threading.Tasks;
using ShinyOwl.Common.Utils;
using ShinyOwl.Common;

using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.States
{
    public class DockState : IntermissionSubState
    {
        private NetworkManager _networkManager;
        private UIManager _uiManager;
        private CameraManager _cameraManager;
        private SceneManager _sceneManager;
        private EntityManager _entityManager;

        private DockStateConfig _config;

        private float _startTimer;

        private FixedCameraMode _fixedCameraMode;

        private VoyageResultsScreen _voyageResultsScreen;
        private HumanModel _voyageResultsHumanModel;

        public DockState(StateMachine<EIntermissionState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public override void InitialiseConfig(IntermissionStateConfig config)
        {
            _config = config.DockStateConfig;
        }

        public override void Enter()
        {
            base.Enter();

            // Understand that clients can join at any point
            _context.References.Ocean.SetCurrent(false, 0f);

            _startTimer = 0f;

            _ = EnterVoyageResultsAsync();
        }

        private async Task EnterVoyageResultsAsync()
        {
            try
            {
                _context.LocalPlayer.RaftPlayerActLogic.SetInCutscene(true);

                await _sceneManager.LoadSceneAsync(EScene.EnvironmentVoyageResults, LoadSceneMode.Additive, LoadSceneContext.Local);

                _voyageResultsHumanModel = (HumanModel)_entityManager.GetModel(EntityId.RaftPlayer, new SpawnParams() { Position = new Vector3(0f, 0.125f, 0f), Rotation = Quaternion.LookRotation(Vector3.back, Vector3.up), SpawnScene = SpawnScene.Scene(EScene.EnvironmentVoyageResults) });

                if (_context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem != null)
                {
                    _voyageResultsHumanModel.HoldItem(_context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem.ItemInstance.Data.ItemId);
                }

                Utils.GameObjects.TraverseHierarchy(_voyageResultsHumanModel.gameObject, (GameObject obj) => obj.layer = (int)ELayer.VoyageResults);

                _fixedCameraMode = new FixedCameraMode(_cameraManager.Config.VoyageResultsFixedCameraModeSettings);
                _cameraManager.AddMode(_fixedCameraMode);

                _cameraManager.CinemachineBrain.OutputCamera.cullingMask = _config.VoyageResultsMask;

                _voyageResultsScreen = await _uiManager.CreateScreenUIAsync(_uiManager.Config.VoyageResultsScreenPrefab, UILayer.Screens);
                _voyageResultsScreen.Setup(() => _ = ExitVoyageResultsAsync());

                _voyageResultsScreen.Show(null);
                _context.GameplayScreen.Hide(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async Task ExitVoyageResultsAsync()
        {
            try
            {
                _entityManager.ReturnModel(_voyageResultsHumanModel);
                _voyageResultsHumanModel = null;

                await _sceneManager.UnloadSceneAsync(EScene.EnvironmentVoyageResults, LoadSceneContext.Local);

                _cameraManager.RemoveMode(_fixedCameraMode);
                _fixedCameraMode = null;

                _cameraManager.CinemachineBrain.OutputCamera.cullingMask = ~0;

                _uiManager.DestroyScreenUI(_voyageResultsScreen, UILayer.Screens);
                _context.GameplayScreen.Show(null);

                _voyageResultsScreen = null;

                _context.LocalPlayer.RaftPlayerActLogic.SetInCutscene(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (_networkManager.IsServer)
            {
                StartTick();
            }
        }

        // Start counting down once all players are on the raft
        private void StartTick()
        {
            //bool canStart = true;

            //foreach (RaftPlayer player in _context.Players)
            //{
            //    if (!player.RaftPlayerPhysicsModule.OnRaft)
            //    {
            //        canStart = false;
            //        break;
            //    }
            //}

            //if (!canStart)
            //{
            //    _startTimer = 0f;
            //    return;
            //}

            //_startTimer += Time.deltaTime;

            //if (_startTimer >= _config.StartDuration)
            //{
            //    _parentStateMachine.ChangeState(EIntermissionState.Depart);
            //}
        }
    }
}