using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    [CreateAssetMenu(fileName = "EnvironmentTimeData", menuName = "Data/Environments/EnvironmentTimeData")]
    public class EnvironmentTimeData : ScriptableObject
    {
        [SerializeField] private Vector3 _lightRotation;
        [SerializeField] private Color _lightColor;
        [SerializeField] private Color _horizonColor;
        [SerializeField, Range(0f, 1f)] private float _horizonEmission = 1f;
        [SerializeField] private float _ambientIntensity = 1f;
        [SerializeField] private Texture2D _skyGradient;

        public Vector3 LightRotation => _lightRotation;
        public Color LightColor => _lightColor;
        public Color HorizonColor => _horizonColor;
        public float HorizonEmission => _horizonEmission;
        public float AmbientIntensity => _ambientIntensity;
        public Texture2D SkyGradient => _skyGradient;

    }
}