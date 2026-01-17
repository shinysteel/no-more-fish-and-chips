using UnityEditor;
using UnityEngine;

namespace ShinyOwl.Common.Framework
{
    [CustomEditor(typeof(AssetScanner), true)]
    public class AssetScannerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AssetScanner scanner = (AssetScanner)target;

            if (!scanner.AutoGenerate && GUILayout.Button("Generate"))
            {
                scanner.Scan();
            }            

            EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                foreach (object asset in scanner.Assets)
                {
                    if (asset is Object obj)
                    {
                        EditorGUILayout.ObjectField(obj.name, obj, typeof(Object), false);
                    }
                }
            }
        }
    }
}