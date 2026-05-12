using FishFlingers.Pools;
using UnityEngine;
using FishFlingers.Environments;
using PrimeTween;
using ShinyOwl.Common.Extensions;
using PurrNet;
using System;

namespace FishFlingers.Entities
{
    public class RaftPlayerDefeatModule : CharacterDefeatModule
    {
        private RaftPlayer _player;
        private SyncVar<bool> _netInBarrel;

        private RaftPlayerDefeatSettings _settings;

        private bool _inBarrel;
        public bool InBarrel => _inBarrel;

        private Prop _barrelProp;

        private float _moveTimer;

        private Collider[] _reviveCollidersNonAlloc = new Collider[1];
        
        public RaftPlayerDefeatModule(RaftPlayer player, Func<bool> isDefeatedGetter, Action<bool> isDefeatedSetter, SyncVar<bool> netInBarrel) : base(player, isDefeatedGetter, isDefeatedSetter)
        {
            _player = player;
            _netInBarrel = netInBarrel;

            _settings = (RaftPlayerDefeatSettings)_player.DefinitionData.EntityDefeatSettings;

            _netInBarrel.onChanged += HandleNetInBarrelChanged;
        }

        // Don't inherit Despawn or Tick from CharacterDefeatModule
        protected override void Despawn()
        { }

        public override void Tick()
        { }

        public override void FixedTick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            if (!_inBarrel)
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

            if (!_player.RaftPlayerPhysicsModule.InWater)
            {
                return;
            }

            Vector3 forcedirection = _player.InputLogic.MoveDirection;

            if (forcedirection == Vector3.zero)
            {
                return;
            }
            
            forcedirection = Quaternion.AngleAxis(_settings.MovePitch, Vector3.Cross(forcedirection, Vector3.up)) * forcedirection;
            
            _player.RaftPlayerPhysicsModule.Rigidbody.AddForce(forcedirection * _settings.MoveLinearStrength, ForceMode.Impulse);

            Vector3 torqueDirection = Vector3.Cross(forcedirection, Vector3.up);

            _player.RaftPlayerPhysicsModule.Rigidbody.AddTorque(torqueDirection * -_settings.MoveAngularStrength, ForceMode.Impulse);

            _moveTimer = 0f;
        }

        private void StabalisationFixedTick()
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.back, Vector3.up) * Quaternion.Inverse(_player.RaftPlayerPhysicsModule.Rigidbody.rotation);
            
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
            Vector3 torque = direction * _settings.StabalisationStrength - _player.RaftPlayerPhysicsModule.Rigidbody.angularVelocity * _settings.StabalisationDamping;

            _player.RaftPlayerPhysicsModule.Rigidbody.AddTorque(torque, ForceMode.Acceleration);
        }

        private void ReviveFixedTick()
        {
            if (Physics.OverlapSphereNonAlloc(_player.RaftPlayerPhysicsModule.Rigidbody.position, _settings.ReviveRadius, _reviveCollidersNonAlloc, _settings.ReviveMask) == 0)
            {
                return;
            }

            SetIsDefeated(false);
            SetNetInBarrel(false);

            _player.RaftPlayerPhysicsModule.Rigidbody.AddForce(Vector3.up * _settings.ReviveStrength, ForceMode.Impulse);
        }

        public override void HandleIsDefeatedChanged(bool defeated)
        {
            if (defeated)
            {
                _player.RaftPlayerPhysicsModule.Rigidbody.isKinematic = defeated;
                TweenExtensions.Rotation(_player.transform, endValue: Quaternion.LookRotation(Vector3.back, Vector3.up), duration: 0.33f, ease: Ease.OutQuad);
            }

            RaiseIsDefeatedChanged();
        }

        public void SetNetInBarrel(bool inBarrel)
        {
            _netInBarrel.value = inBarrel;
        }

        private void HandleNetInBarrelChanged(bool inBarrel)
        {
            if (_inBarrel == inBarrel)
            {
                return;
            }

            _inBarrel = inBarrel;

            if (_inBarrel)
            {
                _barrelProp = _poolManager.GetProp(PropId.Barrel, new SpawnParams() { Parent = _player.transform });
                _player.CharacterModel.transform.localPosition = Vector3.up * 0.1f;
                _moveTimer = _settings.MoveInterval;

                if (_player.isOwner)
                {
                    _player.RaftPlayerPhysicsModule.Rigidbody.isKinematic = false;
                    _player.RaftPlayerPhysicsModule.Rigidbody.constraints = RigidbodyConstraints.None; 
                }
            }
            else
            {
                _poolManager.ReturnProp(_barrelProp);
                _barrelProp = null;
                _player.CharacterModel.transform.localPosition = Vector3.zero;

                if (_player.isOwner)
                {
                    _player.RaftPlayerPhysicsModule.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                }
            }
        }
    }
}