#if UNITY_EDITOR
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

            EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                foreach (object asset in scanner.GetAssets())
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
#endif