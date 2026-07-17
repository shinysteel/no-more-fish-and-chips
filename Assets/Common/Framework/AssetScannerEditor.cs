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

            if (GUILayout.Button("Manual Scan"))
            {
                scanner.Scan();
            }
        }
    }
}
#endif