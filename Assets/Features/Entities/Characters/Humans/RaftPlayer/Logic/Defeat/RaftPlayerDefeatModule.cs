using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common.Extensions;
using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerDefeatLogic : CharacterDefeatLogic
    {
        private RaftPlayerDefeatSettings _settings;

        private RaftPlayer _player;
        private SyncVar<bool> _netInBarrel;

        public bool InBarrel => _netInBarrel.value;

        private Prop _barrelProp;

        private float _moveTimer;

        private Collider[] _reviveCollidersNonAlloc = new Collider[1];
        
        public RaftPlayerDefeatLogic(RaftPlayer player, SyncVar<bool> netIsDefeated, SyncVar<bool> netInBarrel) : base(player, netIsDefeated)
        {   
            _player = player;
            _netInBarrel = netInBarrel;

            _settings = (RaftPlayerDefeatSettings)_player.DefinitionData.EntityDefeatSettings;

            _netInBarrel.onChanged += HandleNetInBarrelChanged;
        }

        // Don't inherit Despawn or Tick from CharacterDefeatModule
        public override void Despawn()
        { }

        public override void Tick()
        { }

        public override void FixedTick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            if (!_netInBarrel.value)
            {
                return;
            }
            
            MoveFixedTick();
            StabalisationFixedTick();
            ReviveFixedTick();
        }

        private void MoveFixedTick()
        {
            _moveTimer += Time.fixedDeltaTime;
            _moveTimer = Mathf.Min(_moveTimer, _settings.MoveInterval);
            
            if (_moveTimer < _settings.MoveInterval)
            {
                return;
            }

            if (!_player.CharacterPhysicsModule.InWater)
            {
                return;
            }

            Vector3 forcedirection = _player.InputLogic.MoveDirection;

            if (forcedirection == Vector3.zero)
            {
                return;
            }
            
            forcedirection = Quaternion.AngleAxis(_settings.MovePitch, Vector3.Cross(forcedirection, Vector3.up)) * forcedirection;
            
            _player.EntityPhysicsLogic.NetworkRigidbody.AddForce(forcedirection * _settings.MoveLinearStrength, ForceMode.Impulse);

            Vector3 torqueDirection = Vector3.Cross(forcedirection, Vector3.up);

            _player.EntityPhysicsLogic.NetworkRigidbody.AddTorque(torqueDirection * _settings.MoveAngularStrength, ForceMode.Impulse);

            _moveTimer = 0f;
        }

        private void StabalisationFixedTick()
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.back, Vector3.up) * Quaternion.Inverse(_player.EntityPhysicsLogic.NetworkRigidbody.rotation);
            
            if (rotation.w < 0f)
            {
                rotation = new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
            }

            rotation.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle > 180f)
            {
                angle -= 360f;
            }

            Vector3 direction = axis.normalized * angle * Mathf.Deg2Rad;
            Vector3 torque = direction * _settings.StabalisationStrength - _player.EntityPhysicsLogic.NetworkRigidbody.angularVelocity * _settings.StabalisationDamping;

            _player.EntityPhysicsLogic.NetworkRigidbody.AddTorque(torque, ForceMode.Acceleration);
        }

        private void ReviveFixedTick()
        {
            if (Physics.OverlapSphereNonAlloc(_player.EntityPhysicsLogic.NetworkRigidbody.position, _settings.ReviveRadius, _reviveCollidersNonAlloc, _settings.ReviveMask) == 0)
            {
                return;
            }

            SetIsDefeated(false);
            _player.SetNetInBarrelRpc(_player.owner.Value, false);

            _player.EntityPhysicsLogic.NetworkRigidbody.AddForce(Vector3.up * _settings.ReviveStrength, ForceMode.Impulse);
        }

        protected override void HandleNetIsDefeatedChanged(bool defeated)
        {
            if (defeated)
            {
                _player.EntityPhysicsLogic.NetworkRigidbody.isKinematic = true;
                TweenExtensions.Rotation(_player.transform, endValue: Quaternion.LookRotation(Vector3.back, Vector3.up), duration: 0.33f, ease: Ease.OutQuad);
            }

            RaiseIsDefeatedChanged();
        }

        private void HandleNetInBarrelChanged(bool barrel)
        {
            if (barrel)
            {
                _barrelProp = _environmentManager.GetProp(PropId.Barrel, new SpawnParams() { Parent = _player.transform });
                _player.EntityModel.transform.localPosition = Vector3.up * 0.1f;
                _moveTimer = _settings.MoveInterval;

                if (_player.isOwner)
                {
                    _player.EntityPhysicsLogic.NetworkRigidbody.isKinematic = false;
                    _player.EntityPhysicsLogic.Rigidbody.constraints = RigidbodyConstraints.None; 
                }
            }
            else
            {
                _environmentManager.ReturnProp(_barrelProp);
                _barrelProp = null;
                _player.EntityModel.transform.localPosition = Vector3.zero;

                if (_player.isOwner)
                {
                    _player.EntityPhysicsLogic.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                }

                if (_networkManager.IsServer)
                {
                    _player.CharacterPhysicsModule.ResetTimeInWater();
                }
            }
        }

        // Respawn in a barrel in front of the raft
        public void Respawn()
        {
            _player.Inventory.DropAllItems(_player.transform.position);

            _player.SetPositionRpc(_player.owner.Value, new Vector3(Random.Range(-2f, 2f), 0.5f, 5f));

            _player.SetNetInBarrelRpc(_player.owner.Value, true);
        }
    }
}