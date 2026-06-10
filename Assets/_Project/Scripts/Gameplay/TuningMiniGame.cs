using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// TuningMiniGame - Frequency matching mini-game for building restoration.
    /// Player adjusts frequency slider to match target (432 Hz, 528 Hz, etc.).
    /// Per 15_MVP_BUILD_SPEC.md.
    ///
    /// Phase 1: auto-creates its UI canvas at runtime if SerializedFields are null,
    /// so no per-building Inspector wiring is needed.
    /// </summary>
    public class TuningMiniGame : MonoBehaviour, ITuningVariant
    {
        // Events subscribed by InteractableBuilding + HUD systems
        public event Action<float> OnTuningComplete;
        public event Action OnTuningFailed;
        public event Action<float> OnFrequencyChanged;

        // Read-only state for HUD/audio bindings
        public bool IsActive => isPlaying;
        public float CurrentAccuracy { get; private set; }

        /// <summary>Maps 0-1 accuracy to Perfect/Great/Good/Fail per docs/15 MVP § 9.</summary>
        public static string GetAccuracyTier(float accuracy)
        {
            if (accuracy >= 0.95f) return "Perfect";
            if (accuracy >= 0.80f) return "Great";
            if (accuracy >= 0.60f) return "Good";
            return "Fail";
        }

        [Header("UI Elements (optional — auto-created if null)")]
        [SerializeField] private GameObject tuningUI;
        [SerializeField] private Slider frequencySlider;
        [SerializeField] private Text currentFrequencyText;
        [SerializeField] private Text targetFrequencyText;
        [SerializeField] private Image feedbackMeter;

        [Header("Game Settings")]
        [SerializeField] private float targetFrequency = 432f;
        [SerializeField] private float tolerance = 5f;
        [SerializeField] private float timeLimit = 30f;
        [SerializeField] private bool isPlaying = false;

        private System.Action _onComplete;
        private float _timer;
        private static Canvas _sharedCanvas;  // one shared canvas across all TuningMiniGame instances

        // 2026-05-30 playtest fix: Unity 6 Image without a sprite renders nothing.
        // Every UI element auto-built here was assigned a color but no sprite — that's
        // why the panel was "running" but invisible. Create a 1x1 white sprite once,
        // assign to every Image so the colored quad actually shows up.
        private static Sprite _whiteSprite;
        static Sprite GetWhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;
            var tex = Texture2D.whiteTexture;
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            return _whiteSprite;
        }

        void Start()
        {
            // R418 bug fix: only hide if NOT mid-tuning. The old version hid the
            // panel even when StartTuning() had already been called in Awake-phase.
            if (tuningUI != null && !isPlaying) tuningUI.SetActive(false);
        }

        /// <summary>
        /// Auto-create a Canvas + Panel + Slider + Texts + FeedbackMeter at runtime.
        /// Called on first StartTuning if SerializedFields are still null.
        /// </summary>
        void EnsureUIBuilt()
        {
            if (tuningUI != null) return;

            // Find or create the shared canvas
            if (_sharedCanvas == null)
            {
                var canvasGO = new GameObject("TuningCanvas_Auto");
                _sharedCanvas = canvasGO.AddComponent<Canvas>();
                _sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                // 2026-05-30 playtest fix: was 100, but main HUD canvas was higher and
                // hid the tuning UI. Bump to 32000 (Unity Canvas max-ish) so it's on top.
                _sharedCanvas.sortingOrder = 32000;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasGO);
            }

            // Build the tuning panel
            tuningUI = new GameObject("TuningPanel_" + gameObject.name);
            tuningUI.transform.SetParent(_sharedCanvas.transform, false);
            var panelRect = tuningUI.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.15f);
            panelRect.anchorMax = new Vector2(0.5f, 0.15f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900f, 220f);
            panelRect.anchoredPosition = Vector2.zero;

            // Background
            var bg = tuningUI.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = new Color(0.08f, 0.06f, 0.04f, 0.85f);

            // Target frequency text (top)
            var targetGO = new GameObject("TargetFrequencyText");
            targetGO.transform.SetParent(tuningUI.transform, false);
            var targetRT = targetGO.AddComponent<RectTransform>();
            targetRT.anchorMin = new Vector2(0.5f, 1f);
            targetRT.anchorMax = new Vector2(0.5f, 1f);
            targetRT.pivot = new Vector2(0.5f, 1f);
            targetRT.sizeDelta = new Vector2(800f, 50f);
            targetRT.anchoredPosition = new Vector2(0f, -10f);
            targetFrequencyText = targetGO.AddComponent<Text>();
            targetFrequencyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            targetFrequencyText.fontSize = 32;
            targetFrequencyText.alignment = TextAnchor.MiddleCenter;
            targetFrequencyText.color = new Color(0.85f, 0.65f, 0.10f); // golden
            targetFrequencyText.text = "Target: 432 Hz";

            // Current frequency text (middle)
            var currentGO = new GameObject("CurrentFrequencyText");
            currentGO.transform.SetParent(tuningUI.transform, false);
            var currentRT = currentGO.AddComponent<RectTransform>();
            currentRT.anchorMin = new Vector2(0.5f, 0.5f);
            currentRT.anchorMax = new Vector2(0.5f, 0.5f);
            currentRT.pivot = new Vector2(0.5f, 0.5f);
            currentRT.sizeDelta = new Vector2(800f, 60f);
            currentRT.anchoredPosition = new Vector2(0f, 20f);
            currentFrequencyText = currentGO.AddComponent<Text>();
            currentFrequencyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            currentFrequencyText.fontSize = 42;
            currentFrequencyText.fontStyle = FontStyle.Bold;
            currentFrequencyText.alignment = TextAnchor.MiddleCenter;
            currentFrequencyText.color = Color.white;
            currentFrequencyText.text = "432.0 Hz";

            // Feedback meter (thin colored bar above slider)
            var meterGO = new GameObject("FeedbackMeter");
            meterGO.transform.SetParent(tuningUI.transform, false);
            var meterRT = meterGO.AddComponent<RectTransform>();
            meterRT.anchorMin = new Vector2(0.05f, 0.30f);
            meterRT.anchorMax = new Vector2(0.95f, 0.30f);
            meterRT.pivot = new Vector2(0.5f, 0.5f);
            meterRT.sizeDelta = new Vector2(0f, 12f);
            meterRT.anchoredPosition = Vector2.zero;
            feedbackMeter = meterGO.AddComponent<Image>();
            feedbackMeter.sprite = GetWhiteSprite();
            feedbackMeter.color = Color.red;

            // Slider (bottom)
            var sliderGO = new GameObject("FrequencySlider");
            sliderGO.transform.SetParent(tuningUI.transform, false);
            var sliderRT = sliderGO.AddComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.05f, 0.05f);
            sliderRT.anchorMax = new Vector2(0.95f, 0.20f);
            sliderRT.pivot = new Vector2(0.5f, 0.5f);
            sliderRT.sizeDelta = Vector2.zero;
            sliderRT.anchoredPosition = Vector2.zero;
            frequencySlider = sliderGO.AddComponent<Slider>();
            frequencySlider.minValue = 332f;
            frequencySlider.maxValue = 532f;
            frequencySlider.value = 432f;

            // Slider visual children — Background, FillArea, Handle
            BuildSliderVisuals(sliderGO, frequencySlider);

            Debug.Log("[TuningMiniGame] Auto-built UI: Canvas + Panel + Slider + Texts + Meter");
        }

        void BuildSliderVisuals(GameObject sliderRoot, Slider slider)
        {
            // Background image
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(sliderRoot.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = GetWhiteSprite();
            bgImg.color = new Color(0.2f, 0.15f, 0.1f, 1f);

            // Fill area + fill
            var fillAreaGO = new GameObject("Fill Area");
            fillAreaGO.transform.SetParent(sliderRoot.transform, false);
            var fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRT.sizeDelta = Vector2.zero;

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.sprite = GetWhiteSprite();
            fillImg.color = new Color(0.85f, 0.65f, 0.10f, 1f); // gold
            slider.fillRect = fillRT;

            // Handle slide area + handle
            var handleAreaGO = new GameObject("Handle Slide Area");
            handleAreaGO.transform.SetParent(sliderRoot.transform, false);
            var handleAreaRT = handleAreaGO.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.sizeDelta = Vector2.zero;

            var handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            var handleRT = handleGO.AddComponent<RectTransform>();
            handleRT.anchorMin = new Vector2(0f, 0f);
            handleRT.anchorMax = new Vector2(0f, 1f);
            handleRT.sizeDelta = new Vector2(20f, 0f);
            var handleImg = handleGO.AddComponent<Image>();
            handleImg.sprite = GetWhiteSprite();
            handleImg.color = Color.white;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
        }

        public void StartTuning(Vector3 nodePosition, System.Action onComplete)
        {
            _onComplete = onComplete;
            isPlaying = true;
            _timer = timeLimit;
            ApplyDifficultyForgiveness();

            // Auto-build UI if not assigned in inspector
            EnsureUIBuilt();

            // Randomize target frequency (Solfeggio scale)
            float[] frequencies = { 432f, 528f, 639f, 741f, 852f };
            targetFrequency = frequencies[UnityEngine.Random.Range(0, frequencies.Length)];

            if (tuningUI != null) tuningUI.SetActive(true);

            if (frequencySlider != null)
            {
                frequencySlider.minValue = targetFrequency - 100f;
                frequencySlider.maxValue = targetFrequency + 100f;
                // Start somewhere off-target so player has to actually tune
                float offset = UnityEngine.Random.Range(0, 2) == 0 ? -60f : 60f;
                frequencySlider.value = targetFrequency + offset + UnityEngine.Random.Range(-15f, 15f);
            }

            if (targetFrequencyText != null)
                targetFrequencyText.text = $"Target: {targetFrequency:F0} Hz";

            // R418: instruction overlay + control hint
            ShowInstructionOverlay($"Move slider to match {targetFrequency:F0} Hz",
                Tartaria.Input.InputPromptHelper.Localize("[A/D] or [L-Stick] - Adjust  •  [E] - Confirm"));
            Debug.Log($"[TuningMiniGame] Started! Target: {targetFrequency} Hz (move slider to match)");
        }

        void ShowInstructionOverlay(string instruction, string controls)
        {
            if (tuningUI == null) return;
            // Find or create instruction Text
            var existing = tuningUI.transform.Find("InstructionText");
            UnityEngine.UI.Text instrText;
            if (existing == null) {
                var go = new GameObject("InstructionText");
                go.transform.SetParent(tuningUI.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(0f, 28f);
                rt.anchoredPosition = new Vector2(0f, 28f);
                instrText = go.AddComponent<UnityEngine.UI.Text>();
                instrText.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                instrText.fontSize = 18;
                instrText.alignment = TextAnchor.MiddleCenter;
                instrText.color = new Color(1f, 0.95f, 0.8f);
                instrText.raycastTarget = false;
            } else instrText = existing.GetComponent<UnityEngine.UI.Text>();
            instrText.text = instruction;
            // Same for controls hint
            var ctrlExisting = tuningUI.transform.Find("ControlsHint");
            UnityEngine.UI.Text ctrlText;
            if (ctrlExisting == null) {
                var go = new GameObject("ControlsHint");
                go.transform.SetParent(tuningUI.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(0f, 24f);
                rt.anchoredPosition = new Vector2(0f, -8f);
                ctrlText = go.AddComponent<UnityEngine.UI.Text>();
                ctrlText.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                ctrlText.fontSize = 14;
                ctrlText.alignment = TextAnchor.MiddleCenter;
                ctrlText.color = new Color(0.8f, 0.7f, 0.5f);
                ctrlText.raycastTarget = false;
            } else ctrlText = ctrlExisting.GetComponent<UnityEngine.UI.Text>();
            ctrlText.text = controls;
        }

        /// <summary>
        /// Config-driven overload used by InteractableBuilding so each node puzzle can
        /// specify its own target frequency, time limit, tolerance, and difficulty.
        /// Subscribers consume completion/failure via OnTuningComplete / OnTuningFailed
        /// events (no Action callback needed at this call site).
        /// </summary>
        public void StartTuning(TuningPuzzleConfig config)
        {
            if (config == null)
            {
                // Fall through to the randomized default behaviour rather than no-op.
                StartTuning(transform.position, null);
                return;
            }

            // Apply config to the inspector-mirrored fields so Update() picks them up.
            targetFrequency = config.targetFrequency;
            timeLimit = config.timeLimitSeconds;
            // tolerancePercent is a 0-1 fraction of the ±100 Hz slider range.
            tolerance = Mathf.Max(0.5f, config.tolerancePercent * 100f);
            ApplyDifficultyForgiveness();

            _onComplete = null;          // event-driven path — see OnTuningComplete/OnTuningFailed
            isPlaying = true;
            _timer = timeLimit;
            CurrentAccuracy = 0f;

            EnsureUIBuilt();

            if (tuningUI != null) tuningUI.SetActive(true);

            if (frequencySlider != null)
            {
                frequencySlider.minValue = targetFrequency - 100f;
                frequencySlider.maxValue = targetFrequency + 100f;

                // Difficulty drives how far off-target we start (0.3 → ±40 Hz, 1.0 → ±90 Hz).
                float diff = Mathf.Clamp01(config.difficultySpeed);
                float offsetMag = Mathf.Lerp(40f, 90f, diff);
                float sign = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f;
                frequencySlider.value = targetFrequency + sign * offsetMag + UnityEngine.Random.Range(-10f, 10f);
            }

            if (targetFrequencyText != null)
                targetFrequencyText.text = $"Target: {targetFrequency:F0} Hz";

            Debug.Log($"[TuningMiniGame] Started (config) variant={config.variant} target={targetFrequency}Hz tol=±{tolerance:F1}Hz time={timeLimit:F1}s");
        }

        void Update()
        {
            if (!isPlaying) return;

            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0)
            {
                FailTuning();
                return;
            }

            if (frequencySlider != null && currentFrequencyText != null)
            {
                // 2026-05-30 playtest fix: Unity's UI Slider doesn't respond to
                // Left/Right arrows out of the box. Nudge directly from input.
                // Keyboard: Left/Right or A/D. Gamepad: left stick X.
                float nudge = 0f;
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null)
                {
                    if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) nudge -= 80f * Time.unscaledDeltaTime;
                    if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) nudge += 80f * Time.unscaledDeltaTime;
                }
                var pad = UnityEngine.InputSystem.Gamepad.current;
                if (pad != null)
                {
                    float sx = pad.leftStick.ReadValue().x;
                    if (Mathf.Abs(sx) > 0.15f) nudge += sx * 80f * Time.unscaledDeltaTime;
                }
                if (Mathf.Abs(nudge) > 0.0001f)
                    frequencySlider.value = Mathf.Clamp(frequencySlider.value + nudge, frequencySlider.minValue, frequencySlider.maxValue);

                float current = frequencySlider.value;
                currentFrequencyText.text = $"{current:F1} Hz";

                float distance = Mathf.Abs(current - targetFrequency);
                float accuracy = 1f - Mathf.Clamp01(distance / 100f);

                CurrentAccuracy = accuracy;
                OnFrequencyChanged?.Invoke(current);

                if (feedbackMeter != null)
                {
                    feedbackMeter.color = Color.Lerp(Color.red, Color.green, accuracy);
                }

                if (distance <= tolerance)
                {
                    SucceedTuning(accuracy);
                }
            }
        }

        void SucceedTuning(float accuracy)
        {
            if (!isPlaying) return;
            isPlaying = false;
            CurrentAccuracy = accuracy;

            string tier = GetAccuracyTier(accuracy);
            Debug.Log($"[TuningMiniGame] SUCCESS — {tier} ({accuracy:P0}) at {targetFrequency:F0} Hz");

            CleanupTuning();

            // Notify via event AND legacy callback so both subscriber styles work.
            OnTuningComplete?.Invoke(accuracy);
            _onComplete?.Invoke();
        }

        void FailTuning()
        {
            if (!isPlaying) return;
            isPlaying = false;
            Debug.Log("[TuningMiniGame] FAILED (timeout)");

            CleanupTuning();

            OnTuningFailed?.Invoke();
        }

        void CleanupTuning()
        {
            if (tuningUI != null) tuningUI.SetActive(false);
        }

        /// <summary>Sprint 7 Lane 2 - applies MiniGameForgiveness multiplier from DifficultyController to the active tolerance window. Story raises tolerance, Hardened tightens it.</summary>
        void ApplyDifficultyForgiveness()
        {
            float mul;
            try { mul = Tartaria.Gameplay.DifficultyController.MiniGameForgiveness; }
            catch (System.Exception e) { Debug.LogWarning("[DifficultyApply] TuningMiniGame.ApplyDifficultyForgiveness: lookup threw " + e.GetType().Name + ": " + e.Message + " - using 1.0"); mul = 1f; }
            mul = Mathf.Clamp(mul, 0.1f, 5f);
            float before = tolerance;
            tolerance = Mathf.Max(0.5f, tolerance * mul);
            Debug.Log("[DifficultyApply] tolerance=" + tolerance.ToString("F2") + " (multiplier=" + mul.ToString("F2") + ", was=" + before.ToString("F2") + ")");
        }

        public bool IsPlaying() => isPlaying;
    }
}
