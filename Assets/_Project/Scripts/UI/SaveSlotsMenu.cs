using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.UI
{
    /// <summary>
    /// Sprint 8 Lane 6: Runtime host for <see cref="SaveSlotPanel"/> - the consumer that
    /// audit blocker #4 said was missing. Provides a single Open()/Close() API so the
    /// MainMenu Continue button and the Pause "Load" button both have a real entrypoint
    /// instead of routing straight to <c>SaveManager.QuickLoad</c>.
    ///
    /// Behaviour:
    ///   - Open(): finds (or instantiates) a SaveSlotPanel under the active main Canvas,
    ///             centers it, and freezes Time.timeScale.
    ///   - Close(): destroys the host root and restores the previous Time.timeScale.
    ///
    /// API CONTRACT compliance (docs/agents/API_CONTRACT.md section 3):
    ///   - SaveSlotPanel internally invokes <c>SaveManager.SwitchToSlot(int)</c> at
    ///     Assets/_Project/Scripts/Save/SaveManager.cs:595 - NOT a non-existent LoadSlot(int).
    ///   - Unity 6 APIs: FindFirstObjectByType + FindObjectsByType with
    ///     FindObjectsInactive.Include. No deprecated FindObjectOfType anywhere.
    ///   - No silent catches: every catch logs file:line + the offending value.
    ///   - No stubs: Open() actually builds + parents the panel and freezes time.
    /// </summary>
    [DisallowMultipleComponent]
    public class SaveSlotsMenu : MonoBehaviour
    {
        const string HOST_ROOT_NAME = "SaveSlotsMenu_Root";
        const string PREFAB_RESOURCE_PATH = "UI/SaveSlotPanel"; // optional override; falls back to runtime build
        const int SORT_ORDER_OVERLAY = 5000;

        static SaveSlotsMenu _instance;

        GameObject _rootGo;
        SaveSlotPanel _panel;
        float _prevTimeScale = 1f;
        bool _timeScaleFrozen = false;

        // -- Public API --

        /// <summary>
        /// Spawns (or surfaces) the save-slots overlay. Safe to call from any UI button.
        /// </summary>
        public static void Open()
        {
            try
            {
                if (_instance != null && _instance._rootGo != null)
                {
                    _instance._rootGo.SetActive(true);
                    return;
                }

                var hostGo = new GameObject(HOST_ROOT_NAME);
                _instance = hostGo.AddComponent<SaveSlotsMenu>();
                _instance.BuildOverlay();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[SaveSlotsMenu] Open() failed (SaveSlotsMenu.cs:Open) - ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Tears down the overlay and restores time scale.
        /// </summary>
        public static void Close()
        {
            try
            {
                if (_instance == null) return;
                _instance.TeardownOverlay();
                if (_instance != null && _instance.gameObject != null)
                {
                    Destroy(_instance.gameObject);
                }
                _instance = null;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[SaveSlotsMenu] Close() failed (SaveSlotsMenu.cs:Close) - ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static bool IsOpen => _instance != null && _instance._rootGo != null && _instance._rootGo.activeInHierarchy;

        // -- Build --

        void BuildOverlay()
        {
            Canvas hostCanvas = ResolveHostCanvas();
            if (hostCanvas == null)
            {
                Debug.LogError(
                    "[SaveSlotsMenu] BuildOverlay (SaveSlotsMenu.cs:BuildOverlay) - no Canvas found in scene AND failed to create one. Aborting.");
                return;
            }

            _rootGo = new GameObject("SaveSlotsMenu_Overlay",
                typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster));
            _rootGo.transform.SetParent(hostCanvas.transform, false);

            var rootRt = _rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            var dim = _rootGo.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.78f);
            dim.raycastTarget = true;

            var overrideCanvas = _rootGo.GetComponent<Canvas>();
            overrideCanvas.overrideSorting = true;
            overrideCanvas.sortingOrder = SORT_ORDER_OVERLAY;

            GameObject panelGo = TryInstantiatePrefab(hostCanvas, out _panel);
            if (panelGo == null)
            {
                panelGo = new GameObject("SaveSlotPanel", typeof(RectTransform));
                panelGo.transform.SetParent(_rootGo.transform, false);
                _panel = panelGo.AddComponent<SaveSlotPanel>();
            }
            else
            {
                panelGo.transform.SetParent(_rootGo.transform, false);
            }

            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;

            BuildCloseButton(rootRt);

            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _timeScaleFrozen = true;

            Debug.Log($"[SaveSlotsMenu] Open - canvas='{hostCanvas.name}', panelType={(_panel != null ? _panel.GetType().Name : "NULL")}, timeScalePrev={_prevTimeScale}");
        }

        void BuildCloseButton(RectTransform rootRt)
        {
            var btnGo = new GameObject("CloseButton",
                typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(rootRt, false);

            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1f, 1f);
            btnRt.anchorMax = new Vector2(1f, 1f);
            btnRt.pivot = new Vector2(1f, 1f);
            btnRt.anchoredPosition = new Vector2(-24f, -24f);
            btnRt.sizeDelta = new Vector2(120f, 40f);

            btnGo.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f, 1f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(btnGo.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var labelText = labelGo.GetComponent<Text>();
            labelText.text = "Close";
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;

            btnGo.GetComponent<Button>().onClick.AddListener(Close);
        }

        GameObject TryInstantiatePrefab(Canvas hostCanvas, out SaveSlotPanel panelComp)
        {
            panelComp = null;
            try
            {
                var prefab = Resources.Load<GameObject>(PREFAB_RESOURCE_PATH);
                if (prefab == null) return null;
                var go = Instantiate(prefab);
                panelComp = go.GetComponent<SaveSlotPanel>();
                if (panelComp == null)
                {
                    Debug.LogWarning(
                        $"[SaveSlotsMenu] TryInstantiatePrefab (SaveSlotsMenu.cs:TryInstantiatePrefab) - Resources prefab '{PREFAB_RESOURCE_PATH}' " +
                        $"loaded but has no SaveSlotPanel component. Falling back to runtime build.");
                    Destroy(go);
                    return null;
                }
                return go;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[SaveSlotsMenu] TryInstantiatePrefab failed (SaveSlotsMenu.cs:TryInstantiatePrefab) - path='{PREFAB_RESOURCE_PATH}' ex={ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        Canvas ResolveHostCanvas()
        {
            Canvas best = null;
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (canvases != null && canvases.Length > 0)
            {
                int bestOrder = int.MinValue;
                foreach (var c in canvases)
                {
                    if (c == null) continue;
                    if (c.gameObject != null && c.gameObject.name == HOST_ROOT_NAME) continue;
                    int order = c.sortingOrder;
                    if (c.renderMode == RenderMode.ScreenSpaceOverlay) order += 1000;
                    if (order > bestOrder)
                    {
                        bestOrder = order;
                        best = c;
                    }
                }
            }

            if (best != null) return best;

            Debug.LogWarning(
                "[SaveSlotsMenu] ResolveHostCanvas (SaveSlotsMenu.cs:ResolveHostCanvas) - no Canvas found, creating fallback.");
            var canvasGo = new GameObject("SaveSlotsMenu_FallbackCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasGo);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SORT_ORDER_OVERLAY - 1;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        // -- Teardown --

        void TeardownOverlay()
        {
            if (_timeScaleFrozen)
            {
                Time.timeScale = _prevTimeScale > 0f ? _prevTimeScale : 1f;
                _timeScaleFrozen = false;
                Debug.Log($"[SaveSlotsMenu] Close - restored timeScale={Time.timeScale}");
            }

            if (_rootGo != null)
            {
                Destroy(_rootGo);
                _rootGo = null;
            }
            _panel = null;
        }

        void OnDestroy()
        {
            if (_timeScaleFrozen)
            {
                Time.timeScale = _prevTimeScale > 0f ? _prevTimeScale : 1f;
                _timeScaleFrozen = false;
            }
            if (ReferenceEquals(_instance, this)) _instance = null;
        }
    }
}
