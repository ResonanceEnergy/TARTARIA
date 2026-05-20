using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tartaria.Gameplay
{
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

        Text _titleText;
        Text _progressText;
        Text _shieldText;
        Text _freqText;
        Text _companionText;
        Text _waveTimerText;
        Text _statusText; // 17th / levi / branch etc.

        RectTransform _progressBarBg;
        RectTransform _progressBarFill;

        public void Initialize(RailEscortController escort)
        {
            _escort = escort;
            CreateRuntimeCanvasHUD();
            Debug.Log("[Moon3 R7 HUD] Dedicated escort HUD initialized (non-OnGUI Canvas).");
        }

        void CreateRuntimeCanvasHUD()
        {
            // Canvas at runtime — top-center overlay for escort only
            var canvasGO = new GameObject("Moon3_EscortHUD_Canvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 500; // above most

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();

            // Main panel (semi-transparent box at top)
            _panel = new GameObject("EscortPanel");
            _panel.transform.SetParent(canvasGO.transform, false);
            var rect = _panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -12);
            rect.sizeDelta = new Vector2(980, 142);

            var img = _panel.AddComponent<Image>();
            img.color = new Color(0.06f, 0.08f, 0.14f, 0.82f);
            img.raycastTarget = false;

            // Title
            _titleText = CreateText(_panel.transform, "ORPHAN TRAIN ESCORT — WINDSWEPT HIGHLANDS (MOON 3 R7)", new Vector2(0, -18), 18, TextAnchor.MiddleCenter, Color.cyan);

            // Progress row
            _progressText = CreateText(_panel.transform, "PROGRESS: 00% | TIME: 000s / 420s | TRAIN: 100%", new Vector2(-320, -48), 14, TextAnchor.MiddleLeft, Color.white);

            // Bar background
            var barBgGO = new GameObject("ProgressBg");
            barBgGO.transform.SetParent(_panel.transform, false);
            _progressBarBg = barBgGO.AddComponent<RectTransform>();
            _progressBarBg.anchoredPosition = new Vector2(0, -68);
            _progressBarBg.sizeDelta = new Vector2(720, 14);
            var bgImg = barBgGO.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.25f, 0.3f, 0.9f);

            // Fill
            var fillGO = new GameObject("ProgressFill");
            fillGO.transform.SetParent(barBgGO.transform, false);
            _progressBarFill = fillGO.AddComponent<RectTransform>();
            _progressBarFill.anchorMin = new Vector2(0, 0);
            _progressBarFill.anchorMax = new Vector2(0, 1);
            _progressBarFill.pivot = new Vector2(0, 0.5f);
            _progressBarFill.sizeDelta = new Vector2(0, 0);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0.4f, 0.95f, 0.7f, 1f);

            // Shield / Lullaby
            _shieldText = CreateText(_panel.transform, "LULLABY SHIELD: 1.00x | ADOPTED ORPHANS: 0", new Vector2(-280, -90), 13, TextAnchor.MiddleLeft, new Color(1f, 0.9f, 0.4f));

            // Frequency
            _freqText = CreateText(_panel.transform, "FREQ MATCH: 00% | TARGET: 432 Hz | LIVE Hz: 432", new Vector2(280, -90), 13, TextAnchor.MiddleLeft, new Color(0.6f, 0.95f, 1f));

            // Companion status (physical tells + trust fork indicators)
            _companionText = CreateText(_panel.transform, "COMPANIONS: Lirael (roof, singing) | Milo (rear, guard) | Cassian (mid)  Trust: +0", new Vector2(0, -110), 12, TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.95f));

            // Wave / Timer
            _waveTimerText = CreateText(_panel.transform, "WAVE: 0/7 | NEXT WAVE: --s | THREATS: 0", new Vector2(-320, -128), 12, TextAnchor.MiddleLeft, Color.white);

            // Dynamic status (17th, Levi, Branch, Victory)
            _statusText = CreateText(_panel.transform, "", new Vector2(260, -128), 12, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.3f));

            // Subscribe to escort events for reactive updates
            if (_escort != null)
            {
                _escort.OnWaveStarted += OnWaveUpdate;
                _escort.OnSeventeenthHourTriggered += On17thHour;
                _escort.OnLeviathanPurified += OnLeviPurified;
                _escort.OnEscortComplete += OnComplete;
            }
        }

        Text CreateText(Transform parent, string initial, Vector2 pos, int size, TextAnchor align, Color col)
        {
            var go = new GameObject("HUDText");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(920, 22);
            var txt = go.AddComponent<Text>();
            txt.text = initial;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = size;
            txt.alignment = align;
            txt.color = col;
            txt.raycastTarget = false;
            return txt;
        }

        void Update()
        {
            if (_escort == null || !_escort.IsActive || _canvas == null) return;

            // Pull live state (R6 + R7 extended props via public accessors)
            float prog = _escort.Progress;
            float time = _escort.GetEscortTime();
            float trainN = _escort.TrainHealthNormalized;
            float shield = _escort.LullabyShieldStrength;
            float freq = _escort.LastFreqMatch;
            int adopted = SpectralOrphanAdoption.AdoptedCount;
            int wave = _escort.CurrentWave;
            int threats = _escort.ActiveThreatCount;
            float targetHz = _escort.CurrentTargetHz;

            _progressText.text = $"PROGRESS: {prog * 100f:F0}% | TIME: {time:F0}s / 420s | TRAIN: {trainN * 100f:F0}%";

            // Update progress bar fill
            if (_progressBarFill != null)
            {
                float w = 720f * Mathf.Clamp01(prog);
                _progressBarFill.sizeDelta = new Vector2(w, 14);
            }

            _shieldText.text = $"LULLABY SHIELD: {shield:F2}x | ADOPTED ORPHANS: {adopted}  (Lullaby power scales with children + freq success)";

            _freqText.text = $"FREQ MATCH: {freq:P0} | TARGET: {targetHz:F0} Hz | LIVE PLAYER Hz drives dynamic waves & shield";

            // Companion status with fork hints (R7 reactivity)
            string companionStatus = "Lirael: roof-singer lean (freq success ↑) | Milo: rear-brace (protection focus ↑) | Cassian: mid-support";
            if (freq > 0.75f) companionStatus += "  [FREQ FORK: Lirael trust high]";
            else if (shield > 2.2f) companionStatus += "  [PROTECTION FORK: Milo trust high]";
            _companionText.text = $"COMPANIONS: {companionStatus}";

            float nextWaveEst = Mathf.Max(0, 52f * (0.82f + (1f - freq) * 0.38f) - (time % 52f));
            _waveTimerText.text = $"WAVE: {wave}/7 | NEXT WAVE IN: {nextWaveEst:F0}s | ACTIVE THREATS: {threats}";

            // Status line for special states
            string status = "";
            if (_escort.IsSeventeenthHourActive) status += "★ 17TH HOUR ALIGNMENT — RAILS REMEMBER ★  ";
            if (_escort.IsLeviathanPhaseActive) status += "LEVIATHAN VULN WINDOWS OPEN — ORPHANS' LULLABY SYNERGY!  ";
            if (_escort.IsPermanentWorldChanged) status += "WORLD TRANSFORMED: GOLDEN RAILS + GIANT ECHO + CALMED WINDS";
            _statusText.text = status;

            // Color pulse on high shield/freq for polish
            if (shield > 2.5f) _shieldText.color = Color.Lerp(new Color(1f, 0.9f, 0.4f), Color.yellow, Mathf.PingPong(Time.time * 3f, 1f));
            else _shieldText.color = new Color(1f, 0.9f, 0.4f);
        }

        void OnWaveUpdate(int wave)
        {
            if (_statusText) _statusText.text = $"WAVE {wave} ESCALATION — TUNE OR PROTECT?";
        }

        void On17thHour()
        {
            if (_statusText) _statusText.text = "THE 17TH HOUR — CHILDREN SING THE HIDDEN SUN. SPECIAL SHIELD SPIKE!";
        }

        void OnLeviPurified()
        {
            if (_statusText) _statusText.text = "LEVIATHAN PURIFIED BY LULLABY — GIANT ECHO RELEASED. PERMANENT VICTORY!";
        }

        void OnComplete(bool success)
        {
            if (success)
            {
                if (_statusText) _statusText.text = "ESCORT VICTORY — RAIL NETWORK AWAKENS. WORLD'S FAIR TICKET + FAST TRAVEL HOOKS UNLOCKED.";
            }
            Shutdown();
        }

        public void Shutdown()
        {
            if (_canvas != null) Destroy(_canvas.gameObject);
            _escort = null;
            Debug.Log("[Moon3 R7 HUD] Escort HUD shutdown.");
        }

        void OnDestroy()
        {
            Shutdown();
        }
    }
}
