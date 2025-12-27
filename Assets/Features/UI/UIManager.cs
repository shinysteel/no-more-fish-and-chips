using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using ShinyOwl.Common.Framework;

using Object = UnityEngine.Object;

namespace FishFlingers.UI
{
    public interface IUIManagerListener
    { }

    public enum UILayer
    {
        Backing    ,
        Screens    ,
        Foreground ,
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
        private UIManagerConfig _config;
        public UIManagerConfig Config => _config;

        private Canvas _gameCanvas;
        private EventSystem _eventSystem;

        private RectTransform[] _layers;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.UIManagerConfig;

            CreateCanvasAndEventSystem();
            CreateLayers();            

            base.Initialise(gameManagerConfig);
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }

        private void CreateCanvasAndEventSystem()
        {
            _gameCanvas = Object.Instantiate(_config.GameCanvasPrefab);
            _eventSystem = Object.Instantiate(_config.EventSystemPrefab);

            Object.DontDestroyOnLoad(_gameCanvas.gameObject);
            Object.DontDestroyOnLoad(_eventSystem.gameObject);
        }

        private void CreateLayers()
        {
            UILayer[] enums = (UILayer[])Enum.GetValues(typeof(UILayer));
            _layers = new RectTransform[enums.Length];
            for (int i = 0; i < enums.Length; i++)
            {
                _layers[i] = CreateLayer(enums[i].ToString(), (RectTransform)_gameCanvas.transform);
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

        public AsyncOperationBridge<UIElement> CreateUIElementAsync<T>(T prefab, UILayer layer, UILayerInsertMode mode = UILayerInsertMode.LastSibling) where T : UIElement
        {
            if (prefab == null)
            {
                Debugger.LogError(this, "Tried to create a ui element, but the prefab given was null");
                return null;
            }

            InstantiateParameters parameters = new()
            {
                parent = _layers[(int)layer], 
                worldSpace = false
            };

            AsyncOperation op = Object.InstantiateAsync(prefab, parameters);

            AsyncOperationBridge<UIElement> bridge = new AsyncOperationBridge<UIElement>(op, _ =>
            {
                AsyncInstantiateOperation instantiateOp = (AsyncInstantiateOperation)op;
                T uiElement = (T)instantiateOp.Result[0];

                if (mode == UILayerInsertMode.LastSibling)
                {
                    uiElement.transform.SetAsLastSibling();
                }
                else if (mode == UILayerInsertMode.FirstSibling)
                {
                    uiElement.transform.SetAsFirstSibling();
                }

                uiElement.Load();
                uiElement.gameObject.SetActive(false);
                return uiElement;
            });

            return bridge;
        }

        public void DestroyUIElement(UIElement uiElement, UILayer layer)
        {
            if (uiElement == null)
            {
                Debugger.LogError(this, "Tried to destroy a ui element, but the ui element given was null");
                return;
            }

            if (!uiElement.transform.IsChildOf(_layers[(int)layer]))
            {
                return;
            }

            uiElement.Unload();
            Object.Destroy(uiElement.gameObject);
        }
    }
}