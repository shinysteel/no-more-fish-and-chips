using Ara;
using NoMoreFishAndChips.Effects;
using PurrNet;
using ShinyOwl.Common.Utils;
using UnityEngine;
using System.Threading.Tasks;
using NoMoreFishAndChips.Audio;

namespace NoMoreFishAndChips.Entities
{
    public class TeslaCoil : Structure<TeslaCoilDefinitionData>
    {
        [SerializeField] private LineRenderer _electrocuteLineRenderer;

        private EffectManager _effectManager;
        private AudioManager _audioManager;

        private SyncVar<float> _netChargeBlend = new SyncVar<float>(ownerAuth: true);
        private SyncVar<Entity> _netElectrocuteEntity = new SyncVar<Entity>(ownerAuth: true);

        private float _chargeTimer;

        private Material _electrocuteMaterial;
        private Collider[] _electrocuteCollidersNonAlloc = new Collider[10];

        private VFX _electricityVfx;

        private const string ChargeBlendName = "_ChargeBlend";

        protected override void Awake()
        {
            base.Awake();

            _effectManager = GameManager.Instance.Get<EffectManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _electrocuteMaterial = _electrocuteLineRenderer.material;
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleNetChargeBlendChanged(0f, _netChargeBlend.value);

            _netChargeBlend.onChangedWithOld += HandleNetChargeBlendChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netChargeBlend.onChangedWithOld -= HandleNetChargeBlendChanged;
        }

        private void HandleNetChargeBlendChanged(float previous, float current)
        {
            _entityModel.SetMaterialFloat(ChargeBlendName, current);

            if (current == 1f)
            {
                if (_electricityVfx == null)
                {
                    _electricityVfx = _effectManager.GetVfx(VfxId.TeslaElectricity, DefinitionData.ElectricityOffset, transform);
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

            float[] thresholds = DefinitionData.ChargeSettings.Thresholds;
            
            for (int i = thresholds.Length - 1; i >= 0; i--)
            {
                if (previous < thresholds[i] && current >= thresholds[i])
                {
                    _audioManager.PlaySound(SoundId.TeslaCharge, (thresholds[i] - thresholds[0]) * DefinitionData.ChargeSettings.PitchMultiplier);

                    break;
                }
            }

            if (previous < 1f && current == 1f)
            {
                _audioManager.PlaySound(SoundId.TeslaCharged, 0f);
            }
        }

        protected override void Update()
        {
            base.Update();

            ElectrocuteUpdate();

            if (isOwner)
            {
                ChargeUpdate();
            }
        }

        private void ChargeUpdate()
        {
            _chargeTimer += Time.deltaTime;
            _chargeTimer = Mathf.Min(_chargeTimer, DefinitionData.ChargeSettings.Duration);

            float blend = _netElectrocuteEntity.value == null ? _chargeTimer / DefinitionData.ChargeSettings.Duration : 1f;
            _netChargeBlend.value = blend;
        }

        private void ElectrocuteUpdate()
        {
            if (_netElectrocuteEntity.value != null)
            {
                _electrocuteLineRenderer.positionCount = 2;

                _electrocuteLineRenderer.SetPosition(0, transform.position + DefinitionData.ElectrocuteSettings.Offset);
                _electrocuteLineRenderer.SetPosition(1, _netElectrocuteEntity.value.transform.position);

                int index = Mathf.FloorToInt(Time.time * DefinitionData.ElectrocuteSettings.Frequency) % DefinitionData.ElectrocuteSettings.Textures.Length;
                _electrocuteMaterial.SetTexture("_BaseMap", DefinitionData.ElectrocuteSettings.Textures[index]);
            }
            else
            {
                _electrocuteLineRenderer.positionCount = 0;
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

        private void ElectrocuteFixedUpdate()
        {
            if (_netElectrocuteEntity.value != null)
            {
                return;
            }

            if (_chargeTimer < DefinitionData.ChargeSettings.Duration)
            {
                return;
            }

            int overlaps = Physics.OverlapSphereNonAlloc(transform.position + DefinitionData.ElectrocuteSettings.Offset, DefinitionData.ElectrocuteSettings.Radius, _electrocuteCollidersNonAlloc, DefinitionData.ElectrocuteSettings.Mask);

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

                _ = ElectrocuteAsync(entity);

                break;
            }
        }

        private async Task ElectrocuteAsync(Entity entity)
        {
            Vector3 position = transform.position + DefinitionData.ElectrocuteSettings.Offset;

            entity.HitRpc(entity.owner.Value, DefinitionData.ElectrocuteSettings.Hit, (entity.transform.position - position).normalized);

            _audioManager.PlaySound(SoundId.TeslaElectrocute, 0f);

            _netElectrocuteEntity.value = entity;

            await Task.Delay(500);

            _netElectrocuteEntity.value = null;

            _chargeTimer = 0f;
        }
    }
}