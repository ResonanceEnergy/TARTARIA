using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Variant B — Waveform Trace. Per docs/15_MVP_BUILD_SPEC.md §9 Variant B.
    /// A golden sine wave scrolls horizontally; the player moves a cursor up/down
    /// (mouse Y or right-stick Y) to keep it on the wave. Accuracy is the fraction
    /// of time the cursor stayed within tolerance of the wave Y.
    ///
    /// 20-second duration. Builds its own UI canvas on first StartTuning.
    /// </summary>
    public class TuningVariantB_Waveform : MonoBehaviour, ITuningVariant
    {
        public event Action<float> OnTuningComplete;
        public event Action OnTuningFailed;
        public event Action<float> OnFrequencyChanged;

        public bool IsActive => _isPlaying;
        public float CurrentAccuracy { get; private set; }

        [Header("Difficulty")]
        // 2026-06-01 variant-polish: was 20f. Spec (mini-game-variant-polish) requires
        // success-with-input in <8s. Player can't shortcut the timer in this variant,
        // so the timer itself must complete inside the window.
        [SerializeField] private float duration = 7.5f;
        [SerializeField] private float tolerance = 0.10f;       // fraction of half-height that's "on-line"
        [SerializeField] private float scrollSpeed = 1.4f;      // wave phase per second
        [SerializeField] private float waveFrequency = 1.0f;    // wavelengths visible
        [SerializeField] private float waveAmplitude = 0.35f;   // fraction of half-height
        [SerializeField] private float cursorSpeed = 0.9f;      // how fast the cursor moves with input

        private static Canvas _sharedCanvas;
        private GameObject _panel;
        private RawImage _waveImage;
        private RectTransform _cursor;
        private Text _statusText;
        private Texture2D _waveTex;

        private bool _isPlaying;
        private float _timer;
        private float _phase;
        private float _cursorY01 = 0.5f;
        private float _onLineTime;

        const int WAVE_TEX_WIDTH = 256;
        const int WAVE_TEX_HEIGHT = 96;

        public void StartTuning(Vector3 _, Action onComplete)
        {
            EnsureUI();
            _isPlaying = true;
            _timer = duration;
            _onLineTime = 0f;
            _phase = 0f;
            _cursorY01 = 0.5f;
            CurrentAccuracy = 0f;

            if (_panel != null) _panel.SetActive(true);
            if (_statusText != null) _statusText.text = "Trace the wave";
            RedrawWave();
            Debug.Log("[TuningVariantB] Started (waveform trace).");
        }

        void EnsureUI()
        {
            if (_panel != null) return;

            if (_sharedCanvas == null)
            {
                var canvasGO = new GameObject("TuningCanvas_VariantB");
                _sharedCanvas = canvasGO.AddComponent<Canvas>();
                _sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _sharedCanvas.sortingOrder = 100;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasGO);
            }

            _panel = new GameObject("WaveformPanel");
            _panel.transform.SetParent(_sharedCanvas.transform, false);
            var prt = _panel.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.18f);
            prt.anchorMax = new Vector2(0.5f, 0.18f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(900f, 260f);
            prt.anchoredPosition = Vector2.zero;
            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.04f, 0.85f);

            // Status text top
            var statusGO = new GameObject("Status");
            statusGO.transform.SetParent(_panel.transform, false);
            var srt = statusGO.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 1f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.sizeDelta = new Vector2(800f, 40f);
            srt.anchoredPosition = new Vector2(0f, -10f);
            _statusText = statusGO.AddComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 28;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color = new Color(0.85f, 0.65f, 0.10f);
            _statusText.text = "Trace the wave";

            // Wave area (RawImage with a generated wave texture)
            var waveGO = new GameObject("WaveImage");
            waveGO.transform.SetParent(_panel.transform, false);
            var wrt = waveGO.AddComponent<RectTransform>();
            wrt.anchorMin = new Vector2(0.05f, 0.1f);
            wrt.anchorMax = new Vector2(0.95f, 0.8f);
            wrt.sizeDelta = Vector2.zero;
            wrt.anchoredPosition = Vector2.zero;
            _waveImage = waveGO.AddComponent<RawImage>();
            _waveTex = new Texture2D(WAVE_TEX_WIDTH, WAVE_TEX_HEIGHT, TextureFormat.RGBA32, false);
            _waveTex.filterMode = FilterMode.Bilinear;
            _waveImage.texture = _waveTex;

            // Cursor (a small white pip)
            var cursorGO = new GameObject("Cursor");
            cursorGO.transform.SetParent(waveGO.transform, false);
            var crt = cursorGO.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0f);
            crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(20f, 20f);
            crt.anchoredPosition = new Vector2(0f, 0.5f * waveAmplitude * 96f);
            _cursor = crt;
            var cimg = cursorGO.AddComponent<Image>();
            cimg.color = Color.white;
        }

        void Update()
        {
            if (!_isPlaying) return;

            float dt = Time.unscaledDeltaTime;
            _timer -= dt;
            _phase += dt * scrollSpeed * Mathf.PI * 2f;

            // Sample input — right stick Y (gamepad) or mouse Y delta (kb+mouse)
            float yDelta = 0f;
            var pad = Gamepad.current;
            if (pad != null)
            {
                yDelta += pad.rightStick.ReadValue().y * cursorSpeed * dt;
            }
            var mouse = Mouse.current;
            if (mouse != null)
            {
                yDelta += mouse.delta.ReadValue().y * 0.0015f;
            }
            // Also w/s as accessibility fallback
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.upArrowKey.isPressed)   yDelta += cursorSpeed * dt;
                if (kb.downArrowKey.isPressed) yDelta -= cursorSpeed * dt;
            }
            _cursorY01 = Mathf.Clamp01(_cursorY01 + yDelta);

            // Move cursor visually
            if (_cursor != null)
            {
                var size = ((RectTransform)_cursor.parent).rect.size;
                _cursor.anchoredPosition = new Vector2(0f, (_cursorY01 - 0.5f) * size.y);
            }

            // Sample wave Y at cursor X (middle of screen, x=0.5 of width)
            // Wave equation: y = 0.5 + sin(_phase) * amplitude  (in [0,1] coords)
            float waveY01 = 0.5f + Mathf.Sin(_phase) * waveAmplitude;
            float diff = Mathf.Abs(waveY01 - _cursorY01);
            bool onLine = diff < tolerance;
            if (onLine) _onLineTime += dt;

            RedrawWave();

            // Live accuracy
            CurrentAccuracy = _onLineTime / Mathf.Max(0.001f, (duration - _timer));
            OnFrequencyChanged?.Invoke(waveY01 * 1000f); // dummy "frequency" for HUD parity

            if (_timer <= 0f)
            {
                float finalAcc = Mathf.Clamp01(_onLineTime / duration);
                Finish(finalAcc);
            }
        }

        void RedrawWave()
        {
            if (_waveTex == null) return;
            // Clear
            var pixels = new Color32[WAVE_TEX_WIDTH * WAVE_TEX_HEIGHT];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(20, 16, 12, 220);

            // Wave line — golden
            var goldA = new Color32(217, 183, 80, 255);
            for (int x = 0; x < WAVE_TEX_WIDTH; x++)
            {
                float t = (float)x / WAVE_TEX_WIDTH;
                float phaseAt = _phase - t * waveFrequency * Mathf.PI * 2f;
                float y01 = 0.5f + Mathf.Sin(phaseAt) * waveAmplitude;
                int y = Mathf.Clamp(Mathf.RoundToInt(y01 * (WAVE_TEX_HEIGHT - 1)), 0, WAVE_TEX_HEIGHT - 1);
                pixels[y * WAVE_TEX_WIDTH + x] = goldA;
                if (y > 0) pixels[(y - 1) * WAVE_TEX_WIDTH + x] = goldA;
                if (y < WAVE_TEX_HEIGHT - 1) pixels[(y + 1) * WAVE_TEX_WIDTH + x] = goldA;
            }

            _waveTex.SetPixels32(pixels);
            _waveTex.Apply(false);
        }

        void Finish(float accuracy)
        {
            _isPlaying = false;
            CurrentAccuracy = accuracy;
            string tier = TuningMiniGame.GetAccuracyTier(accuracy);
            if (_panel != null) _panel.SetActive(false);

            if (accuracy >= 0.6f)
            {
                Debug.Log($"[TuningVariantB] SUCCESS! Accuracy {accuracy:P0} ({tier})");
                ServiceLocator.HUD?.ShowBanner("TUNED!", $"{tier} - Waveform locked", 3f);
                OnTuningComplete?.Invoke(accuracy);
            }
            else
            {
                Debug.Log($"[TuningVariantB] FAILED — accuracy {accuracy:P0}");
                ServiceLocator.HUD?.ShowBanner("FAILED", "Waveform lost", 3f);
                OnTuningFailed?.Invoke();
            }
        }
    }
}
