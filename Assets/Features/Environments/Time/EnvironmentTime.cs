using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class EnvironmentTime : GameplayBehaviour
    {
        [SerializeField] private EnvironmentTimeData _dawnData;
        [SerializeField] private EnvironmentTimeData _dayData;
        [SerializeField] private EnvironmentTimeData _duskData;
        [SerializeField] private EnvironmentTimeData _nightData;

        private EnvironmentTimeData _data;

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            SetData(_dayData);
        }

        private void SetData(EnvironmentTimeData data)
        {
            _data = data;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetData(_dawnData);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetData(_dayData);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetData(_duskData);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetData(_nightData);
            }

            if (_context != null)
            {
                _context.EnvironmentGameplayReferences.Light.transform.rotation = Quaternion.Euler(_data.LightRotation);
                _context.EnvironmentGameplayReferences.Light.color = _data.LightColor;
                _context.EnvironmentGameplayReferences.Sky.SetGradient(_data.SkyGradient);
                _context.Ocean.SetHorizon(_data.HorizonColor, _data.HorizonEmission);
                RenderSettings.ambientIntensity = _data.AmbientIntensity;
            }
        }
    }
}