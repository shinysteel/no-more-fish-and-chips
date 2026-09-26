using NoMoreFishAndChips.Effects;
using PurrNet;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class TeslaCoil : Structure<TeslaCoilDefinitionData>
    {
        [SerializeField] private LineRenderer _electrocuteLineRenderer;

        private SyncVar<float> _netChargeBlend = new SyncVar<float>(ownerAuth: true);

        private float _chargeTimer;

        private Collider[] _electrocuteCollidersNonAlloc = new Collider[10];

        private VFX _electricityVfx;

        private const string ChargeBlendName = "_ChargeBlend";

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleNetChargeBlendChanged(_netChargeBlend.value);

            _netChargeBlend.onChanged += HandleNetChargeBlendChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netChargeBlend.onChanged -= HandleNetChargeBlendChanged;
        }

        private void HandleNetChargeBlendChanged(float blend)
        {
            _entityModel.SetMaterialFloat(ChargeBlendName, blend);

            if (blend == 1f)
            {
                if (_electricityVfx == null)
                {
                    _electricityVfx = _effectManager.GetVfx(VfxId.TeslaElectricity, DefinitionData.ElectricityPosition, transform);
                }
            }
            else
            {
                if (_electricityVfx != null)
                {
                    _effectManager.ReturnVfx(_electricityVfx);
                    _electricityVfx = null;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isOwner)
            {
                ChargeUpdate();
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isOwner)
            {
                ElectrocuteFixedUpdate();
            }
        }

        private void ChargeUpdate()
        {
            _chargeTimer += Time.deltaTime;
            _chargeTimer = Mathf.Min(_chargeTimer, DefinitionData.ChargeDuration);

            float blend = _chargeTimer / DefinitionData.ChargeDuration;
            _netChargeBlend.value = blend;
        }

        private void ElectrocuteFixedUpdate()
        {
            if (_chargeTimer < DefinitionData.ChargeDuration)
            {
                return;
            }

            int overlaps = Physics.OverlapSphereNonAlloc(transform.position, DefinitionData.ElectrocuteRadius, _electrocuteCollidersNonAlloc, DefinitionData.ElectrocuteMask);

            for (int i = 0; i < overlaps; i++)
            {
                Entity entity = Utils.Physics.ColliderGetComponent<Entity>(_electrocuteCollidersNonAlloc[i]);

                if (entity.EntityDefinitionData.Alliance == EntityAlliance.Ally)
                {
                    continue;
                }

                if (entity.EntityDefeatLogic.IsDefeated)
                {
                    continue;
                }

                entity.HitRpc(entity.owner.Value, DefinitionData.ElectrocuteHit, (entity.transform.position - transform.position).normalized);

                _chargeTimer = 0f;

                break;
            }
        }
    }
}