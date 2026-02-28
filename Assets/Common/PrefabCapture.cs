using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShinyOwl.Common
{
    public class PrefabCapture : EditorWindow
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private string _destination = "Assets";
        [SerializeField] private int _resolution = 256;
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0.025f, 0.35f, -0.75f);
        [SerializeField] private Vector3 _cameraRotation = new Vector3(25f, -25f, 0f);
        [SerializeField] private Vector3 _lightRotation = new Vector3(45f, -45f, 0f);

        private const int MinResolution = 1;
        private const int MaxResolution = 512;

        private const float NearClipPlane = 0.01f;

        [MenuItem("Tools/PrefabCapture")]
        public static void ShowWindow()
        {
            GetWindow<PrefabCapture>("Prefab Capture");
        }

        private void OnGUI()
        {
            GUILayout.Label("Prefab Capture", EditorStyles.boldLabel);

            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
            _destination = EditorGUILayout.TextField("Destination", _destination);
            _resolution = EditorGUILayout.IntField("Resolution", _resolution);
            _cameraOffset = EditorGUILayout.Vector3Field("Camera Offset", _cameraOffset);
            _cameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", _cameraRotation);
            _lightRotation = EditorGUILayout.Vector3Field("Light Rotation", _lightRotation);

            _resolution = Mathf.Clamp(_resolution, MinResolution, MaxResolution);

            if (GUILayout.Button("Capture"))
            {
                Capture();
            }
        }

        private void Capture()
        {
            if (_prefab == null)
            {
                Log.Error($"Missing a prefab reference");
                return;
            }

            if (!Directory.Exists(_destination))
            {
                Log.Error($"Invalid destination");
                return;
            }

            GameObject obj = Instantiate(_prefab);

            // Setup the lighting
            Light light = new GameObject().AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.Rotate(_lightRotation);

            AmbientMode previousAmbientMode = RenderSettings.ambientMode;
            Color previousAmbientLight = RenderSettings.ambientLight;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.gray7;

            // Setup the camera
            Camera camera = new GameObject().AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.nearClipPlane = NearClipPlane;

            camera.transform.Rotate(_cameraRotation);
            camera.transform.Translate(_cameraOffset, Space.Self);

            // Capture the image
            RenderTexture texture = new RenderTexture(_resolution, _resolution, 32);
            camera.targetTexture = texture;
            camera.Render();

            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;

            RenderTexture previousTexture = RenderTexture.active;
            RenderTexture.active = texture;

            // Extract into .png
            Texture2D texture2D = new Texture2D(_resolution, _resolution, TextureFormat.RGBA32, false);
            texture2D.ReadPixels(new Rect(0, 0, _resolution, _resolution), 0, 0);
            texture2D.Apply();

            byte[] bytes = texture2D.EncodeToPNG();
            string path = Path.Combine(_destination, $"{_prefab.name}.png");
            File.WriteAllBytes(path, bytes);

            // Cleanup temporary objects
            RenderTexture.active = previousTexture;
            DestroyImmediate(obj);
            DestroyImmediate(light.gameObject);
            DestroyImmediate(camera.gameObject);

            AssetDatabase.Refresh();

            // Modify the output to be a sprite
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }
}