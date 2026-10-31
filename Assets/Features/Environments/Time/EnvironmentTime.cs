using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class EnvironmentTime : GameplayBehaviour
    {
        [SerializeField] private EnvironmentTimeData[] _datas;

        private SyncVar<ETime> _netTime = new SyncVar<ETime>(initialValue: ETime.Day, ownerAuth: true);

        private Dictionary<ETime, EnvironmentTimeData> _timeDataMap = new();

        protected override void Awake()
        {
            base.Awake();

            foreach (EnvironmentTimeData data in _datas)
            {
                _timeDataMap.Add(data.Time, data);
            }
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            if (_netTime.value != ETime.Day)
            {
                _ = TransitionTimeAsync(ETime.Day, _netTime.value, 0f);
            }
        }

        public void ProgressTime(float duration)
        {
            SetTime((ETime)(((int)_netTime.value + 1) % Enum.GetValues(typeof(ETime)).Length), duration);
        }

        public void SetTime(ETime time, float duration)
        {
            TransitionTimeRpc(_netTime.value, time, duration);

            _netTime.value = time;
        }

        [ObserversRpc]
        private void TransitionTimeRpc(ETime fromTime, ETime toTime, float duration)
        {
            _ = TransitionTimeAsync(fromTime, toTime, duration);
        }

        private async Task TransitionTimeAsync(ETime fromTime, ETime toTime, float duration)
        {
            EnvironmentTimeData fromData = _timeDataMap[fromTime];
            EnvironmentTimeData toData = _timeDataMap[toTime];

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