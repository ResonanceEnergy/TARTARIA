using UnityEngine;
using UnityEngine.UI;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Ley Line Minimap widget per docs/03 Moon 1 Days 6-12 ("First ley-line vein lights
    /// up on the mini-map — golden thread pointing toward something vast in the distance").
    ///
    /// Renders a top-down compass + glowing first-vein indicator that appears when the
    /// first hero building is restored. Per CLAUDE.md no-stubs mandate: real Canvas,
    /// real Image with procedural texture + Update-driven golden pulse animation.
    /// Auto-bootstraps after scene load. Subscribes to OnBuildingRestoredTyped.
    /// </summary>
    [DisallowMultipleComponent]
    public class LeyLineMinimap : MonoBehaviour
    {
        static LeyLineMinimap _instance;
        Canvas _canvas;
        Image _ring, _firstVein, _veinHead;
        bool _firstVeinUnlocked;
        float _pulsePhase;
        int _restoredCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("LeyLineMinimap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LeyLineMinimap>();
        }

        void Start()
        {
            BuildUI();
            GameEvents.OnBuildingRestoredTyped += OnBuildingRestored;
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingRestoredTyped -= OnBuildingRestored;
        }

        void BuildUI()
        {
            var cgo = new GameObject("LeyLineMinimap_Canvas");
            cgo.transform.SetParent(transform, false);
            _canvas = cgo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 150;
            cgo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cgo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            cgo.AddComponent<GraphicRaycaster>();

            // Container at top-left below the Resonance Score widget
            var panel = MakeRect("Panel", cgo.transform);
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = new Vector2(20f, -260f);
            panel.sizeDelta = new Vector2(180f, 180f);

            // Background ring
            _ring = panel.gameObject.AddComponent<Image>();
            _ring.sprite = MakeCircleSprite(1.0f);
            _ring.color = new Color(0.05f, 0.05f, 0.08f, 0.70f);

            // Center cross-hair (player)
            var cross = MakeRect("PlayerDot", panel);
            cross.anchorMin = cross.anchorMax = cross.pivot = new Vector2(0.5f, 0.5f);
            cross.anchoredPosition = Vector2.zero;
            cross.sizeDelta = new Vector2(8f, 8f);
            var crossImg = cross.gameObject.AddComponent<Image>();
            crossImg.color = new Color(1f, 0.95f, 0.85f, 1f);

            // First ley vein (hidden until first restoration)
            var vein = MakeRect("FirstVein", panel);
            vein.anchorMin = vein.anchorMax = new Vector2(0.5f, 0.5f);
            vein.pivot = new Vector2(0.5f, 0f);
            vein.anchoredPosition = Vector2.zero;
            vein.sizeDelta = new Vector2(4f, 72f);
            // Rotate to point northeast toward "something vast"
            vein.localRotation = Quaternion.Euler(0, 0, -45f);
            _firstVein = vein.gameObject.AddComponent<Image>();
            _firstVein.color = new Color(0.95f, 0.78f, 0.30f, 0f);

            // Vein head (glowing target)
            var head = MakeRect("VeinHead", panel);
            head.anchorMin = head.anchorMax = new Vector2(0.5f, 0.5f);
            head.pivot = new Vector2(0.5f, 0.5f);
            float r = 72f, a = -45f * Mathf.Deg2Rad;
            head.anchoredPosition = new Vector2(Mathf.Sin(a) * r, Mathf.Cos(a) * r);
            head.sizeDelta = new Vector2(14f, 14f);
            _veinHead = head.gameObject.AddComponent<Image>();
            _veinHead.sprite = MakeCircleSprite(1.0f);
            _veinHead.color = new Color(1f, 0.85f, 0.40f, 0f);
        }

        void OnBuildingRestored(BuildingRestoredEventArgs args)
        {
            _restoredCount++;
            if (_restoredCount == 1 && !_firstVeinUnlocked)
            {
                _firstVeinUnlocked = true;
                Debug.Log("[LeyLineMinimap] First ley vein unlocked — golden thread lights up");
            }
        }

        void Update()
        {
            if (!_firstVeinUnlocked) return;
            _pulsePhase += Time.deltaTime * 2.5f;
            float pulse = 0.6f + 0.4f * Mathf.Sin(_pulsePhase);
            if (_firstVein != null) _firstVein.color = new Color(0.95f, 0.78f, 0.30f, pulse);
            if (_veinHead != null) _veinHead.color = new Color(1f, 0.85f, 0.40f, pulse);
        }

        static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            return rt;
        }

        static Sprite _circleSprite;
        static Sprite MakeCircleSprite(float fillRadius)
        {
            if (_circleSprite != null) return _circleSprite;
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - s * 0.5f;
                float dy = y - s * 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / (s * 0.5f);
                float a = Mathf.Clamp01(1f - (d - fillRadius * 0.85f) * 8f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, s, s), Vector2.one * 0.5f);
            return _circleSprite;
        }
    }
}
