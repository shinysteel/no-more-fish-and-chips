using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace FishFlingers.Audio
{
    [CustomEditor(typeof(SoundCueData))]
    public class SoundCueDataEditor : Editor
    {
        private List<CancellationTokenSource> _ctsList = new();

        private void OnEnable()
        {
            EditorApplication.focusChanged += HandleFocusChanged;
        }

        private void OnDisable()
        {
            EditorApplication.focusChanged -= HandleFocusChanged;

            // When clicking off the asset, stop all sounds
            Stop();
        }

        private void HandleFocusChanged(bool focus)
        {
            // When unfocusing the editor, stop all sounds
            if (!focus)
            {
                Stop();
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewSettings()
        {
            SoundCueData cue = (SoundCueData)target;

            if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton"), "preButton"))
            {
                Play();
            }

            if (GUILayout.Button("Stop", "preButton"))
            {
                Stop();
            }
        }

        private void Play()
        {
            CancellationTokenSource cts = new();
            _ctsList.Add(cts);
            _ = PlayAsync(cts);
        }

        private async Task PlayAsync(CancellationTokenSource cts)
        {
            SoundCueData data = (SoundCueData)target;

            GameObject obj = new GameObject("Sound");
            obj.hideFlags = HideFlags.HideAndDontSave;
            AudioSource source = obj.AddComponent<AudioSource>();

            source.PlayOneShot(data.AudioClip);

            try
            {
                await Task.Delay((int)(data.AudioClip.length * 1000f), cts.Token);
            }
            catch (OperationCanceledException) { }

            // Task.Delay is not guaranteed to resume on Unity's main thread, and methods like DestroyImmediate are thread sensitive
            EditorApplication.delayCall += () =>
            {
                DestroyImmediate(obj);
                _ctsList.Remove(cts);
            };
        }

        private void Stop()
        {
            foreach (CancellationTokenSource cts in _ctsList)
            {
                cts.Cancel();
            }
        }
    }
}