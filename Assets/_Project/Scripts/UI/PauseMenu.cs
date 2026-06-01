using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tartaria.Input;

namespace Tartaria.UI
{
    /// <summary>
    /// agent/ui/pause-menu-polish (2026-06-01): real Canvas-based pause menu.
    /// Subscribes to PlayerInputHandler.OnPauseToggled (static event) so both
    /// Esc and gamepad Start route through the same path. Auto-bootstraps as a
    /// DontDestroyOnLoad singleton via RuntimeInitializeOnLoadMethod.
    ///
    /// NOTE (blocker for Director): PauseAndGameOverMenu.cs also owns an Esc
    /// pause overlay (IMGUI). Both will fire when running together. Out of
    /// scope for this PR (path restriction = PauseMenu*.cs only). Resolve by
    /// removing the pause branch from PauseAndGameOverMenu in a follow-up.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        public bool IsOpen { get; private set; }

        Canvas _canvas;
        GameObject _root;
        float _prevTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("PauseMenu");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PauseMenu>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildCanvas();
            if (_root != null) _root.SetActive(false);
        }

        void OnEnable()
        {
            PlayerInputHandler.OnPauseToggled += Toggle;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            PlayerInputHandler.OnPauseToggled -= Toggle;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            // Force-close on scene change so a paused state from previous scene clears.
            if (IsOpen) Close();
        }

        void BuildCanvas()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 30000; // above HUD, below WinScreen (32000)
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            _root = new GameObject("PauseRoot", typeof(RectTransform));
            _root.transform.SetParent(transform, false);
            var rootRt = (RectTransform)_root.transform;
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // Dim background
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(_root.transform, false);
            var dimRt = (RectTransform)dim.transform;
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero;
            dimRt.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            // Title
            var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(_root.transform, false);
            var titleRt = (RectTransform)title.transform;
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0, -120);
            titleRt.sizeDelta = new Vector2(800, 100);
            var titleTxt = title.GetComponent<Text>();
            titleTxt.text = "PAUSED";
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.fontSize = 72;
            titleTxt.color = Color.white;
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            CreateButton("ResumeBtn",   "Resume",             new Vector2(0,   60), OnResumeClicked);
            CreateButton("SettingsBtn", "Settings",           new Vector2(0,  -20), OnSettingsClicked);
            CreateButton("QuitBtn",     "Quit to Main Menu",  new Vector2(0, -100), OnQuitClicked);
        }

        void CreateButton(string objName, string label, Vector2 anchoredPos, System.Action onClick)
        {
            var btn = new GameObject(objName, typeof(RectTransform), typeof(Image), typeof(Button));
            btn.transform.SetParent(_root.transform, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(400, 60);
            var img = btn.GetComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
            var button = btn.GetComponent<Button>();
            var cb = button.colors;
            cb.normalColor      = new Color(1f, 1f, 1f, 1f);
            cb.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f);
            cb.pressedColor     = new Color(0.6f, 0.8f, 1f, 1f);
            button.colors = cb;
            button.onClick.AddListener(() => onClick());

            var txtGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(btn.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            var txt = txtGo.GetComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 28;
            txt.color = Color.black;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            _prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            if (_root != null) _root.SetActive(true);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Time.timeScale = 1f;
            if (_root != null) _root.SetActive(false);
        }

        void OnResumeClicked()
        {
            Close();
        }

        void OnSettingsClicked()
        {
            Debug.Log("[PauseMenu] Settings not in MVP");
        }

        void OnQuitClicked()
        {
            Time.timeScale = 1f;
            IsOpen = false;
            if (_root != null) _root.SetActive(false);
            SceneManager.LoadScene("PersistentSystems", LoadSceneMode.Single);
        }
    }
}
