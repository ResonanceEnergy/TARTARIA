using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.UI
{
    /// <summary>
    /// 3-Band Aether HUD per docs/15 § "Aether Band Focus: 3-Band introduction".
    ///
    /// Telluric (7.83 Hz, blue) / Harmonic (432 Hz, amber) / Celestial (528 Hz, green).
    /// Three vertical bars on the right side of the screen showing current Aether
    /// energy per band. Bars fill 0..1 normalized.
    ///
    /// Per CLAUDE.md no-stubs mandate: real Canvas + 3 colored Image bars + label Text +
    /// runtime polling for PlayerPrefs band values (decoupled from any Core asmdef).
    /// Auto-bootstraps after scene load.
    /// </summary>
    [DisallowMultipleComponent]
    public class AetherBandHUD : MonoBehaviour
    {
        static AetherBandHUD _instance;
        Canvas _canvas;
        Image _telluricFill, _harmonicFill, _celestialFill;
        Text _label;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("AetherBandHUD");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AetherBandHUD>();
        }

        void Start()
        {
            BuildUI();
        }

        void BuildUI()
        {
            var cgo = new GameObject("AetherBandHUD_Canvas");
            cgo.transform.SetParent(transform, false);
            _canvas = cgo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 140;
            cgo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cgo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            cgo.AddComponent<GraphicRaycaster>();

            var panel = MakeRect("Panel", cgo.transform);
            panel.anchorMin = new Vector2(1f, 0.5f);
            panel.anchorMax = new Vector2(1f, 0.5f);
            panel.pivot = new Vector2(1f, 0.5f);
            panel.anchoredPosition = new Vector2(-20f, 0f);
            panel.sizeDelta = new Vector2(120f, 280f);

            // Background
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.65f);

            // Three vertical bars
            _telluricFill = MakeBar(panel, new Vector2(-30f, 0f), new Color(0.30f, 0.55f, 0.95f, 1f));   // 7.83 Hz blue
            _harmonicFill = MakeBar(panel, new Vector2(  0f, 0f), new Color(0.95f, 0.65f, 0.20f, 1f));   // 432 Hz amber
            _celestialFill = MakeBar(panel, new Vector2( 30f, 0f), new Color(0.40f, 0.85f, 0.50f, 1f));  // 528 Hz green

            // Label
            var labRt = MakeRect("Label", panel);
            labRt.anchorMin = labRt.anchorMax = new Vector2(0.5f, 0f);
            labRt.pivot = new Vector2(0.5f, 0f);
            labRt.anchoredPosition = new Vector2(0f, 4f);
            labRt.sizeDelta = new Vector2(110f, 20f);
            _label = labRt.gameObject.AddComponent<Text>();
            _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _label.alignment = TextAnchor.MiddleCenter;
            _label.fontSize = 11;
            _label.color = new Color(0.92f, 0.88f, 0.75f, 1f);
            _label.text = "T  H  C";
        }

        Image MakeBar(RectTransform parent, Vector2 anchored, Color color)
        {
            var bg = MakeRect("Bar_BG", parent);
            bg.anchorMin = bg.anchorMax = bg.pivot = new Vector2(0.5f, 0.5f);
            bg.anchoredPosition = anchored;
            bg.sizeDelta = new Vector2(22f, 240f);
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(color.r * 0.20f, color.g * 0.20f, color.b * 0.20f, 0.65f);

            var fill = MakeRect("Bar_Fill", bg);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(1f, 0f); // fills from bottom up
            fill.pivot = new Vector2(0.5f, 0f);
            fill.anchoredPosition = Vector2.zero;
            fill.sizeDelta = new Vector2(0f, 0f); // height set in Update via anchoredPosition trick
            var img = fill.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        void Update()
        {
            float telluric = PlayerPrefs.GetFloat("TARTARIA_Aether_Telluric", 0.50f);
            float harmonic = PlayerPrefs.GetFloat("TARTARIA_Aether_Harmonic", 0.30f);
            float celestial = PlayerPrefs.GetFloat("TARTARIA_Aether_Celestial", 0.15f);
            SetFill(_telluricFill, telluric);
            SetFill(_harmonicFill, harmonic);
            SetFill(_celestialFill, celestial);
        }

        void SetFill(Image bar, float t)
        {
            if (bar == null) return;
            var rt = bar.rectTransform;
            // anchorMax.y goes from 0..1 based on t
            rt.anchorMax = new Vector2(1f, Mathf.Clamp01(t));
        }

        static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            return rt;
        }
    }
}
