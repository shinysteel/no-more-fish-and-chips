using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using ShinyOwl.Common.Framework;

using Object = UnityEngine.Object;
using FishFlingers.Cameras;

namespace FishFlingers.UI
{
    public interface IUIManagerListener
    { }

    public enum UILayer
    {
        Backing    ,
        Screens    ,
        Foreground ,
        Panels     ,
        Popups     ,
        Notices    ,
        Overlay    ,
    }

    public enum UILayerInsertMode
    {
        LastSibling  ,
        FirstSibling , 
    }

    public class UIManager : GameSystem<IUIManagerListener>
    {
        private CameraManager _cameraManager;

        private UIManagerConfig _config;
        public UIManagerConfig Config => _config;

        private Canvas _screenCanvas;
        private Canvas _worldCanvas;
        private EventSystem _eventSystem;

        private RectTransform[] _layerContainers;

        public override void Initialise(GameManagerConfig config)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _config = config.UIManagerConfig;

            CreateCanvases();
            CreateLayers();            

            base.Initialise(config);
        }

        private void CreateCanvases()
        {
            _screenCanvas = Object.Instantiate(_config.ScreenCanvasPrefab);
            _worldCanvas = Object.Instantiate(_config.WorldCanvasPrefab);
            _eventSystem = Object.Instantiate(_config.EventSystemPrefab);

            _worldCanvas.worldCamera = _cameraManager.MainCamera;

            Object.DontDestroyOnLoad(_screenCanvas.gameObject);
            Object.DontDestroyOnLoad(_worldCanvas.gameObject);
            Object.DontDestroyOnLoad(_eventSystem.gameObject);
        }

        private void CreateLayers()
        {
            UILayer[] enums = (UILayer[])Enum.GetValues(typeof(UILayer));
            _layerContainers = new RectTransform[enums.Length];
            for (int i = 0; i < enums.Length; i++)
            {
                _layerContainers[i] = CreateLayer(enums[i].ToString(), (RectTransform)_screenCanvas.transform);
            }
        }

        private RectTransform CreateLayer(string name, RectTransform parent)
        {
            GameObject obj = new GameObject(name);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        public AsyncOperationBridge<T> CreateScreenUIAsync<T>(T prefab, UILayer layer, UILayerInsertMode mode = UILayerInsertMode.LastSibling) where T : ScreenUI
        {
            if (prefab == null)
            {
                Debugger.LogError(this, "Tried to create a ScreenUI, but the prefab given was null");
                return null;
            }

            InstantiateParameters parameters = new()
            {
                parent = _layerContainers[(int)layer], 
                worldSpace = false
            };

            AsyncOperation op = Object.InstantiateAsync(prefab, parameters);

            AsyncOperationBridge<T> bridge = new AsyncOperationBridge<T>(op, _ =>
            {
                AsyncInstantiateOperation instantiateOp = (AsyncInstantiateOperation)op;
                T ui = (T)instantiateOp.Result[0];

                // Recover the prefab's transform, since we are instantiating in local space
                ui.RectTransform.anchoredPosition = prefab.RectTransform.anchoredPosition;

                if (mode == UILayerInsertMode.LastSibling)
                {
                    ui.transform.SetAsLastSibling();
                }
                else if (mode == UILayerInsertMode.FirstSibling)
                {
                    ui.transform.SetAsFirstSibling();
                }

                ui.Load(_screenCanvas);
                ui.gameObject.SetActive(false);
                return ui;
            });

            return bridge;
        }

        public void DestroyScreenUI(ScreenUI ui, UILayer layer)
        {
            if (ui == null)
            {
                Debugger.LogError(this, "Tried to destroy a null ScreenUI");
                return;
            }

            if (!ui.transform.IsChildOf(_layerContainers[(int)layer]))
            {
                Debugger.LogError(this, "The ScreenUI to destroy was not on the specified layer");
                return;
            }

            ui.Unload();
            Object.Destroy(ui.gameObject);
        }

        public void PopLayer(UILayer layer)
        {
            RectTransform container = _layerContainers[(int)layer];

            if (container.childCount == 0)
            {
                return;
            }

            ScreenUI ui = container.GetChild(container.childCount - 1).GetComponent<ScreenUI>();

            ui.Hide(() => DestroyScreenUI(ui, layer));
        }

        public bool IsLayerEmpty(UILayer layer)
        {
            return _layerContainers[(int)layer].childCount == 0;
        }

        public T CreateWorldUI<T>(T prefab, Vector3 position) where T : WorldUI
        {
            if (prefab == null)
            {
                Debugger.LogError(this, $"Can't create a null WorldUI");
                return null;
            }

            T ui = Object.Instantiate(prefab, _worldCanvas.transform);
            ui.Load(_worldCanvas);
            return ui;
        }

        public void DestroyWorldUI(WorldUI ui)
        {
            if (ui == null)
            {
                Debugger.LogError(this, $"Can't destroy a null WorldUI");
                return;
            }

            if (ui.Canvas != _worldCanvas)
            {
                Debugger.LogError(this, $"Can't destroy WorldUI - it's not on the world canvas");
                return;
            }

            ui.Unload();
            Object.Destroy(ui.gameObject);
        }
    }
}