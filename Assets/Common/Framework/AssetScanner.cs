using ParrelSync;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShinyOwl.Common.Framework
{
    public abstract class AssetScanner : ScriptableObject
    {
        [SerializeField] protected DefaultAsset _folder;
        [SerializeField] protected bool _autoGenerate;

        public bool AutoGenerate => _autoGenerate;

        public abstract Array Assets { get; }

        public abstract void Scan();

        private void OnEnable()
        {
            if (ClonesManager.IsClone())
            {
                return;
            }

            if (_folder != null)
            {
                return;
            }

            _folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets");
            EditorUtility.SetDirty(this);
        }

        private void OnValidate()
        {
            if (ClonesManager.IsClone())
            {
                return;
            }

            if (!_autoGenerate)
            {
                return;
            }

            // delayCall is a single-shot queue that automatically clears subscribers once invoked
            EditorApplication.delayCall += HandleDelayCall;
        }

        private void HandleDelayCall()
        {
            Scan();
        }
    }

    public abstract class AssetScanner<T> : AssetScanner
    {
        private T[] _assets = new T[0];

        public override Array Assets => _assets;

        private class Lookup : IComparable<Lookup>
        {
            public string Guid { get; private set; }
            public T Obj { get; private set; }

            public Lookup(string guid, T obj)
            {
                Guid = guid;
                Obj = obj;
            }

            // Deterministic sorting
            public int CompareTo(Lookup other)
            {
                return Guid.CompareTo(other.Guid);
            }
        }

        public override void Scan()
        {
            Type type = typeof(T);

            bool isPrefabAsset = typeof(Component).IsAssignableFrom(type) || type.IsInterface;

            // Since T is unconstrained, we need to handle both cases
            string filter = isPrefabAsset
                ? "t:prefab"
                : $"t:{type.Name}";

            string folderPath = AssetDatabase.GetAssetPath(_folder);

            string[] guids = AssetDatabase.FindAssets(filter, new string[] { folderPath });

            List<Lookup> lookups = CreateLookups(guids, isPrefabAsset);
            lookups.Sort();

            if (!HaveChanges(_assets, lookups))
            {
                return;
            }

            _assets = new T[lookups.Count];
            for (int i = 0; i < _assets.Length; i++)
            {
                _assets[i] = lookups[i].Obj;
            }

            EditorUtility.SetDirty(this);
        }

        // Creates lookups for relevant assest via their guid
        private List<Lookup> CreateLookups(string[] guids, bool isPrefabAsset)
        {
            List<Lookup> lookups = new();

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                T obj = default;

                if (isPrefabAsset)
                {
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    asset?.TryGetComponent(out obj);
                }
                else
                {
                    obj = (T)(object)AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
                }

                if (obj == null)
                {
                    continue;
                }

                lookups.Add(new Lookup(guids[i], obj));
            }

            return lookups;
        }

        // It's worth checking for changes, since we want to avoid needlessly marking the asset dirty
        private bool HaveChanges(T[] assets, List<Lookup> lookups)
        {
            if (_assets.Length != lookups.Count)
            {
                return true;
            }

            for (int i = 0; i < _assets.Length; i++)
            {
                if (!Equals(_assets[i], lookups[i].Obj))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}