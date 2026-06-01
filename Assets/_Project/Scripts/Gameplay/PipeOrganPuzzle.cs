using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// PipeOrganPuzzle — the canonical Moon 1 puzzle per docs/03_CAMPAIGN_13_MOONS.md
    /// Days 6–12: "First tuning mini-game: simple 3-note organ sequence. Perfect tune
    /// = dome glows, rose window projects cymatic pattern onto the floor, pure water
    /// font trickles back to life."
    ///
    /// Mechanic: 3 pipes show their target frequency by glowing at the right key.
    /// Player listens (via an audible reference tone) then plays the sequence
    /// using number keys 1-7 (do/re/mi/fa/sol/la/ti) or D-pad. Each correct note
    /// in sequence lights the next pipe. Wrong note resets the sequence.
    ///
    /// Implements ITuningVariant so InteractableBuilding can pick this for
    /// Moon-1-themed buildings (the Dome / Listeners' Hall) instead of the
    /// generic slider Variant A.
    ///
    /// Per docs/03: "Excavated stone carved with 3-6-9 sequence and golden-ratio
    /// diagrams" — so the default 3-note sequence is rooted in 3 / 6 / 9 indexing:
    /// pipes 3, 6, 9 of a 12-note Solfeggio array. Players can succeed by tapping
    /// the 3 lit-up pipes in order.
    /// </summary>
    public class PipeOrganPuzzle : MonoBehaviour, ITuningVariant
    {
        public event Action<float> OnTuningComplete;
        public event Action OnTuningFailed;
        public event Action<float> OnFrequencyChanged;

        public bool IsActive => _isPlaying;
        public float CurrentAccuracy { get; private set; }

        [Header("Difficulty (docs/03 Days 6–12)")]
        [SerializeField] private float timeLimit = 30f;
        [SerializeField] private int sequenceLength = 3;
        [SerializeField] private int pipeCount = 7; // 7-pipe Solfeggio organ

        // Solfeggio frequencies (Hz) per pipe — 528 (mi) is the foundation
        static readonly float[] PIPE_FREQS = { 396f, 417f, 528f, 639f, 741f, 852f, 963f };
        static readonly string[] PIPE_LABELS = { "do", "re", "mi", "fa", "sol", "la", "ti" };

        private static Canvas _sharedCanvas;
        private GameObject _panel;
        private Text _statusText;
        private Text _hintText;
        private List<Image> _pipeVisuals = new();
        private List<int> _sequence = new();
        private List<int> _played = new();
        private bool _isPlaying;
        private float _timer;
        private AudioSource _audioSource;

        public void StartTuning(Vector3 _, Action onComplete)
        {
            EnsureUI();
            _isPlaying = true;
            _timer = timeLimit;
            _played.Clear();
            CurrentAccuracy = 0f;

            // Pick 3 pipes that include 3, 6, 9 indexing flavor — but constrained to 7-pipe array
            // so we use indices 0, 2, 5 (do, mi, la) as the default "3-6-9 flavor" within Solfeggio.
            _sequence.Clear();
            _sequence.Add(0); // do
            _sequence.Add(2); // mi (the 528 Hz foundation)
            _sequence.Add(5); // la
            while (_sequence.Count < sequenceLength) _sequence.Add(UnityEngine.Random.Range(0, pipeCount));

            if (_panel != null) _panel.SetActive(true);
            UpdateStatus();
            PreviewSequence();
            Debug.Log("[PipeOrganPuzzle] Started — sequence: " + string.Join(",", _sequence));
        }

        void EnsureUI()
        {
            if (_panel != null) return;

            if (_sharedCanvas == null)
            {
                var canvasGO = new GameObject("TuningCanvas_PipeOrgan");
                _sharedCanvas = canvasGO.AddComponent<Canvas>();
                _sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _sharedCanvas.sortingOrder = 100;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasGO);
            }

            _panel = new GameObject("PipeOrganPanel");
            _panel.transform.SetParent(_sharedCanvas.transform, false);
            var prt = _panel.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.18f);
            prt.anchorMax = new Vector2(0.5f, 0.18f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(1000f, 280f);
            prt.anchoredPosition = Vector2.zero;
            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.05f, 0.03f, 0.92f);

            // Title
            var titleGO = new GameObject("Status");
            titleGO.transform.SetParent(_panel.transform, false);
            var trt = titleGO.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 1f);
            trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.sizeDelta = new Vector2(900f, 40f);
            trt.anchoredPosition = new Vector2(0f, -10f);
            _statusText = titleGO.AddComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 28;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color = new Color(0.95f, 0.78f, 0.20f);
            _statusText.text = "Tune the Pipe Organ — match the sequence";

            // Hint text
            var hintGO = new GameObject("Hint");
            hintGO.transform.SetParent(_panel.transform, false);
            var hrt = hintGO.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.5f, 0f);
            hrt.anchorMax = new Vector2(0.5f, 0f);
            hrt.pivot = new Vector2(0.5f, 0f);
            hrt.sizeDelta = new Vector2(900f, 36f);
            hrt.anchoredPosition = new Vector2(0f, 18f);
            _hintText = hintGO.AddComponent<Text>();
            _hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _hintText.fontSize = 20;
            _hintText.alignment = TextAnchor.MiddleCenter;
            _hintText.color = new Color(0.85f, 0.80f, 0.65f);
            _hintText.text = "Press 1–7 (keyboard) or D-Pad (gamepad)";

            // 7 pipe visuals — vertical rectangles
            for (int i = 0; i < pipeCount; i++)
            {
                var pipeGO = new GameObject($"Pipe_{i}_{PIPE_LABELS[i]}");
                pipeGO.transform.SetParent(_panel.transform, false);
                var pr = pipeGO.AddComponent<RectTransform>();
                pr.anchorMin = new Vector2(0.5f, 0.5f);
                pr.anchorMax = new Vector2(0.5f, 0.5f);
                pr.pivot = new Vector2(0.5f, 0.5f);
                pr.sizeDelta = new Vector2(70f, 140f);
                float xOff = (i - (pipeCount - 1) * 0.5f) * 110f;
                pr.anchoredPosition = new Vector2(xOff, 0f);
                var img = pipeGO.AddComponent<Image>();
                img.color = new Color(0.40f, 0.30f, 0.20f, 0.85f);
                _pipeVisuals.Add(img);

                // Pipe label
                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(pipeGO.transform, false);
                var lrt = labelGO.AddComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0.5f, 0f);
                lrt.anchorMax = new Vector2(0.5f, 0f);
                lrt.pivot = new Vector2(0.5f, 1f);
                lrt.sizeDelta = new Vector2(80f, 28f);
                lrt.anchoredPosition = new Vector2(0f, -2f);
                var label = labelGO.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 18;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = new Color(0.9f, 0.85f, 0.65f);
                label.text = (i + 1) + " " + PIPE_LABELS[i];
            }

            // Audio source for the tones
            _audioSource = _panel.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 0.4f;
        }

        void Update()
        {
            if (!_isPlaying) return;

            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0f)
            {
                Finish(false, 0f);
                return;
            }

            // Read input — number keys 1-7 + gamepad D-pad / face buttons for 4 of them
            int pressed = -1;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) pressed = 0;
                else if (kb.digit2Key.wasPressedThisFrame) pressed = 1;
                else if (kb.digit3Key.wasPressedThisFrame) pressed = 2;
                else if (kb.digit4Key.wasPressedThisFrame) pressed = 3;
                else if (kb.digit5Key.wasPressedThisFrame) pressed = 4;
                else if (kb.digit6Key.wasPressedThisFrame) pressed = 5;
                else if (kb.digit7Key.wasPressedThisFrame) pressed = 6;
            }
            var pad = Gamepad.current;
            if (pad != null && pressed == -1)
            {
                // Map gamepad: dpad-up/right/down/left = 0/1/2/3, face buttons = 4/5/6
                if (pad.dpad.up.wasPressedThisFrame) pressed = 0;
                else if (pad.dpad.right.wasPressedThisFrame) pressed = 1;
                else if (pad.dpad.down.wasPressedThisFrame) pressed = 2;
                else if (pad.dpad.left.wasPressedThisFrame) pressed = 3;
                else if (pad.buttonNorth.wasPressedThisFrame) pressed = 4;
                else if (pad.buttonEast.wasPressedThisFrame) pressed = 5;
                else if (pad.buttonSouth.wasPressedThisFrame) pressed = 6;
            }

            if (pressed >= 0 && pressed < pipeCount)
            {
                PlayPipe(pressed);

                int expected = _sequence[_played.Count];
                if (pressed == expected)
                {
                    _played.Add(pressed);
                    HighlightPipe(pressed, new Color(0.20f, 0.95f, 0.40f));
                    if (_played.Count >= _sequence.Count)
                    {
                        // Perfect — score based on remaining time
                        float accuracy = 0.80f + 0.20f * (_timer / timeLimit);
                        Finish(true, accuracy);
                    }
                    else
                    {
                        UpdateStatus();
                    }
                }
                else
                {
                    HighlightPipe(pressed, new Color(0.85f, 0.30f, 0.30f));
                    _played.Clear();
                    StartCoroutine(ResetPipesAfter(0.5f));
                    if (_statusText != null) _statusText.text = "Wrong note — resetting sequence";
                }
            }
        }

        IEnumerator ResetPipesAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            for (int i = 0; i < _pipeVisuals.Count; i++)
            {
                if (_pipeVisuals[i] != null) _pipeVisuals[i].color = new Color(0.40f, 0.30f, 0.20f, 0.85f);
            }
            UpdateStatus();
        }

        void UpdateStatus()
        {
            if (_statusText == null) return;
            _statusText.text = $"Pipe Organ — sequence {_played.Count}/{_sequence.Count}  ·  {_timer:F0}s";
        }

        void HighlightPipe(int idx, Color c)
        {
            if (idx >= 0 && idx < _pipeVisuals.Count && _pipeVisuals[idx] != null)
                _pipeVisuals[idx].color = c;
        }

        void PreviewSequence()
        {
            StartCoroutine(PreviewRoutine());
        }

        IEnumerator PreviewRoutine()
        {
            yield return new WaitForSecondsRealtime(0.4f);
            foreach (var idx in _sequence)
            {
                HighlightPipe(idx, new Color(0.95f, 0.78f, 0.20f));
                PlayPipe(idx);
                yield return new WaitForSecondsRealtime(0.6f);
                HighlightPipe(idx, new Color(0.40f, 0.30f, 0.20f, 0.85f));
                yield return new WaitForSecondsRealtime(0.15f);
            }
        }

        void PlayPipe(int idx)
        {
            if (_audioSource == null || idx < 0 || idx >= PIPE_FREQS.Length) return;
            // Generate a short sine-wave click at the pipe's frequency
            int sampleRate = 44100;
            int samples = (int)(sampleRate * 0.25f);
            float freq = PIPE_FREQS[idx];
            var clip = AudioClip.Create($"PipeTone_{idx}", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-3f * t);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f * envelope;
            }
            clip.SetData(data, 0);
            _audioSource.PlayOneShot(clip);
        }

        void Finish(bool success, float accuracy)
        {
            _isPlaying = false;
            CurrentAccuracy = accuracy;
            string tier = TuningMiniGame.GetAccuracyTier(accuracy);

            if (_panel != null) _panel.SetActive(false);

            if (success)
            {
                Debug.Log($"[PipeOrganPuzzle] SUCCESS! {tier} ({accuracy:P0})");
                ServiceLocator.HUD?.ShowBanner("ORGAN TUNED!", $"{tier} — the dome remembers", 4f);
                OnTuningComplete?.Invoke(accuracy);
            }
            else
            {
                Debug.Log("[PipeOrganPuzzle] FAILED (timeout)");
                ServiceLocator.HUD?.ShowBanner("FAILED", "The pipes fall silent", 3f);
                OnTuningFailed?.Invoke();
            }
        }
    }
}
