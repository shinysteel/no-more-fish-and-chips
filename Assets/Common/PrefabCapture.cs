using System.Collections.Generic;
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
        [SerializeField] private string _suffix = "Icon";
        [SerializeField] private Vector2Int _resolution = Vector2Int.one * 256;
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 0f, -10f);
        [SerializeField] private Vector3 _cameraRotation = new Vector3(25f, -25f, 0f);
        [SerializeField] private Vector3 _lightRotation = new Vector3(45f, -45f, 0f);
        [SerializeField] private float _cameraFOV = 20f;
        [SerializeField] private int _outlineThickness = 10;
        [SerializeField] private Color _outlineColor = new Color32(14, 57, 84, 255);

        private Texture2D _previewTexture;

        private const int MinResolution = 1;
        private const int MaxResolution = 1024;

        // Helps fine tune the right position
        private const float CameraOffsetMultiplier = 0.05f;

        private const float NearClipPlane = 0.01f;

        [MenuItem("Tools/PrefabCapture")]
        public static void ShowWindow()
        {
            GetWindow<PrefabCapture>("Prefab Capture");
        }

        private void OnGUI()
        {
            GUILayout.Label("Prefab Capture", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
            _destination = EditorGUILayout.TextField("Destination", _destination);
            _suffix = EditorGUILayout.TextField("Suffix", _suffix);
            _resolution = EditorGUILayout.Vector2IntField("Resolution", _resolution);
            _cameraOffset = EditorGUILayout.Vector3Field("Camera Offset", _cameraOffset);
            _cameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", _cameraRotation);
            _lightRotation = EditorGUILayout.Vector3Field("Light Rotation", _lightRotation);
            _cameraFOV = EditorGUILayout.FloatField("Camera FOV", _cameraFOV);
            _outlineThickness = EditorGUILayout.IntField("Outline Thickness", _outlineThickness);
            _outlineColor = EditorGUILayout.ColorField("Outline Color", _outlineColor);

            _resolution = new Vector2Int(Mathf.Clamp(_resolution.x, MinResolution, MaxResolution), Mathf.Clamp(_resolution.y, MinResolution, MaxResolution));

            if (EditorGUI.EndChangeCheck())
            {
                RefreshPreview();
            }

            if (_previewTexture != null)
            {
                DrawPreview();
            }

            if (GUILayout.Button("Save"))
            {
                Save();
            }
        }

        private void RefreshPreview()
        {
            if (_prefab == null)
            {
                // This is the case by default, so there's no need to spam errors here
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
            camera.fieldOfView = _cameraFOV;
            camera.nearClipPlane = NearClipPlane;

            camera.transform.Rotate(_cameraRotation);
            camera.transform.Translate(_cameraOffset * CameraOffsetMultiplier, Space.Self);

            // Capture the image
            RenderTexture renderTexture = new RenderTexture(_resolution.x, _resolution.y, 32);
            camera.targetTexture = renderTexture;
            camera.Render();

            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;

            RenderTexture previousTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // Extract into .png
            _previewTexture = new Texture2D(_resolution.x, _resolution.y, TextureFormat.RGBA32, false);
            _previewTexture.ReadPixels(new Rect(0, 0, _resolution.x, _resolution.y), 0, 0);
            _previewTexture.Apply();

            // Outline it
            if (_outlineThickness > 0)
            {
                OutlinePreview();
            }

            // Cleanup temporary objects
            RenderTexture.active = previousTexture;
            renderTexture.Release();

            DestroyImmediate(obj);
            DestroyImmediate(light.gameObject);
            DestroyImmediate(camera.gameObject);
            DestroyImmediate(renderTexture);
        }

        private void OutlinePreview()
        {
            float alphaThreshold = 0.01f;

            bool IsOpaque(Color color)
            {
                return color.a > alphaThreshold;
            }

            int width = _previewTexture.width;
            int height = _previewTexture.height;

            Color[] oldPixels = _previewTexture.GetPixels();
            Color[] newPixels = (Color[])oldPixels.Clone();

            // Precompute offsets
            List<Vector2Int> offsets = new();

            for (int offsetY = -_outlineThickness; offsetY <= _outlineThickness; offsetY++)
            {
                for (int offsetX = -_outlineThickness; offsetX <= _outlineThickness; offsetX++)
                {
                    offsets.Add(new Vector2Int(offsetX, offsetY));
                }
            }

            for (int y = 0; y < height; y++)
            {
                int rowIndex = y * width;

                for (int x = 0; x < width; x++)
                {
                    int index = rowIndex + x;

                    // Ignore pixels that are transparent
                    if (!IsOpaque(oldPixels[index]))
                    {
                        continue;
                    }

                    // Determine if the pixel is an edge
                    bool isEdge = false;

                    if (x > 0 && !IsOpaque(oldPixels[index - 1]))
                    {
                        isEdge = true;
                    }
                    else if (x < width - 1 && !IsOpaque(oldPixels[index + 1]))
                    {
                        isEdge = true;
                    }
                    else if (y > 0 && !IsOpaque(oldPixels[index - width]))
                    {
                        isEdge = true;
                    }
                    else if (y < height - 1 && !IsOpaque(oldPixels[index + width]))
                    {
                        isEdge = true;
                    }

                    if (!isEdge)
                    {
                        continue;
                    }

                    // Determine if the pixel is close enough to be outlined
                    foreach (Vector2Int offset in offsets)
                    {
                        int neighbourX = x + offset.x;
                        int neighbourY = y + offset.y;

                        if (neighbourX < 0 || neighbourY < 0 || neighbourX >= width || neighbourY >= height)
                        {
                            continue;
                        }

                        int neighbourIndex = neighbourY * width + neighbourX;

                        if (IsOpaque(oldPixels[neighbourIndex]))
                        {
                            continue;
                        }

                        newPixels[neighbourIndex] = _outlineColor;
                    }
                }
            }

            _previewTexture.SetPixels(newPixels);

            _previewTexture.Apply();
        }

        private void DrawPreview()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            float aspect = (float)_resolution.x / _resolution.y;
            float height = _resolution.x / aspect;
            EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(_resolution.x, height, GUILayout.ExpandWidth(false)), _previewTexture);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void Save()
        {
            if (_previewTexture == null)
            {
                Log.Error($"No preview texture exists to save");
                return;
            }

            byte[] bytes = _previewTexture.EncodeToPNG();
            string path = Path.Combine(_destination, $"{_prefab.name}{_suffix}.png");
            File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();

            // Modify the output to be a sprite
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }
}