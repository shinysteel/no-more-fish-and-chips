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

        private bool _inBarrel;
        public bool InBarrel => _inBarrel;

        private Prop _barrelProp;

        public RaftPlayerDefeatModule(RaftPlayer player, Func<bool> isDefeatedGetter, Action<bool> isDefeatedSetter, SyncVar<bool> netInBarrel) : base(player, isDefeatedGetter, isDefeatedSetter)
        {
            _player = player;
            _netInBarrel = netInBarrel;

            _netInBarrel.onChanged += HandleNetInBarrelChanged;
        }

        protected override void Despawn()
        {
        }

        // Don't inherit Tick logic from CharacterDefeatModule
        public override void Tick()
        { }

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
            }
            else
            {
                _poolManager.ReturnProp(_barrelProp);
                _barrelProp = null;
            }

            _player.RaftPlayerPhysicsModule.Rigidbody.isKinematic = false;
        }
    }
}