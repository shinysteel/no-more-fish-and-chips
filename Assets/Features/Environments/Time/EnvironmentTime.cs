using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class EnvironmentTime : GameplayBehaviour
    {
        [SerializeField] private EnvironmentTimeData[] _datas;

        private ETime _time;

        private Dictionary<ETime, EnvironmentTimeData> _timeDataMap = new();

        protected override void Awake()
        {
            base.Awake();

            foreach (EnvironmentTimeData data in _datas)
            {
                _timeDataMap.Add(data.Time, data);
            }
        }

        public void ProgressTime()
        {
            SetTime((ETime)(((int)_time + 1) % Enum.GetValues(typeof(ETime)).Length));
        }

        public void SetTime(ETime time)
        {
            _time = time;

            SetData(_timeDataMap[_time]);
        }

        private void SetData(EnvironmentTimeData data)
        {
            _context.EnvironmentGameplayReferences.Light.transform.rotation = Quaternion.Euler(data.LightRotation);
            _context.EnvironmentGameplayReferences.Light.color = data.LightColor;
            _context.EnvironmentGameplayReferences.Sky.SetGradient(data.SkyGradient);
            _context.Ocean.SetHorizon(data.HorizonColor, data.HorizonEmission);
            RenderSettings.ambientIntensity = data.AmbientIntensity;
        }
    }
}