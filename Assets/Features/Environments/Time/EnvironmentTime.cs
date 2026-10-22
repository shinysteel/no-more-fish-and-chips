using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class EnvironmentTime : GameplayBehaviour
    {
        [SerializeField] private EnvironmentTimeData[] _datas;

        private ETime _time = ETime.Day;

        private Dictionary<ETime, EnvironmentTimeData> _timeDataMap = new();

        protected override void Awake()
        {
            base.Awake();

            foreach (EnvironmentTimeData data in _datas)
            {
                _timeDataMap.Add(data.Time, data);
            }
        }

        public void ProgressTime(float duration)
        {
            SetTime((ETime)(((int)_time + 1) % Enum.GetValues(typeof(ETime)).Length), duration);
        }

        public void SetTime(ETime time, float duration)
        {
            ETime previous = _time;

            _time = time;

            _ = TransitionData(_timeDataMap[previous], _timeDataMap[_time], duration);
        }

        private async Task TransitionData(EnvironmentTimeData fromData, EnvironmentTimeData toData, float duration)
        {
            float timer = 0f;

            Quaternion lightStartRotation = Quaternion.Euler(fromData.LightRotation);
            Quaternion lightEndRotation = Quaternion.Euler(toData.LightRotation);

            _context.EnvironmentGameplayReferences.Sky.SetGradientTextures(fromData.SkyGradient, toData.SkyGradient);

            void set(float t)
            {
                _context.EnvironmentGameplayReferences.Light.transform.rotation = Quaternion.Slerp(lightStartRotation, lightEndRotation, t);
                _context.EnvironmentGameplayReferences.Light.color = Color.Lerp(fromData.LightColor, toData.LightColor, t);
                _context.EnvironmentGameplayReferences.Sky.SetGradientBlend(t);
                _context.Ocean.SetHorizon(Color.Lerp(fromData.HorizonColor, toData.HorizonColor, t), Mathf.Lerp(fromData.HorizonEmission, toData.HorizonEmission, t));
                RenderSettings.ambientIntensity = Mathf.Lerp(fromData.AmbientIntensity, toData.AmbientIntensity, t);
            }

            while (timer < duration)
            {
                set(timer / duration);
                timer += Time.deltaTime;
                await Task.Yield();
            }

            set(1f);
        }
    }
}