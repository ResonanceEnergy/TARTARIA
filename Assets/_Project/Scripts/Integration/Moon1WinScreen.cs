using UnityEngine;
using System.Collections;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1WinScreen — listens for GameEvents.OnMoonCompleted and shows a
    /// transition card: full-screen fade + big title + subtitle + RS reward.
    /// Auto-bootstraps so no scene wiring needed. Survives scene loads.
    ///
    /// Ship-gate checklist item 6 (win condition fires when 3rd building restores
    /// → credits or transition card). Item 7 also triggers Anastasia reveal beat
    /// via the same event (Anastasia controller listens independently).
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1WinScreen : MonoBehaviour
    {
        public static Moon1WinScreen Instance { get; private set; }

        Canvas _canvas;
        CanvasGroup _group;
        UnityEngine.UI.Image _bg;
        UnityEngine.UI.Text _title;
        UnityEngine.UI.Text _subtitle;
        UnityEngine.UI.Text _stats;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("Moon1WinScreen");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<Moon1WinScreen>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildCanvas();
            GameEvents.OnMoonCompleted += HandleMoonCompleted;
        }

        void OnDestroy()
        {
            GameEvents.OnMoonCompleted -= HandleMoonCompleted;
            if (Instance == this) Instance = null;
        }

        void BuildCanvas()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 32000; // on top of every other HUD
            gameObject.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
                UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var bgGo = new GameObject("BG");
            bgGo.transform.SetParent(transform, false);
            _bg = bgGo.AddComponent<UnityEngine.UI.Image>();
            _bg.color = new Color(0f, 0f, 0f, 0f);
            var bgRT = _bg.rectTransform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;

            _title = MakeText("Title", new Vector2(0.5f, 0.62f), 96, FontStyle.Bold,
                              new Color(1f, 0.95f, 0.65f));
            _subtitle = MakeText("Subtitle", new Vector2(0.5f, 0.50f), 36, FontStyle.Normal,
                                 new Color(0.92f, 0.92f, 0.92f));
            _stats = MakeText("Stats", new Vector2(0.5f, 0.36f), 24, FontStyle.Italic,
                              new Color(0.75f, 0.75f, 0.75f));
        }

        UnityEngine.UI.Text MakeText(string name, Vector2 anchor, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<UnityEngine.UI.Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = anchor; rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(1800, 220);
            rt.anchoredPosition = Vector2.zero;
            return t;
        }

        void HandleMoonCompleted(MoonCompletedEventArgs args)
        {
            if (args == null || args.moonIndex != 1) return; // Moon 1 only for now
            StopAllCoroutines();
            StartCoroutine(ShowSequence(args));
        }

        IEnumerator ShowSequence(MoonCompletedEventArgs args)
        {
            _title.text = "ECHOHAVEN AWAKENED";
            _subtitle.text = "Moon 1 Complete";
            int min = Mathf.FloorToInt(args.completionTime / 60f);
            int sec = Mathf.FloorToInt(args.completionTime % 60f);
            _stats.text = $"3 / 3 Hero Buildings Restored   +{args.rsReward} RS   {min:00}:{sec:00}";

            // Fade in over 1.2 s, hold 6 s, fade out 1.5 s
            float t = 0f;
            const float fadeIn = 1.2f, hold = 6f, fadeOut = 1.5f;

            while (t < fadeIn)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / fadeIn);
                _group.alpha = a;
                _bg.color = new Color(0f, 0f, 0f, a * 0.85f);
                yield return null;
            }
            _group.alpha = 1f;
            _bg.color = new Color(0f, 0f, 0f, 0.85f);

            yield return new WaitForSecondsRealtime(hold);

            t = 0f;
            while (t < fadeOut)
            {
                t += Time.unscaledDeltaTime;
                float a = 1f - Mathf.Clamp01(t / fadeOut);
                _group.alpha = a;
                _bg.color = new Color(0f, 0f, 0f, a * 0.85f);
                yield return null;
            }
            _group.alpha = 0f;
            _bg.color = new Color(0f, 0f, 0f, 0f);
        }
    }
}
