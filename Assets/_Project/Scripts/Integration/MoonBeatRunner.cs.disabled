using System.Collections;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// B1 — Moon Framework v2.
    /// Orchestrates the 5-beat structure shared across Moons 1–13:
    ///   1. Discovery     — new zone reveal
    ///   2. Restoration   — light architecture revival / setup
    ///   3. Conflict      — mechanic encounter (delegated to MoonMechanicActivator)
    ///   4. Climax        — peak set-piece (waits for Activator's clear signal)
    ///   5. Revelation    — lore drop + reward funnel
    ///
    /// Sits next to MoonRuntimeBootstrapper + MoonMechanicActivator on every
    /// moon scene root (attached by editor phase MoonFrameworkBinder).
    ///
    /// Additive — does NOT replace the existing activator. The activator still
    /// runs the actual gameplay; this runner supervises pacing, HUD banners,
    /// and per-beat persistence. Falls back to time-only beats if no activator.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonBeatRunner : MonoBehaviour
    {
        public enum Beat { Discovery = 0, Restoration = 1, Conflict = 2, Climax = 3, Revelation = 4 }

        public MoonDefinition definition;
        public bool autoStart = true;
        public float startDelay = 1.5f;

        public static event System.Action<int /*moon*/, Beat> OnBeatStarted;
        public static event System.Action<int /*moon*/, Beat> OnBeatCompleted;
        public static event System.Action<int /*moon*/>        OnAllBeatsCompleted;

        Beat _current = Beat.Discovery;
        bool _running;
        bool _moonClearedFromActivator;

        void OnEnable()
        {
            MoonProgressTracker.OnMoonCleared += HandleMoonClearedFromActivator;
        }

        void OnDisable()
        {
            MoonProgressTracker.OnMoonCleared -= HandleMoonClearedFromActivator;
        }

        void Start()
        {
            if (!autoStart) return;
            if (definition == null)
            {
                Debug.LogWarning("[MoonBeatRunner] No definition assigned on " + name);
                return;
            }
            StartCoroutine(RunSequence());
        }

        void HandleMoonClearedFromActivator(int moon)
        {
            if (definition == null || moon != definition.number) return;
            _moonClearedFromActivator = true;
        }

        public void SkipToBeat(Beat b)
        {
            _current = b;
            Debug.Log($"[MoonBeatRunner] Moon {definition?.number:D2} skip → {b}");
        }

        IEnumerator RunSequence()
        {
            if (_running) yield break;
            _running = true;
            yield return new WaitForSeconds(startDelay);

            // Resume from highest already-cleared beat for this moon.
            int resumeIndex = 0;
            for (int i = 0; i < 5; i++)
            {
                if (MoonProgressTracker.Instance != null && MoonProgressTracker.Instance.IsBeatCleared(definition.number, i))
                    resumeIndex = i + 1;
            }
            _current = (Beat)Mathf.Clamp(resumeIndex, 0, 4);

            for (int i = (int)_current; i < 5; i++)
            {
                _current = (Beat)i;
                yield return RunBeat(_current);
            }

            OnAllBeatsCompleted?.Invoke(definition.number);
            _running = false;
        }

        IEnumerator RunBeat(Beat b)
        {
            OnBeatStarted?.Invoke(definition.number, b);
            ShowBanner(b);

            float dur = SafeDuration(b);

            if (b == Beat.Climax)
            {
                // Wait for the existing MoonMechanicActivator to flag clear (via MoonProgressTracker),
                // bounded by a soft timeout so a missing activator can't stall the runner.
                float t = 0f;
                float timeout = Mathf.Max(dur, 90f);
                while (!_moonClearedFromActivator && t < timeout)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(dur);
            }

            MoonProgressTracker.Instance?.MarkBeatCleared(definition.number, (int)b);
            MoonRewardService.AwardBeat(definition, b);
            OnBeatCompleted?.Invoke(definition.number, b);
        }

        float SafeDuration(Beat b)
        {
            int idx = (int)b;
            if (definition?.beatDurations != null && idx < definition.beatDurations.Length && definition.beatDurations[idx] > 0f)
                return definition.beatDurations[idx];
            // Sensible defaults
            switch (b)
            {
                case Beat.Discovery:   return 4f;
                case Beat.Restoration: return 6f;
                case Beat.Conflict:    return 12f;
                case Beat.Climax:      return 8f;
                case Beat.Revelation:  return 6f;
                default:               return 5f;
            }
        }

        void ShowBanner(Beat b)
        {
            string subtitle = BeatSubtitle(b);
            string title = $"MOON {definition.number:D2}  —  {b.ToString().ToUpperInvariant()}";
            MoonHUDBanner.Show(title, subtitle, BeatTint(b));
            // Also push to existing HUDController objective slot for back-compat.
            GameEvents.RaiseHUDShowObjective($"<b>{title}</b>\n<size=70%>{subtitle}</size>");
        }

        string BeatSubtitle(Beat b)
        {
            int idx = (int)b;
            if (definition?.beatHeadlines != null && idx < definition.beatHeadlines.Length && !string.IsNullOrEmpty(definition.beatHeadlines[idx]))
                return definition.beatHeadlines[idx];
            switch (b)
            {
                case Beat.Discovery:   return $"Enter {definition.zoneName}.";
                case Beat.Restoration: return $"Restore the resonance.";
                case Beat.Conflict:    return $"Mechanic: {definition.mechanic}.";
                case Beat.Climax:      return $"Climax — hold the line.";
                case Beat.Revelation:  return $"Revelation — {definition.aetherWhisper}";
                default:               return string.Empty;
            }
        }

        Color BeatTint(Beat b)
        {
            switch (b)
            {
                case Beat.Discovery:   return new Color(0.55f, 0.85f, 1.00f, 1f); // cyan
                case Beat.Restoration: return new Color(0.95f, 0.78f, 0.30f, 1f); // gold
                case Beat.Conflict:    return new Color(1.00f, 0.45f, 0.35f, 1f); // crimson
                case Beat.Climax:      return new Color(0.95f, 0.30f, 0.85f, 1f); // magenta
                case Beat.Revelation:  return new Color(0.75f, 1.00f, 0.85f, 1f); // mint
                default:               return Color.white;
            }
        }
    }
}
