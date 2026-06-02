// CymaticMusicEngine.cs
// ─────────────────────────────────────────────────────────────────────────────
// Procedural three-band cymatic drone bed for TARTARIA Moon 1.
//
// Each restored hero building activates one Aether band (per CLAUDE.md):
//   - Telluric  (7.83 Hz) : earth      — spire / cathedral
//   - Harmonic  (432  Hz) : water      — fountain
//   - Celestial (528  Hz) : light      — dome / stardome
//
// Capstone: when Moon 1 (moonIndex == 1) completes, all three bands ramp to
// full volume and 2nd-order overtones (2x carrier) are added per band.
//
// IMPORTANT — per docs/agents/API_CONTRACT.md:
//   - GameEvents.OnBuildingRestored : Action<string>  (legacy, buildingId)
//   - GameEvents.OnMoonCompleted    : Action<MoonCompletedEventArgs>
//
// Per CLAUDE.md no-debt mandate:
//   - No silent catches: every catch logs file:line + the value that broke.
//   - No silent fallbacks: layer activation logs id, target volume, mix state.
//   - No stubs: all method bodies do real work.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Audio
{
    [DisallowMultipleComponent]
    public class CymaticMusicEngine : MonoBehaviour
    {
        // ── Aether band frequencies (canonical per CLAUDE.md 2026-05-29) ─────
        public const float TELLURIC_HZ  = 7.83f;   // Earth — Schumann resonance
        public const float HARMONIC_HZ  = 432f;    // Water — Verdi tuning
        public const float CELESTIAL_HZ = 528f;    // Light — solfeggio "love"

        // ── Mix targets (single-layer "restored" state) ──────────────────────
        private const float SINGLE_LAYER_TARGET_VOL = 0.4f;
        // ── Capstone target (full Moon 1 completion) ─────────────────────────
        private const float CAPSTONE_CARRIER_VOL   = 0.55f;
        private const float CAPSTONE_OVERTONE_VOL  = 0.22f;
        private const float DEFAULT_FADE_SECONDS   = 3.0f;
        private const float CAPSTONE_FADE_SECONDS  = 5.5f;

        // ── Procedural clip configuration ────────────────────────────────────
        private const int   SAMPLE_RATE = 48000;
        private const int   CHANNELS    = 1;
        // 10 seconds of looping sine; AudioClip.Create stream callback will be
        // invoked repeatedly so duration only sets the loop window. Keep it
        // long enough that the clip's internal buffer doesn't thrash.
        private const int   LOOP_SAMPLES = SAMPLE_RATE * 10;

        // ── Per-band runtime state ───────────────────────────────────────────
        private class Band
        {
            public string id;
            public float carrierHz;
            public AudioSource carrierSource;
            public AudioSource overtoneSource;     // 2x carrier, silent until capstone
            public double carrierPhase;
            public double overtonePhase;
            public bool   active;                  // restored?
            public float  currentVolume;           // mirrors carrierSource.volume (read-back)
            public float  currentOvertoneVolume;
        }

        private Band _telluric;
        private Band _harmonic;
        private Band _celestial;

        private static CymaticMusicEngine _instance;
        public static CymaticMusicEngine Instance => _instance;

        // ─────────────────────────────────────────────────────────────────────
        // Self-bootstrap so a scene without an authored CymaticMusicEngine
        // GameObject still gets a working instance.
        // ─────────────────────────────────────────────────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var existing = UnityEngine.Object.FindFirstObjectByType<CymaticMusicEngine>(
                FindObjectsInactive.Include);
            if (existing != null)
            {
                Debug.Log("[CymaticMusicEngine] Bootstrap: instance already in scene, reusing.");
                return;
            }
            var go = new GameObject("CymaticMusicEngine (auto)");
            go.AddComponent<CymaticMusicEngine>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.Log("[CymaticMusicEngine] Bootstrap: auto-created instance in DontDestroyOnLoad.");
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[CymaticMusicEngine] Duplicate instance on '{name}' destroyed; '{_instance.name}' is canonical.");
                Destroy(this);
                return;
            }
            _instance = this;

            _telluric  = BuildBand("Telluric",  TELLURIC_HZ);
            _harmonic  = BuildBand("Harmonic",  HARMONIC_HZ);
            _celestial = BuildBand("Celestial", CELESTIAL_HZ);

            Debug.Log($"[CymaticMusicEngine] Initialized 3 bands — Telluric {TELLURIC_HZ}Hz, Harmonic {HARMONIC_HZ}Hz, Celestial {CELESTIAL_HZ}Hz. All sources muted (vol=0).");
        }

        private void OnEnable()
        {
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            GameEvents.OnMoonCompleted    += HandleMoonCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            GameEvents.OnMoonCompleted    -= HandleMoonCompleted;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Band construction
        // ─────────────────────────────────────────────────────────────────────
        private Band BuildBand(string id, float carrierHz)
        {
            var band = new Band
            {
                id = id,
                carrierHz = carrierHz,
                carrierPhase = 0.0,
                overtonePhase = 0.0,
                active = false,
                currentVolume = 0f,
                currentOvertoneVolume = 0f
            };

            var carrierGO = new GameObject($"Band_{id}_Carrier");
            carrierGO.transform.SetParent(transform, false);
            band.carrierSource = carrierGO.AddComponent<AudioSource>();
            band.carrierSource.clip = CreateBandClip(id + "_Carrier",
                () => band.carrierHz,
                () => band.carrierPhase,
                phase => band.carrierPhase = phase);
            band.carrierSource.loop          = true;
            band.carrierSource.playOnAwake   = false;
            band.carrierSource.spatialBlend  = 0f;   // 2D drone
            band.carrierSource.volume        = 0f;
            band.carrierSource.Play();

            var overtoneGO = new GameObject($"Band_{id}_Overtone");
            overtoneGO.transform.SetParent(transform, false);
            band.overtoneSource = overtoneGO.AddComponent<AudioSource>();
            band.overtoneSource.clip = CreateBandClip(id + "_Overtone",
                () => band.carrierHz * 2f,
                () => band.overtonePhase,
                phase => band.overtonePhase = phase);
            band.overtoneSource.loop         = true;
            band.overtoneSource.playOnAwake  = false;
            band.overtoneSource.spatialBlend = 0f;
            band.overtoneSource.volume       = 0f;
            band.overtoneSource.Play();

            return band;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Procedural sine generator. AudioClip.Create's PCMReaderCallback
        // captures phase via closure refs so each AudioSource has independent
        // continuous phase across the loop boundary (avoids click on wrap).
        // ─────────────────────────────────────────────────────────────────────
        private static AudioClip CreateBandClip(
            string clipName,
            Func<float> freqHz,
            Func<double> readPhase,
            Action<double> writePhase)
        {
            AudioClip.PCMReaderCallback reader = (float[] data) =>
            {
                try
                {
                    double phase = readPhase();
                    float hz     = freqHz();
                    double step  = 2.0 * Math.PI * hz / SAMPLE_RATE;
                    for (int i = 0; i < data.Length; i += CHANNELS)
                    {
                        float sample = (float)Math.Sin(phase);
                        for (int c = 0; c < CHANNELS; c++)
                        {
                            data[i + c] = sample;
                        }
                        phase += step;
                        if (phase > 2.0 * Math.PI) phase -= 2.0 * Math.PI;
                    }
                    writePhase(phase);
                }
                catch (Exception ex)
                {
                    // No silent catch — rule 3 of no-debt mandate.
                    Debug.LogError($"[CymaticMusicEngine] PCMReaderCallback failed for clip '{clipName}' freqHz={freqHz()} bufferLen={data?.Length ?? -1}: {ex}");
                    throw;
                }
            };

            return AudioClip.Create(
                name: clipName,
                lengthSamples: LOOP_SAMPLES,
                channels: CHANNELS,
                frequency: SAMPLE_RATE,
                stream: true,
                pcmreadercallback: reader);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event handlers
        // ─────────────────────────────────────────────────────────────────────
        private void HandleBuildingRestored(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                Debug.LogWarning("[CymaticMusicEngine] OnBuildingRestored fired with null/empty buildingId — ignoring (no band can be matched).");
                return;
            }

            string lowered = buildingId.ToLowerInvariant();
            Band target = MatchBand(lowered);

            if (target == null)
            {
                Debug.Log($"[CymaticMusicEngine] OnBuildingRestored('{buildingId}') — no band matches substring rules (fountain|dome|stardome|spire|cathedral). Mix unchanged: {DescribeMix()}.");
                return;
            }

            if (target.active)
            {
                Debug.Log($"[CymaticMusicEngine] OnBuildingRestored('{buildingId}') — {target.id} band already active at vol={target.carrierSource.volume:F2}. No re-fade. Mix: {DescribeMix()}.");
                return;
            }

            target.active = true;
            Debug.Log($"[CymaticMusicEngine] LAYER-ACTIVATE id='{target.id}' carrierHz={target.carrierHz} targetVol={SINGLE_LAYER_TARGET_VOL:F2} trigger='{buildingId}' mixBefore={DescribeMix()}");
            StartCoroutine(FadeSource(target.carrierSource, SINGLE_LAYER_TARGET_VOL, DEFAULT_FADE_SECONDS,
                v => target.currentVolume = v,
                () =>
                {
                    Debug.Log($"[CymaticMusicEngine] LAYER-ACTIVE-COMPLETE id='{target.id}' atVol={target.carrierSource.volume:F2} mixAfter={DescribeMix()}");
                }));
        }

        private void HandleMoonCompleted(MoonCompletedEventArgs args)
        {
            if (args == null)
            {
                Debug.LogWarning("[CymaticMusicEngine] OnMoonCompleted fired with null args — ignoring.");
                return;
            }

            if (args.moonIndex != 1)
            {
                Debug.Log($"[CymaticMusicEngine] OnMoonCompleted(moonIndex={args.moonIndex}, name='{args.moonName}') — not Moon 1 capstone, mix unchanged: {DescribeMix()}.");
                return;
            }

            Debug.Log($"[CymaticMusicEngine] CAPSTONE-BEGIN Moon 1 '{args.moonName}' — ramping all 3 carriers to {CAPSTONE_CARRIER_VOL:F2} + adding 2nd-order overtones @ {CAPSTONE_OVERTONE_VOL:F2} over {CAPSTONE_FADE_SECONDS}s. mixBefore={DescribeMix()}");

            CapstoneBand(_telluric);
            CapstoneBand(_harmonic);
            CapstoneBand(_celestial);
        }

        private void CapstoneBand(Band band)
        {
            band.active = true;
            StartCoroutine(FadeSource(band.carrierSource, CAPSTONE_CARRIER_VOL, CAPSTONE_FADE_SECONDS,
                v => band.currentVolume = v,
                () => Debug.Log($"[CymaticMusicEngine] CAPSTONE carrier ramp complete id='{band.id}' vol={band.carrierSource.volume:F2}")));
            StartCoroutine(FadeSource(band.overtoneSource, CAPSTONE_OVERTONE_VOL, CAPSTONE_FADE_SECONDS,
                v => band.currentOvertoneVolume = v,
                () => Debug.Log($"[CymaticMusicEngine] CAPSTONE overtone ramp complete id='{band.id}' vol={band.overtoneSource.volume:F2} hz={band.carrierHz * 2f}")));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Substring → band routing per spec.
        //   "fountain"            → Harmonic   (water, 432 Hz)
        //   "dome" / "stardome"   → Celestial  (light, 528 Hz)
        //   "spire" / "cathedral" → Telluric   (earth, 7.83 Hz)
        // ─────────────────────────────────────────────────────────────────────
        private Band MatchBand(string loweredId)
        {
            if (loweredId.Contains("fountain"))                                  return _harmonic;
            if (loweredId.Contains("stardome") || loweredId.Contains("dome"))    return _celestial;
            if (loweredId.Contains("spire") || loweredId.Contains("cathedral"))  return _telluric;
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Linear volume fade coroutine. Logs start + end with value attached.
        // ─────────────────────────────────────────────────────────────────────
        private IEnumerator FadeSource(AudioSource src, float target, float seconds, Action<float> onTick, Action onDone)
        {
            if (src == null)
            {
                Debug.LogError($"[CymaticMusicEngine] FadeSource called with null AudioSource (target={target}, seconds={seconds}). Aborting fade.");
                yield break;
            }

            float start = src.volume;
            float elapsed = 0f;
            if (seconds <= 0.01f)
            {
                src.volume = target;
                onTick?.Invoke(target);
                onDone?.Invoke();
                yield break;
            }

            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                float v = Mathf.Lerp(start, target, t);
                src.volume = v;
                onTick?.Invoke(v);
                yield return null;
            }
            src.volume = target;
            onTick?.Invoke(target);
            onDone?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Diagnostic snapshot of current mix — used in log lines so each
        // layer activation tells you the full state (no silent fallbacks).
        // ─────────────────────────────────────────────────────────────────────
        private string DescribeMix()
        {
            return $"[T={_telluric.carrierSource.volume:F2}/o{_telluric.overtoneSource.volume:F2}|H={_harmonic.carrierSource.volume:F2}/o{_harmonic.overtoneSource.volume:F2}|C={_celestial.carrierSource.volume:F2}/o{_celestial.overtoneSource.volume:F2}]";
        }

        // ─────────────────────────────────────────────────────────────────────
        // Test/QA hooks — used by editor menus + future integration tests.
        // ─────────────────────────────────────────────────────────────────────
        public void DebugActivateTelluric()  => HandleBuildingRestored("DebugCathedral");
        public void DebugActivateHarmonic()  => HandleBuildingRestored("DebugFountain");
        public void DebugActivateCelestial() => HandleBuildingRestored("DebugStarDome");
        public void DebugCapstone()          => HandleMoonCompleted(new MoonCompletedEventArgs
        {
            moonIndex = 1,
            moonName  = "Moon 1 (debug)",
            rsReward  = 0,
            completionTime = 0f
        });

        public string PublicMixSnapshot() => DescribeMix();
    }
}
