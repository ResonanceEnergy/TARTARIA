using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Variant C — Harmonic Pattern. Per docs/15_MVP_BUILD_SPEC.md §9 Variant C.
    /// Five circles appear in sequence at fixed intervals; player must press E (or
    /// gamepad A) in time with each. ±100ms = perfect, ±200ms = good, ±300ms = ok,
    /// otherwise miss. Accuracy is averaged across the 5 beats.
    ///
    /// 10-second duration. Builds its own UI canvas on first StartTuning.
    /// </summary>
    public class TuningVariantC_Pattern : MonoBehaviour, ITuningVariant
    {
        public event Action<float> OnTuningComplete;
        public event Action OnTuningFailed;
#pragma warning disable 67 // event declared to satisfy ITuningVariant; Variant C uses no continuous frequency
        public event Action<float> OnFrequencyChanged;
#pragma warning restore 67

        public bool IsActive => _isPlaying;
        public float CurrentAccuracy { get; private set; }

        [Header("Difficulty")]
        [SerializeField] private float duration = 10f;
        [SerializeField] private int beatCount = 5;
        [SerializeField] private float firstBeatDelay = 1.5f;
        [SerializeField] private float beatInterval = 1.4f;

        private static Canvas _sharedCanvas;
        private GameObject _panel;
        private Text _statusText;
        private List<Image> _circles = new();
        private List<float> _beatTimes = new();   // expected timestamps
        private List<float> _pressTimes = new();  // actual press timestamps (-1 = missed)
        private int _nextBeatIndex;

        private bool _isPlaying;
        private float _runTime;

        public void StartTuning(Vector3 _, Action onComplete)
        {
            EnsureUI();
            _isPlaying = true;
            _runTime = 0f;
            _nextBeatIndex = 0;
            CurrentAccuracy = 0f;
            _beatTimes.Clear();
            _pressTimes.Clear();
            for (int i = 0; i < beatCount; i++) _beatTimes.Add(firstBeatDelay + i * beatInterval);
            for (int i = 0; i < beatCount; i++) _pressTimes.Add(-1f);

            if (_panel != null) _panel.SetActive(true);
            if (_statusText != null) _statusText.text = "Tap to the rhythm";
            // Reset circle visuals
            for (int i = 0; i < _circles.Count; i++)
            {
                if (_circles[i] != null) _circles[i].color = new Color(0.4f, 0.3f, 0.2f, 0.5f);
            }
            Debug.Log("[TuningVariantC] Started (harmonic pattern).");
        }

        /// <summary>
        /// Config-driven entry point matching <see cref="TuningMiniGame.StartTuning(TuningPuzzleConfig)"/>
        /// so InteractableBuilding and TuningPedestalLink can dispatch to Variant C by config alone.
        /// Maps docs/15 §9 per-node fields:
        ///   timeLimitSeconds → duration
        ///   difficultySpeed (0.30/0.55/0.85) → beatInterval (1.8s slow → 0.9s fast)
        /// tolerancePercent is unused — Variant C accuracy comes from ±ms timing windows, not slider precision.
        /// </summary>
        public void StartTuning(TuningPuzzleConfig config)
        {
            if (config != null)
            {
                duration     = config.timeLimitSeconds > 0f ? config.timeLimitSeconds : duration;
                float diff   = Mathf.Clamp01(config.difficultySpeed);
                beatInterval = Mathf.Lerp(1.8f, 0.9f, diff);
                // Re-fit the beat count so beats span (duration - firstBeatDelay) cleanly.
                int fittedBeats = Mathf.Max(3, Mathf.FloorToInt((duration - firstBeatDelay) / Mathf.Max(0.4f, beatInterval)));
                beatCount = Mathf.Min(8, fittedBeats);
            }
            StartTuning(transform.position, null);
        }

        void EnsureUI()
        {
            if (_panel != null) return;

            if (_sharedCanvas == null)
            {
                var canvasGO = new GameObject("TuningCanvas_VariantC");
                _sharedCanvas = canvasGO.AddComponent<Canvas>();
                _sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _sharedCanvas.sortingOrder = 100;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasGO);
            }

            _panel = new GameObject("PatternPanel");
            _panel.transform.SetParent(_sharedCanvas.transform, false);
            var prt = _panel.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.18f);
            prt.anchorMax = new Vector2(0.5f, 0.18f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(900f, 220f);
            prt.anchoredPosition = Vector2.zero;
            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.04f, 0.85f);

            // Status text
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
            _statusText.text = "Tap to the rhythm";

            // 5 circles in a horizontal row
            for (int i = 0; i < beatCount; i++)
            {
                var circleGO = new GameObject($"Circle_{i}");
                circleGO.transform.SetParent(_panel.transform, false);
                var rt = circleGO.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.4f);
                rt.anchorMax = new Vector2(0.5f, 0.4f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(80f, 80f);
                float xOff = (i - (beatCount - 1) * 0.5f) * 130f;
                rt.anchoredPosition = new Vector2(xOff, 0f);
                var img = circleGO.AddComponent<Image>();
                img.color = new Color(0.4f, 0.3f, 0.2f, 0.5f);
                _circles.Add(img);
            }
        }

        void Update()
        {
            if (!_isPlaying) return;

            _runTime += Time.unscaledDeltaTime;

            // Highlight the next beat's circle as it approaches
            if (_nextBeatIndex < beatCount)
            {
                float dueAt = _beatTimes[_nextBeatIndex];
                float diff = dueAt - _runTime;
                if (_circles[_nextBeatIndex] != null)
                {
                    float intensity = Mathf.Clamp01(1f - Mathf.Abs(diff) / 0.5f);
                    _circles[_nextBeatIndex].color = Color.Lerp(
                        new Color(0.4f, 0.3f, 0.2f, 0.5f),
                        new Color(0.95f, 0.78f, 0.20f, 1.0f),
                        intensity);
                }

                // Check input
                bool kbPress = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
                bool gpPress = Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
                if (kbPress || gpPress)
                {
                    _pressTimes[_nextBeatIndex] = _runTime;
                    // Show hit color based on timing precision
                    float absDiff = Mathf.Abs(diff);
                    Color hitColor;
                    if (absDiff < 0.10f) hitColor = new Color(0.20f, 0.95f, 0.40f); // perfect green
                    else if (absDiff < 0.20f) hitColor = new Color(0.50f, 0.90f, 0.40f); // good
                    else if (absDiff < 0.30f) hitColor = new Color(0.90f, 0.70f, 0.20f); // ok
                    else hitColor = new Color(0.85f, 0.30f, 0.30f); // miss
                    if (_circles[_nextBeatIndex] != null) _circles[_nextBeatIndex].color = hitColor;
                    _nextBeatIndex++;
                }
                else if (diff < -0.30f)
                {
                    // Missed window — circle stays dim red
                    if (_circles[_nextBeatIndex] != null) _circles[_nextBeatIndex].color = new Color(0.6f, 0.2f, 0.2f, 0.7f);
                    _nextBeatIndex++;
                }
            }

            if (_runTime >= duration || _nextBeatIndex >= beatCount)
            {
                FinishAndScore();
            }
        }

        void FinishAndScore()
        {
            _isPlaying = false;
            // Score: each press contributes based on timing (perfect=1.0, good=0.8, ok=0.5, miss=0)
            float total = 0f;
            for (int i = 0; i < beatCount; i++)
            {
                if (_pressTimes[i] < 0f) continue; // missed
                float diff = Mathf.Abs(_pressTimes[i] - _beatTimes[i]);
                if (diff < 0.10f) total += 1.0f;
                else if (diff < 0.20f) total += 0.8f;
                else if (diff < 0.30f) total += 0.5f;
            }
            float accuracy = total / beatCount;
            CurrentAccuracy = accuracy;
            string tier = TuningMiniGame.GetAccuracyTier(accuracy);
            if (_panel != null) _panel.SetActive(false);

            if (accuracy >= 0.6f)
            {
                Debug.Log($"[TuningVariantC] SUCCESS! Accuracy {accuracy:P0} ({tier})");
                ServiceLocator.HUD?.ShowBanner("TUNED!", $"{tier} - Pattern locked", 3f);
                OnTuningComplete?.Invoke(accuracy);
            }
            else
            {
                Debug.Log($"[TuningVariantC] FAILED — accuracy {accuracy:P0}");
                ServiceLocator.HUD?.ShowBanner("FAILED", "Pattern broken", 3f);
                OnTuningFailed?.Invoke();
            }
        }
    }
}
