using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class TeslaCoil : Structure<TeslaCoilDefinitionData>
    {
        private float _chargeTimer;

        private Collider[] _electrocuteCollidersNonAlloc = new Collider[10];

        private const string ChargeBlendName = "_ChargeBlend";

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

            _entityModel.SetMaterialFloat(ChargeBlendName, blend);
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

                entity.EntityHealthLogic.ChangeHealth(-DefinitionData.ElectrocuteDamage);

                _chargeTimer = 0f;

                break;
            }
        }
    }
}