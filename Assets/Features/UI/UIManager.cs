using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using ShinyOwl.Common.Framework;
using NoMoreFishAndChips.Cameras;
using UnityEngine.UI;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.Instantiating;

using Object = UnityEngine.Object;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.UI
{
    public interface IUIManagerListener
    {
        void OnLayerChanged(UILayer layer, int childCount) { }
    }

    public enum UILayer
    {
        Backings    ,
        Screens     ,
        Foregrounds ,
        Panels      ,
        Cursors     ,
        Popups      ,
        Notices     ,
        Overlays    ,
    }

    public enum UILayerInsertMode
    {
        LastSibling  ,
        FirstSibling , 
    }

    public class UIManager : GameSystem<IUIManagerListener>, IStateManagerListener
    {   
        private CameraManager _cameraManager;
        private StateManager _stateManager;

        private UIManagerConfig _config;
        public UIManagerConfig Config => _config;

        private Canvas _screenCanvas;
        private Canvas _worldCanvas;
        private EventSystem _eventSystem;

        private Layer[] _layers;

        private GraphicRaycaster _screenGraphicRaycaster;
        public GraphicRaycaster ScreenGraphicRaycaster => _screenGraphicRaycaster;

        public override void Initialise(GameManagerConfig config)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _stateManager = GameManager.Instance.Get<StateManager>();

            _stateManager.AddListener(this);

            _config = config.UIManagerConfig;

            CreateCanvases();
            CreateLayers();            

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _stateManager?.RemoveListener(this);
        }

        /// <summary>
        /// Initialises the persistent canvases
        /// </summary>
        private void CreateCanvases()
        {
            _screenCanvas = Object.Instantiate(_config.ScreenCanvasPrefab);
            _worldCanvas = Object.Instantiate(_config.WorldCanvasPrefab);
            _eventSystem = Object.Instantiate(_config.EventSystemPrefab);

            _worldCanvas.worldCamera = _cameraManager.MainCamera;

            Object.DontDestroyOnLoad(_screenCanvas.gameObject);
            Object.DontDestroyOnLoad(_worldCanvas.gameObject);
            Object.DontDestroyOnLoad(_eventSystem.gameObject);

            _screenGraphicRaycaster = _screenCanvas.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// Sets up all layers for the _screenCanvas
        /// </summary>
        private void CreateLayers()
        {
            UILayer[] enums = (UILayer[])Enum.GetValues(typeof(UILayer));
            _layers = new Layer[enums.Length];
            for (int i = 0; i < enums.Length; i++)
            {
                _layers[i] = CreateLayer(enums[i].ToString());
            }
        }

        private Layer CreateLayer(string name)
        {
            Layer layer = Object.Instantiate(_config.LayerPrefab, _screenCanvas.transform, false);
            layer.name = name;
            Utils.UI.StretchToParent(layer.RectTransform);
            return layer;
        }

        public override void Tick()
        {
            // Universal 'return' input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PopLayer(UILayer.Panels);
            }
        }

        /// <summary>
        /// Creates a screen ui async. These live in the _screenCanvas
        /// </summary>
        public AsyncOperationBridge<T> CreateScreenUIAsync<T>(T prefab, UILayer uiLayer, UILayerInsertMode mode = UILayerInsertMode.LastSibling) where T : ScreenUI
        {
            if (prefab == null)
            {
                Log.Error("Tried to create a ScreenUI, but the prefab given was null");
                return null;
            }

            Layer layer = _layers[(int)uiLayer];

            layer.ChangePendingCreateOps(1);

            InstantiateParameters parameters = new()
            {
                parent = layer.RectTransform, 
                worldSpace = false
            };

            AsyncOperation op = Object.InstantiateAsync(prefab, parameters);

            AsyncOperationBridge<T> bridge = new AsyncOperationBridge<T>(op, _ =>
            {
                AsyncInstantiateOperation instantiateOp = (AsyncInstantiateOperation)op;

                try
                {
                    // Ops exists outside of playmode, and can throw null refs unless we guard against the .Result being null
                    if (instantiateOp.Result == null)
                    {
                        return null;
                    }

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

                    NotifyLayerChanged(uiLayer);

                    return ui;
                }
                finally
                {
                    layer.ChangePendingCreateOps(-1);
                }
            });

            return bridge;
        }

        /// <summary>
        /// Destroys a screen ui at the end of the frame
        /// </summary>
        public void DestroyScreenUI(ScreenUI ui, UILayer uiLayer)
        {
            if (ui == null)
            {
                Log.Error("Tried to destroy a null ScreenUI");
                return;
            }

            if (!ui.transform.IsChildOf(_layers[(int)uiLayer].RectTransform))
            {
                Log.Error("The ScreenUI to destroy was not on the specified layer");
                return;
            }

            ui.Unload();
            Object.Destroy(ui.gameObject);

            _ = notifyAsync();

            async Task notifyAsync()
            {
                // Destroy is deffered to end of frame
                await Task.Yield();

                NotifyLayerChanged(uiLayer);
            }
        }

        /// <summary>
        /// Destroys the topmost ui in a layer
        /// </summary>
        public void PopLayer(UILayer uiLayer)
        {
            RectTransform rect = _layers[(int)uiLayer].RectTransform;

            if (rect.childCount == 0)
            {
                return;
            }

            ScreenUI ui = rect.GetChild(rect.childCount - 1).GetComponent<ScreenUI>();

            ui.Hide(() => DestroyScreenUI(ui, uiLayer));
        }

        /// <summary>
        /// Destroys all ui in a layer
        /// </summary>
        public void ClearLayer(UILayer uiLayer)
        {
            foreach (Transform child in _layers[(int)uiLayer].RectTransform)
            {
                ScreenUI ui = child.GetComponent<ScreenUI>();
                ui.Hide(() => DestroyScreenUI(ui, uiLayer));
            }
        }

        /// <summary>
        /// InUse translates to a layer having any pending create ops, or simply ui being active in it
        /// </summary>
        public bool IsLayerInUse(UILayer uiLayer)
        {
            return _layers[(int)uiLayer].InUse();
        }

        /// <summary>
        /// Creates a world ui. These live in the _worldCanvas, and layers aren't relevant
        /// </summary>
        public T CreateWorldUI<T>(T prefab, Vector3 position) where T : WorldUI
        {
            if (prefab == null)
            {
                Log.Error($"Can't create a null WorldUI");
                return null;
            }

            T ui = Object.Instantiate(prefab, _worldCanvas.transform);
            ui.Load(_worldCanvas);
            return ui;
        }
        
        /// <summary>
        /// Destroys a world ui at the end of the frame
        /// </summary>
        public void DestroyWorldUI(WorldUI ui)
        {
            if (ui == null)
            {
                Log.Error($"Can't destroy a null WorldUI");
                return;
            }

            if (ui.Canvas != _worldCanvas)
            {
                Log.Error($"Can't destroy WorldUI - it's not on the world canvas");
                return;
            }

            ui.Unload();
            Object.Destroy(ui.gameObject);
        }

        private void NotifyLayerChanged(UILayer uiLayer) => Listeners.Dispatch(listener => listener.OnLayerChanged(uiLayer, _layers[(int)uiLayer].RectTransform.childCount));

        void IStateManagerListener.OnStateChanged(EMainState previous, EMainState current)
        {
            ClearLayer(UILayer.Panels);
        }
    }
}