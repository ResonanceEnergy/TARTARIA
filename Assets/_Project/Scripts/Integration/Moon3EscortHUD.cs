using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tartaria.Gameplay; // for SpectralOrphanAdoption

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Dedicated non-OnGUI Lullaby/Escort HUD for Moon 3 Orphan Train Escort (R7 production depth).
    /// Displays: progress, shield strength, frequency match, companion status (physical tells + trust forks), wave timer.
    /// Runtime Canvas + legacy Text (zero asset, Moon 3 exclusive vertical slice).
    /// Keeps existing OnGUI in RailEscortController for quick debug/testing.
    /// Wires directly to RailEscortController public state + events.
    /// Per 03C Moon 3 rails + 11_SCRIPTED_CLIMAXES Orphan Train phases + R6 foundation.
    /// </summary>
    public class Moon3EscortHUD : MonoBehaviour
    {
        RailEscortController _escort;
        Canvas _canvas;
        GameObject _panel;

        // Core texts (TMP for Tartarian polish)
        TextMeshProUGUI _titleText;
        TextMeshProUGUI _progressLabel;
        TextMeshProUGUI _timeText;
        TextMeshProUGUI _shieldText;
        TextMeshProUGUI _matchPercentText;
        TextMeshProUGUI _freqLabel;
        TextMeshProUGUI _companionLirael;
        TextMeshProUGUI _companionMilo;
        TextMeshProUGUI _companionCassian;
        TextMeshProUGUI _waveText;
        TextMeshProUGUI _threatText;
        TextMeshProUGUI _statusBanner;

        // Bars
        RectTransform _progressBarBg;
        RectTransform _progressBarFill;
        RectTransform _trainHealthBarBg;
        RectTransform _trainHealthBarFill;
        RectTransform _shieldBarBg;
        RectTransform _shieldBarFill;

        // Rhythm visualizer (Hz / lullaby matcher)
        RectTransform _freqVizContainer;
        Image[] _freqBars;
        const int FREQ_BAR_COUNT = 14;

        // Branch choice prompt (WindspireJunction)
        GameObject _branchPromptPanel;
        TextMeshProUGUI _branchTitle;
        TextMeshProUGUI _branchDesc;
        float _branchPromptTimer;

        // Internal state
        float _lastProgressForJunction;
        bool _branchPromptShown;
        bool _subscribed;

        public void Initialize(RailEscortController escort)
        {
            _escort = escort;
            _lastProgressForJunction = 0f;
            _branchPromptShown = false;
            Instance = this;
            CreateRuntimeCanvasHUD();
            Debug.Log("[Moon3 R7 HUD] Dedicated escort HUD initialized (non-OnGUI Canvas, Tartarian style, gamepad-friendly). Lullaby rhythm visualizer active.");
        }

        public static Moon3EscortHUD Instance { get; private set; }

        // Lullaby rhythm visual feedback (called by Moon3RailAudioManager on successful 432Hz tap)
        public void FlashLullabySuccess(int streak)
        {
            if (_shieldBarFill != null)
            {
                StartCoroutine(PulseShieldBar(streak));
            }
            if (_matchPercentText != null)
            {
                _matchPercentText.text = $"HEARTBEAT  x{Mathf.Min(streak, 9)}  •  432 Hz";
                StartCoroutine(ResetMatchTextAfter(0.9f));
            }
            // Strong visual pop for emotional payoff
            Debug.Log($"[Moon3 HUD] Lullaby rhythm success flash — streak {streak}");
        }

        System.Collections.IEnumerator PulseShieldBar(int streak)
        {
            if (_shieldBarFill == null) yield break;
            var img = _shieldBarFill.GetComponent<Image>();
            if (img == null) yield break;
            Color original = img.color;
            Color bright = new Color(1f, 0.98f, 0.65f, 1f);
            float dur = 0.28f + Mathf.Min(streak, 6) * 0.03f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                img.color = Color.Lerp(original, bright, Mathf.Sin(t / dur * Mathf.PI));
                yield return null;
            }
            img.color = original;
        }

        System.Collections.IEnumerator ResetMatchTextAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            // Will be overwritten by normal Update next frame anyway
        }

        void CreateRuntimeCanvasHUD()
        {
            // Canvas at runtime — top-center overlay for escort only (Moon 3 exclusive)
            var canvasGO = new GameObject("Moon3_EscortHUD_Canvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 520; // above core HUDs

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // === MAIN TARTARIAN PANEL (deep indigo + gold trim) ===
            _panel = new GameObject("EscortPanel");
            _panel.transform.SetParent(canvasGO.transform, false);
            var rect = _panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -8);
            rect.sizeDelta = new Vector2(1080, 198);

            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.045f, 0.055f, 0.10f, 0.90f); // deep electric indigo
            bg.raycastTarget = false;

            // Gold outer trim (thin border simulation via child images)
            CreateGoldTrim(_panel.transform, 1080, 198);

            // === HEADER: Title + Wave/Threat (right aligned) ===
            _titleText = CreateTMP(_panel.transform, "❖  ORPHAN TRAIN ESCORT  ❖", new Vector2(0, -14), 22, TextAlignmentOptions.Center, new Color(0.96f, 0.88f, 0.55f));
            _titleText.fontStyle = FontStyles.Bold;

            var subTitle = CreateTMP(_panel.transform, "WINDSWEPT HIGHLANDS  •  ELECTRIC MOON 3  •  COMPASSION & RAILS", new Vector2(0, -32), 11, TextAlignmentOptions.Center, new Color(0.65f, 0.78f, 0.92f));

            // Wave & Threat (right side header)
            _waveText = CreateTMP(_panel.transform, "WAVE  0/10", new Vector2(420, -14), 16, TextAlignmentOptions.Right, new Color(0.95f, 0.82f, 0.35f));
            _waveText.fontStyle = FontStyles.Bold;
            _threatText = CreateTMP(_panel.transform, "THREATS: 00", new Vector2(420, -30), 12, TextAlignmentOptions.Right, new Color(0.95f, 0.55f, 0.45f));

            // === PROGRESS SECTION ===
            // Label
            _progressLabel = CreateTMP(_panel.transform, "ESCORT PROGRESS", new Vector2(-460, -52), 10, TextAlignmentOptions.Left, new Color(0.7f, 0.85f, 0.95f));

            // Big progress bar bg (wide, elegant)
            var pBgGO = new GameObject("ProgressBg");
            pBgGO.transform.SetParent(_panel.transform, false);
            _progressBarBg = pBgGO.AddComponent<RectTransform>();
            _progressBarBg.anchoredPosition = new Vector2(0, -62);
            _progressBarBg.sizeDelta = new Vector2(860, 16);
            var pBgImg = pBgGO.AddComponent<Image>();
            pBgImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            // Gold progress fill
            var pFillGO = new GameObject("ProgressFill");
            pFillGO.transform.SetParent(pBgGO.transform, false);
            _progressBarFill = pFillGO.AddComponent<RectTransform>();
            _progressBarFill.anchorMin = new Vector2(0, 0);
            _progressBarFill.anchorMax = new Vector2(0, 1);
            _progressBarFill.pivot = new Vector2(0, 0.5f);
            _progressBarFill.sizeDelta = new Vector2(0, 0);
            var pFillImg = pFillGO.AddComponent<Image>();
            pFillImg.color = new Color(0.55f, 0.88f, 0.72f, 1f); // living rail green-gold

            // Time readout (right of bar)
            _timeText = CreateTMP(_panel.transform, "00:00 / 07:00", new Vector2(460, -52), 13, TextAlignmentOptions.Right, Color.white);

            // === METRICS ROW: Train Health | Lullaby Shield | Freq Visualizer | Companions ===
            float metricY = -88f;

            // --- Train Health (left) ---
            var thLabel = CreateTMP(_panel.transform, "TRAIN HEALTH", new Vector2(-440, metricY), 9, TextAlignmentOptions.Left, new Color(0.95f, 0.65f, 0.55f));
            var thBgGO = new GameObject("TrainHealthBg");
            thBgGO.transform.SetParent(_panel.transform, false);
            _trainHealthBarBg = thBgGO.AddComponent<RectTransform>();
            _trainHealthBarBg.anchoredPosition = new Vector2(-330, metricY - 14);
            _trainHealthBarBg.sizeDelta = new Vector2(210, 11);
            thBgGO.AddComponent<Image>().color = new Color(0.18f, 0.10f, 0.08f, 0.9f);

            var thFillGO = new GameObject("TrainHealthFill");
            thFillGO.transform.SetParent(thBgGO.transform, false);
            _trainHealthBarFill = thFillGO.AddComponent<RectTransform>();
            _trainHealthBarFill.anchorMin = new Vector2(0, 0);
            _trainHealthBarFill.anchorMax = new Vector2(0, 1);
            _trainHealthBarFill.pivot = new Vector2(0, 0.5f);
            _trainHealthBarFill.sizeDelta = new Vector2(0, 0);
            var thFill = thFillGO.AddComponent<Image>();
            thFill.color = new Color(0.92f, 0.55f, 0.35f, 1f);

            // --- Lullaby Shield (next) ---
            var shLabel = CreateTMP(_panel.transform, "LULLABY SHIELD", new Vector2(-180, metricY), 9, TextAlignmentOptions.Left, new Color(1f, 0.9f, 0.45f));
            var shBgGO = new GameObject("ShieldBg");
            shBgGO.transform.SetParent(_panel.transform, false);
            _shieldBarBg = shBgGO.AddComponent<RectTransform>();
            _shieldBarBg.anchoredPosition = new Vector2(-70, metricY - 14);
            _shieldBarBg.sizeDelta = new Vector2(210, 11);
            shBgGO.AddComponent<Image>().color = new Color(0.22f, 0.18f, 0.08f, 0.9f);

            var shFillGO = new GameObject("ShieldFill");
            shFillGO.transform.SetParent(shBgGO.transform, false);
            _shieldBarFill = shFillGO.AddComponent<RectTransform>();
            _shieldBarFill.anchorMin = new Vector2(0, 0);
            _shieldBarFill.anchorMax = new Vector2(0, 1);
            _shieldBarFill.pivot = new Vector2(0, 0.5f);
            _shieldBarFill.sizeDelta = new Vector2(0, 0);
            var shFill = shFillGO.AddComponent<Image>();
            shFill.color = new Color(0.96f, 0.82f, 0.35f, 1f); // warm lullaby gold

            _shieldText = CreateTMP(_panel.transform, "1.00×  •  0 ORPHANS", new Vector2(-70, metricY - 28), 10, TextAlignmentOptions.Center, new Color(1f, 0.92f, 0.5f));

            // --- FREQUENCY / RHYTHM MATCHER VISUALIZER (center, the star of the HUD) ---
            var freqTitle = CreateTMP(_panel.transform, "LULLABY RESONANCE  •  RHYTHM MATCHER", new Vector2(80, metricY), 9, TextAlignmentOptions.Center, new Color(0.55f, 0.92f, 0.98f));

            _matchPercentText = CreateTMP(_panel.transform, "MATCH  00%", new Vector2(80, metricY - 14), 14, TextAlignmentOptions.Center, new Color(0.7f, 0.98f, 1f));
            _matchPercentText.fontStyle = FontStyles.Bold;

            // Viz container
            var vizGO = new GameObject("FreqViz");
            vizGO.transform.SetParent(_panel.transform, false);
            _freqVizContainer = vizGO.AddComponent<RectTransform>();
            _freqVizContainer.anchoredPosition = new Vector2(80, metricY - 38);
            _freqVizContainer.sizeDelta = new Vector2(240, 38);

            // Create 14 elegant vertical bars for waveform
            _freqBars = new Image[FREQ_BAR_COUNT];
            float barWidth = 9f;
            float gap = 3.5f;
            float startX = -((FREQ_BAR_COUNT * (barWidth + gap)) / 2f) + (barWidth + gap) / 2f;

            for (int i = 0; i < FREQ_BAR_COUNT; i++)
            {
                var barGO = new GameObject($"FreqBar_{i}");
                barGO.transform.SetParent(vizGO.transform, false);
                var brt = barGO.AddComponent<RectTransform>();
                float x = startX + i * (barWidth + gap);
                brt.anchoredPosition = new Vector2(x, 0);
                brt.sizeDelta = new Vector2(barWidth, 8f); // initial height

                var barImg = barGO.AddComponent<Image>();
                barImg.color = new Color(0.4f, 0.85f, 0.95f, 0.95f);
                _freqBars[i] = barImg;
            }

            _freqLabel = CreateTMP(_panel.transform, "TARGET 432 Hz  •  PLAYER TUNE DRIVES THE RAILS", new Vector2(80, metricY - 58), 8, TextAlignmentOptions.Center, new Color(0.6f, 0.8f, 0.9f));

            // --- COMPANION STATUS (right side, physical tells + trust forks) ---
            var compLabel = CreateTMP(_panel.transform, "COMPANIONS ON THE TRAIN", new Vector2(380, metricY), 9, TextAlignmentOptions.Left, new Color(0.92f, 0.82f, 0.95f));

            _companionLirael = CreateTMP(_panel.transform, "♪ Lirael  roof-singer  •  voice lifts with your tune", new Vector2(380, metricY - 16), 9, TextAlignmentOptions.Left, new Color(0.98f, 0.75f, 0.88f));
            _companionMilo = CreateTMP(_panel.transform, "🛡 Milo  rear-guard  •  braces the vulnerable", new Vector2(380, metricY - 30), 9, TextAlignmentOptions.Left, new Color(0.72f, 0.88f, 0.65f));
            _companionCassian = CreateTMP(_panel.transform, "✧ Cassian  mid-car  •  steadies the rails", new Vector2(380, metricY - 44), 9, TextAlignmentOptions.Left, new Color(0.65f, 0.82f, 0.95f));

            // Widen right-side companion rects so text doesn't clip
            if (_companionLirael) _companionLirael.rectTransform.sizeDelta = new Vector2(340, 20);
            if (_companionMilo) _companionMilo.rectTransform.sizeDelta = new Vector2(340, 20);
            if (_companionCassian) _companionCassian.rectTransform.sizeDelta = new Vector2(340, 20);

            // === BOTTOM STATUS / EVENT BANNER + BRANCH PROMPT ===
            _statusBanner = CreateTMP(_panel.transform, "", new Vector2(0, -152), 11, TextAlignmentOptions.Center, new Color(1f, 0.88f, 0.4f));
            _statusBanner.fontStyle = FontStyles.Bold;

            // Dedicated Branch Choice Prompt (appears at WindspireJunction)
            CreateBranchPromptPanel();

            // Subscribe to escort events
            if (_escort != null && !_subscribed)
            {
                _escort.OnWaveStarted += OnWaveUpdate;
                _escort.OnSeventeenthHourTriggered += On17thHour;
                _escort.OnLeviathanPurified += OnLeviPurified;
                _escort.OnEscortComplete += OnComplete;
                _escort.OnBranchChoiceDecided += OnBranchDecision;
                _subscribed = true;
            }
        }

        void CreateGoldTrim(Transform parent, float w, float h)
        {
            // Simple 4-side gold border using thin images (Tartarian elegance, zero sprites)
            float t = 2.2f; // trim thickness
            Color gold = new Color(0.92f, 0.82f, 0.48f, 0.95f);

            // Top
            var top = new GameObject("TrimTop");
            top.transform.SetParent(parent, false);
            var trt = top.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1); trt.pivot = new Vector2(0.5f, 1);
            trt.sizeDelta = new Vector2(0, t); trt.anchoredPosition = Vector2.zero;
            top.AddComponent<Image>().color = gold;

            // Bottom
            var bot = new GameObject("TrimBot");
            bot.transform.SetParent(parent, false);
            var brt = bot.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 0); brt.anchorMax = new Vector2(1, 0); brt.pivot = new Vector2(0.5f, 0);
            brt.sizeDelta = new Vector2(0, t); brt.anchoredPosition = Vector2.zero;
            bot.AddComponent<Image>().color = gold;

            // Left
            var lft = new GameObject("TrimLeft");
            lft.transform.SetParent(parent, false);
            var lrt = lft.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(0, 1); lrt.pivot = new Vector2(0, 0.5f);
            lrt.sizeDelta = new Vector2(t, 0); lrt.anchoredPosition = Vector2.zero;
            lft.AddComponent<Image>().color = gold;

            // Right
            var rgt = new GameObject("TrimRight");
            rgt.transform.SetParent(parent, false);
            var rrt = rgt.AddComponent<RectTransform>();
            rrt.anchorMin = new Vector2(1, 0); rrt.anchorMax = new Vector2(1, 1); rrt.pivot = new Vector2(1, 0.5f);
            rrt.sizeDelta = new Vector2(t, 0); rrt.anchoredPosition = Vector2.zero;
            rgt.AddComponent<Image>().color = gold;
        }

        void CreateBranchPromptPanel()
        {
            _branchPromptPanel = new GameObject("BranchPrompt");
            _branchPromptPanel.transform.SetParent(_panel.transform, false);
            var rt = _branchPromptPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, -168); // near bottom of the tall HUD panel for dramatic junction reveal
            rt.sizeDelta = new Vector2(680, 48);

            var bg = _branchPromptPanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.03f, 0.96f);
            // Gold border trim for the prompt
            CreateGoldTrim(_branchPromptPanel.transform, 620, 52);

            _branchTitle = CreateTMP(_branchPromptPanel.transform, "⚡  WINDSPIRE JUNCTION  ⚡", new Vector2(0, 14), 14, TextAlignmentOptions.Center, new Color(0.98f, 0.85f, 0.4f));
            _branchTitle.fontStyle = FontStyles.Bold;

            _branchDesc = CreateTMP(_branchPromptPanel.transform, "Resonance chose the path...", new Vector2(0, -8), 11, TextAlignmentOptions.Center, Color.white);

            _branchPromptPanel.SetActive(false);
        }

        TextMeshProUGUI CreateTMP(Transform parent, string initial, Vector2 anchoredPos, float size, TextAlignmentOptions align, Color col)
        {
            var go = new GameObject("HUD_TMP");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(620, 26);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = initial;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = col;
            tmp.raycastTarget = false;
            // Enable rich text for symbols
            tmp.richText = true;
            return tmp;
        }

        void Update()
        {
            if (_escort == null || !_escort.IsActive || _canvas == null) return;

            float prog = _escort.Progress;
            float time = _escort.GetEscortTime();
            float trainN = _escort.TrainHealthNormalized;
            float shield = _escort.LullabyShieldStrength;
            float freqMatch = _escort.LastFreqMatch;
            int adopted = SpectralOrphanAdoption.AdoptedCount;
            int wave = _escort.CurrentWave;
            int threats = _escort.ActiveThreatCount;
            float targetHz = _escort.CurrentTargetHz;
            int branch = _escort.CurrentBranchChoice;

            // Lullaby rhythm phase from Moon3 audio heart (visual heartbeat for player)
            float rhythmPhase = 0f;
            int lullabyStreak = 0;
            if (Moon3RailAudioManager.Instance != null)
            {
                rhythmPhase = Moon3RailAudioManager.Instance.GetCurrentLullabyBeatPhase();
                lullabyStreak = Moon3RailAudioManager.Instance.GetLullabyStreak();
            }

            // === PROGRESS + TIMER (beautiful MM:SS) ===
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int totalMin = 11;
            int totalSec = 0;
            _timeText.text = $"{minutes:00}:{seconds:00} / {totalMin:00}:{totalSec:00}";

            // Progress label + bar (clamped elegant fill)
            if (_progressLabel) _progressLabel.text = $"ESCORT PROGRESS  —  {prog * 100f:F0}%";
            if (_progressBarFill != null)
            {
                float fillW = 860f * Mathf.Clamp01(prog);
                _progressBarFill.sizeDelta = new Vector2(fillW, 16);
                // Subtle gold pulse when near end or high shield
                var c = (prog > 0.85f || shield > 2.8f) ? Color.Lerp(new Color(0.55f, 0.88f, 0.72f), new Color(0.96f, 0.88f, 0.4f), Mathf.PingPong(Time.time * 2.2f, 1f)) : new Color(0.55f, 0.88f, 0.72f, 1f);
                _progressBarFill.GetComponent<Image>().color = c;
            }

            // === TRAIN HEALTH BAR ===
            if (_trainHealthBarFill != null)
            {
                float hw = 210f * Mathf.Clamp01(trainN);
                _trainHealthBarFill.sizeDelta = new Vector2(hw, 11);
                // Critical flash
                var hImg = _trainHealthBarFill.GetComponent<Image>();
                hImg.color = (trainN < 0.25f) ? Color.Lerp(new Color(0.92f, 0.55f, 0.35f), Color.red, Mathf.PingPong(Time.time * 6f, 1f)) : new Color(0.92f, 0.55f, 0.35f, 1f);
            }

            // === LULLABY SHIELD + ORPHANS ===
            if (_shieldBarFill != null)
            {
                float sw = 210f * Mathf.Clamp01((shield - 0.6f) / 3.2f); // normalized visual range
                _shieldBarFill.sizeDelta = new Vector2(sw, 11);
            }
            if (_shieldText)
            {
                _shieldText.text = $"{shield:F2}×  •  {adopted} ORPHANS  {(shield > 2.5f ? "— CHILDREN SING LOUDER" : "")}";
                _shieldText.color = (shield > 2.8f) ? Color.Lerp(new Color(1f, 0.92f, 0.5f), Color.yellow, Mathf.PingPong(Time.time * 3.5f, 1f)) : new Color(1f, 0.92f, 0.5f);
            }

            // === FREQUENCY RHYTHM MATCHER VISUALIZER (live animated bars) ===
            UpdateFrequencyVisualizer(freqMatch, targetHz);

            if (_matchPercentText)
            {
                string rhythm = (rhythmPhase > 0.01f) ? $"  |  BEAT {rhythmPhase * 100f:F0}%" : "";
                string streakStr = (lullabyStreak > 0) ? $"  x{lullabyStreak}" : "";
                _matchPercentText.text = $"MATCH  {freqMatch * 100f:F0}%{streakStr}{rhythm}";
            }
            if (_freqLabel)
                _freqLabel.text = $"TARGET {targetHz:F0} Hz  •  YOUR TUNE SHAPES WAVES & SHIELD";

            // === COMPANION PHYSICAL TELLS + DYNAMIC FORKS (Tartarian emotional feedback) ===
            UpdateCompanionStatus(freqMatch, shield, branch);

            // === WAVE / THREAT (header) ===
            if (_waveText) _waveText.text = $"WAVE  {wave}/10";
            if (_threatText)
            {
                _threatText.text = $"THREATS: {threats:00}";
                _threatText.color = threats > 5 ? new Color(0.98f, 0.45f, 0.35f) : new Color(0.95f, 0.82f, 0.35f);
            }

            // === DYNAMIC STATUS BANNER (special Moon 3 moments) ===
            UpdateStatusBanner();

            // === BRANCH PROMPT TIMEOUT (auto hide) ===
            if (_branchPromptPanel && _branchPromptPanel.activeSelf)
            {
                _branchPromptTimer -= Time.deltaTime;
                if (_branchPromptTimer <= 0f)
                {
                    _branchPromptPanel.SetActive(false);
                }
            }

            // Junction crossing detection for branch prompt (robust even if event timing)
            if (!_branchPromptShown && prog >= 0.465f && prog <= 0.51f && branch >= 0)
            {
                ShowBranchPrompt(branch);
            }

            _lastProgressForJunction = prog;
        }

        void UpdateFrequencyVisualizer(float match, float targetHz)
        {
            if (_freqBars == null || _freqVizContainer == null) return;

            float t = Time.time;
            float coherence = 0.35f + match * 0.75f; // high match = beautiful coherent wave
            float amp = 14f + match * 22f;
            float baseH = 6f;

            for (int i = 0; i < _freqBars.Length; i++)
            {
                if (_freqBars[i] == null) continue;
                float phase = i * 0.85f + t * (3.8f + match * 1.6f);
                float wave = Mathf.Sin(phase) * 0.5f + 0.5f;

                // Add gentle second harmonic for "living lullaby" feel
                wave += Mathf.Sin(phase * 2.3f + 1.2f) * 0.22f * coherence;

                float h = baseH + wave * amp * coherence;

                // Slight jitter when match is poor (dissonance)
                if (match < 0.45f)
                    h += (Mathf.PerlinNoise(i * 0.7f, t * 2.2f) - 0.5f) * (28f * (1f - match));

                var rt = _freqBars[i].rectTransform;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Clamp(h, 4f, 36f));

                // Color shift: cool cyan (low) → warm gold (high resonance)
                float c = Mathf.Clamp01(match + (wave - 0.5f) * 0.3f);
                _freqBars[i].color = Color.Lerp(
                    new Color(0.35f, 0.72f, 0.92f, 0.85f),
                    new Color(0.96f, 0.82f, 0.38f, 0.98f),
                    c * 0.85f + 0.15f);
            }
        }

        void UpdateCompanionStatus(float freq, float shield, int branchChoice)
        {
            // Lirael — freq success singer (roof lean physical tell)
            if (_companionLirael)
            {
                string l = "♪ Lirael  roof-singer";
                if (freq > 0.72f) l += "  — voice soars, wind answers";
                else if (freq > 0.5f) l += "  — leaning into the melody";
                else l += "  — holding the note";
                _companionLirael.text = l;
                _companionLirael.color = (freq > 0.72f) ? new Color(1f, 0.82f, 0.92f) : new Color(0.98f, 0.75f, 0.88f);
            }

            // Milo — protection / guard (rear brace)
            if (_companionMilo)
            {
                string m = "🛡 Milo  rear-guard";
                if (shield > 2.4f) m += "  — unbreakable stance";
                else if (shield > 1.6f) m += "  — bracing hard for the children";
                else m += "  — vigilant, ready";
                _companionMilo.text = m;
                _companionMilo.color = (shield > 2.4f) ? new Color(0.82f, 0.96f, 0.72f) : new Color(0.72f, 0.88f, 0.65f);
            }

            // Cassian — physical tells, mid support, redemption hints
            if (_companionCassian)
            {
                string c = "✧ Cassian  mid-car";
                if (branchChoice == 1) c += "  — rails steady under his watch";
                else if (freq > 0.65f) c += "  — quiet strength, rails remember";
                else c += "  — steady hand on the journey";
                _companionCassian.text = c;
                _companionCassian.color = new Color(0.68f, 0.85f, 0.98f);
            }
        }

        void UpdateStatusBanner()
        {
            if (_statusBanner == null) return;

            string txt = "";
            Color col = new Color(1f, 0.88f, 0.4f);

            if (_escort.IsSeventeenthHourActive)
            {
                txt = "★  THE 17TH HOUR  —  THE RAILS REMEMBER THE CHILDREN'S SONG  ★";
                col = new Color(1f, 0.95f, 0.65f);
            }
            else if (_escort.IsLeviathanPhaseActive)
            {
                int phase = _escort.LeviathanPhase;
                txt = phase switch
                {
                    1 => "LEVIATHAN  —  TAIL SWEEP  •  MATCH TO SURVIVE",
                    2 => "LEVIATHAN  —  SONIC SCREAM  •  LULLABY IS YOUR SHIELD",
                    3 => "LEVIATHAN  —  CRYSTAL BARRAGE  •  ORPHANS OPEN THE HEART",
                    4 => "LEVIATHAN  —  VULNERABLE  •  GIVE EVERYTHING TO THE LULLABY",
                    _ => "LEVIATHAN AWAKENS — TUNE THE CHILDREN'S SONG"
                };
                col = new Color(0.98f, 0.45f, 0.55f);
            }
            else if (_escort.IsPermanentWorldChanged)
            {
                txt = "✦  GIANT ECHO FREED  •  GOLDEN RAILS  •  WINDS CALMED  •  WORLD REMEMBERS  ✦";
                col = new Color(0.95f, 0.88f, 0.45f);
            }

            _statusBanner.text = txt;
            _statusBanner.color = col;
        }

        public void ShowBranchPrompt(int choice)
        {
            if (_branchPromptPanel == null || _branchPromptShown) return;

            _branchPromptPanel.SetActive(true);
            _branchPromptTimer = 7.5f; // nice readable duration
            _branchPromptShown = true;

            if (choice == 1)
            {
                // Safe / Lirael path (high freq success)
                _branchTitle.text = "⚡  WINDSPIRE JUNCTION  —  COMPASSIONATE PATH  ⚡";
                _branchTitle.color = new Color(0.65f, 0.95f, 0.75f);
                _branchDesc.text = "Your resonance chose the tuned rails • Lirael’s voice empowered • Lighter threats ahead • Found family grows";
                _branchDesc.color = new Color(0.85f, 0.96f, 0.82f);
            }
            else
            {
                // Combat / Milo path
                _branchTitle.text = "⚡  WINDSPIRE JUNCTION  —  PROTECTIVE GAUNTLET  ⚡";
                _branchTitle.color = new Color(0.98f, 0.72f, 0.55f);
                _branchDesc.text = "Resonance demanded courage • Milo stands unbreakable • Extra threats, but deeper trust • The children are safe in your shield";
                _branchDesc.color = new Color(0.98f, 0.85f, 0.72f);
            }

            // Nice pulse on show
            StartCoroutine(PulseBranchPrompt());
        }

        IEnumerator PulseBranchPrompt()
        {
            if (_branchPromptPanel == null) yield break;
            var cg = _branchPromptPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = _branchPromptPanel.AddComponent<CanvasGroup>();

            float t = 0f;
            while (t < 1f && _branchPromptPanel.activeSelf)
            {
                t += Time.deltaTime * 3f;
                cg.alpha = Mathf.Lerp(0.4f, 1f, Mathf.PingPong(t, 1f));
                yield return null;
            }
            if (cg) cg.alpha = 1f;
        }

        void OnWaveUpdate(int wave)
        {
            if (_statusBanner) _statusBanner.text = $"WAVE {wave} ESCALATION — TUNE THE LULLABY OR STAND WITH THE GUARD?";
            // Brief flash color pulse on wave
            if (_waveText) StartCoroutine(FlashText(_waveText, new Color(1f, 0.6f, 0.3f), 0.6f));
        }

        void On17thHour()
        {
            if (_statusBanner) _statusBanner.text = "★  THE 17TH HOUR  —  CHILDREN SING THE HIDDEN SUN  •  SHIELD SPIKE  ★";
            if (_statusBanner) StartCoroutine(FlashText(_statusBanner, Color.white, 1.2f));
        }

        void OnLeviPurified()
        {
            if (_statusBanner) _statusBanner.text = "✦  LEVIATHAN PURIFIED BY LULLABY  —  GIANT ECHO RELEASED  •  PERMANENT VICTORY  ✦";
            if (_statusBanner) StartCoroutine(FlashText(_statusBanner, new Color(0.95f, 0.9f, 0.5f), 2f));
        }

        void OnComplete(bool success)
        {
            if (success && _statusBanner)
            {
                _statusBanner.text = "✦  ESCORT VICTORY  —  THE RAIL NETWORK AWAKENS  •  WORLD'S FAIR TICKET + CONTINENTAL FAST TRAVEL  ✦";
                _statusBanner.color = new Color(0.85f, 0.95f, 0.6f);
            }
            Shutdown();
        }

        void OnBranchDecision(int choice)
        {
            // Event-driven immediate beautiful prompt (preferred path)
            ShowBranchPrompt(choice);
        }

        IEnumerator FlashText(TextMeshProUGUI tmp, Color flashColor, float duration)
        {
            if (tmp == null) yield break;
            Color original = tmp.color;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.PingPong(t * 4f, 1f);
                tmp.color = Color.Lerp(original, flashColor, a);
                yield return null;
            }
            if (tmp) tmp.color = original;
        }

        public void Shutdown()
        {
            if (_escort != null && _subscribed)
            {
                _escort.OnWaveStarted -= OnWaveUpdate;
                _escort.OnSeventeenthHourTriggered -= On17thHour;
                _escort.OnLeviathanPurified -= OnLeviPurified;
                _escort.OnEscortComplete -= OnComplete;
                _escort.OnBranchChoiceDecided -= OnBranchDecision;
                _subscribed = false;
            }

            if (_canvas != null) Destroy(_canvas.gameObject);
            _escort = null;
            _freqBars = null;
            Debug.Log("[Moon3 R7 HUD] Escort HUD shutdown cleanly.");
        }

        void OnDestroy()
        {
            Shutdown();
        }
    }
}
