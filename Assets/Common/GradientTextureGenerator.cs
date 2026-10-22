#if UNITY_EDITOR
using ShinyOwl.Common;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GradientTextureGenerator : EditorWindow
{
    [SerializeField] private Gradient _gradient = new();
    [SerializeField] private string _name = "Gradient";
    [SerializeField] private string _destination = "Assets";

    private const int Resolution = 256;

    [MenuItem("Tools/GradientTextureGenerator")]
    private static void Open()
    {
        GetWindow<GradientTextureGenerator>("Gradient Texture Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Gradient Texture Generator", EditorStyles.boldLabel);

        _gradient = EditorGUILayout.GradientField("Gradient", _gradient);
        _name = EditorGUILayout.TextField("Name", _name);
        _destination = EditorGUILayout.TextField("Destination", _destination);

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        if (!Directory.Exists(_destination))
        {
            Log.Error($"Invalid destination");
            return;
        }

        Texture2D texture = new Texture2D(1, Resolution, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < Resolution; y++)
        {
            float time = y / (float)(Resolution - 1);
            texture.SetPixel(0, y, _gradient.Evaluate(time));
        }

        texture.Apply();

        string path = System.IO.Path.Combine(_destination, $"{_name}.png");

        File.WriteAllBytes(path, texture.EncodeToPNG());
        DestroyImmediate(texture);

        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
#endif