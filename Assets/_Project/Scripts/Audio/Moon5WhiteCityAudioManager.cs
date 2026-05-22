using UnityEngine;
using Tartaria.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// Moon 5 Overtone audio layer: amplification harmonics, 6-band healing tones,
    /// aurora fountain whooshes, Thorne radio static bursts, victory overtone motif.
    /// Hooks into the WhiteCityAmplificationController events (future expansion via events or direct calls).
    /// </summary>
    public class Moon5WhiteCityAudioManager : MonoBehaviour
    {
        public static Moon5WhiteCityAudioManager Instance { get; private set; }

        [Header("Overtone References (assign in scene or let bootstrap find)")]
        public AudioSource amplificationSource;
        public AudioSource fountainSource;
        public AudioSource bridgeSource;

        // ─── Runtime procedural sources for live tuning / resonance / radio ───
        private AudioSource _humSource;           // persistent district overtone drone, volume driven by resonance
        private AudioSource _tuningSource;        // live hold-E frequency match source (pitch + vol ramp for "tuning")
        private AudioClip _tuningHarmonicClip;    // rich 432+overtones base clip (procedural, reused)
        private float _currentResonance = 0.55f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // Ensure we have a child for 3D positioned sources if needed
            if (transform.Find("AudioSources") == null)
            {
                var srcRoot = new GameObject("AudioSources");
                srcRoot.transform.SetParent(transform);
            }
        }

        void Start()
        {
            EnsureHumSource();
            // Prime procedural lib so Moon5_* clips are ready (safe, idempotent)
            ProceduralSFXLibrary.Initialize();
        }

        void Update()
        {
            // Smooth hum to resonance (living city feel, cheap lerp)
            if (_humSource != null && _humSource.isPlaying)
            {
                float target = Mathf.Clamp01(_currentResonance) * 0.48f;
                _humSource.volume = Mathf.Lerp(_humSource.volume, target, Time.deltaTime * 2.2f);
                _humSource.pitch = 0.84f + _currentResonance * 0.16f;
            }
        }

        void EnsureHumSource()
        {
            if (_humSource != null) return;
            _humSource = gameObject.AddComponent<AudioSource>();
            _humSource.loop = true;
            _humSource.spatialBlend = 0f;
            _humSource.volume = 0f;
            _humSource.pitch = 0.85f;

            // Use Moon5 stinger as long harmonic bed (rich overtones) or fallback tone
            var clip = ProceduralSFXLibrary.Get("Moon5_AmplificationStinger");
            if (clip != null)
            {
                _humSource.clip = clip;
            }
            else
            {
                // Fallback: create a simple 432Hz harmonic drone clip inline (lightweight)
                _humSource.clip = CreateSimpleHarmonicClip(432f, 6.0f);
            }
        }

        AudioClip CreateSimpleHarmonicClip(float baseHz, float dur)
        {
            int sr = AudioSettings.outputSampleRate;
            int samples = Mathf.CeilToInt(sr * dur);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float s = Mathf.Sin(2f * Mathf.PI * baseHz * t) * 0.55f
                        + Mathf.Sin(2f * Mathf.PI * baseHz * 1.5f * t) * 0.32f
                        + Mathf.Sin(2f * Mathf.PI * baseHz * 2.03f * t) * 0.18f
                        + Mathf.Sin(2f * Mathf.PI * 528f * t) * 0.22f;
                data[i] = s * 0.7f;
            }
            var c = AudioClip.Create("Moon5_HarmonicDrone", samples, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }

        // ─── Public API (called by WhiteCityAmplificationController + interactor) ───

        public void PlayAmplificationStinger(int pavilion, float strength)
        {
            float vol = 0.55f + strength * 0.35f;
            var clip = ProceduralSFXLibrary.Get("Moon5_AmplificationStinger");
            if (clip != null)
            {
                AudioManager.Instance?.PlaySFX2D(clip, vol);
            }
            else
            {
                AudioManager.Instance?.PlaySFX2D("BuildingActive");
            }

            // Extra overtone layers via live tones (432 family + healing) for "singing pavilions"
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTone(432f, 0.75f, 0.22f * strength);
                if (strength > 0.6f)
                    AudioManager.Instance.PlayTone(528f, 0.9f, 0.16f);
                if (strength > 0.85f)
                    AudioManager.Instance.PlayTone(864f, 0.65f, 0.12f);
            }

            // Dynamic hum reacts immediately
            SetResonanceLevel(_currentResonance + 0.06f);
        }

        public void PlayHealingAuraTone(Vector3 pos)
        {
            var clip = ProceduralSFXLibrary.Get("Moon5_HealingAuraTone");
            if (clip != null)
            {
                AudioManager.Instance?.PlaySFX(clip, pos, 0.38f); // 3D positioned
            }
            else
            {
                AudioManager.Instance?.PlaySFX3D("Moon2_RestoreHarmonic", pos, 0.35f);
            }
            // Gentle extra 6-band shimmer layer
            AudioManager.Instance?.PlayTone(528f, 1.6f, 0.14f);
        }

        public void PlayAuroraFountainBurst(Vector3 pos)
        {
            var clip = ProceduralSFXLibrary.Get("Moon5_FountainWhoosh");
            if (clip != null)
            {
                AudioManager.Instance?.PlaySFX(clip, pos, 0.52f);
            }
            else
            {
                AudioManager.Instance?.PlaySFX3D("Moon2_FountainChime", pos, 0.45f);
            }
        }

        public void PlayBridgeIgnition()
        {
            var clip = ProceduralSFXLibrary.Get("Moon5_BridgeIgnition");
            if (bridgeSource != null && clip != null)
            {
                bridgeSource.clip = clip;
                bridgeSource.volume = 0.75f;
                bridgeSource.Play();
            }
            else if (clip != null)
            {
                AudioManager.Instance?.PlaySFX2D(clip, 0.72f);
            }
            else
            {
                AudioManager.Instance?.PlaySFX2D("Moon3_AetherRemembers");
            }

            // Climax harmonic bloom layers (empowering payoff)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTone(432f, 2.8f, 0.28f);
                AudioManager.Instance.PlayTone(648f, 2.4f, 0.19f);
                AudioManager.Instance.PlayTone(864f, 1.9f, 0.14f);
            }
        }

        /// <summary>Start the living district overtone hum (called on sequence begin).</summary>
        public void StartOvertoneDrone(float initialResonance = 0.55f)
        {
            EnsureHumSource();
            _currentResonance = initialResonance;
            if (_humSource != null && !_humSource.isPlaying)
            {
                _humSource.volume = Mathf.Clamp01(initialResonance) * 0.12f;
                _humSource.Play();
            }
            SetResonanceLevel(initialResonance);
        }

        /// <summary>Update resonance-driven volume/pitch of the White City hum (dynamic "the city sings with you").</summary>
        public void SetResonanceLevel(float normalizedResonance)
        {
            _currentResonance = Mathf.Clamp01(normalizedResonance);
            // Hum Update() lerps it; immediate nudge for responsiveness
            if (_humSource != null)
            {
                _humSource.volume = Mathf.Lerp(_humSource.volume, _currentResonance * 0.48f, 0.6f);
            }
        }

        // ─── Live Tuning Hold Interaction (1.5s frequency match) ───

        /// <summary>Begin the rising-pitch overtone feedback for pavilion tuning hold.</summary>
        public void StartPavilionTuning(int pavilionIndex, Vector3 worldPos)
        {
            if (_tuningSource == null)
            {
                var go = new GameObject("Moon5_TuningSource");
                go.transform.SetParent(transform);
                _tuningSource = go.AddComponent<AudioSource>();
                _tuningSource.loop = true;
                _tuningSource.spatialBlend = 0.85f;
                _tuningSource.rolloffMode = AudioRolloffMode.Linear;
                _tuningSource.maxDistance = 18f;
            }

            if (_tuningHarmonicClip == null)
            {
                _tuningHarmonicClip = ProceduralSFXLibrary.Get("Moon5_TuningRise");
                if (_tuningHarmonicClip == null)
                    _tuningHarmonicClip = CreateSimpleHarmonicClip(216f, 1.8f); // fallback rich base
            }

            _tuningSource.transform.position = worldPos + Vector3.up * 2.2f;
            _tuningSource.clip = _tuningHarmonicClip;
            _tuningSource.pitch = 0.58f;
            _tuningSource.volume = 0.09f;
            if (!_tuningSource.isPlaying) _tuningSource.Play();
        }

        /// <summary>Update live pitch/volume as hold progresses (0..1) — feels like locking the frequency.</summary>
        public void UpdateTuningProgress(int pavilionIndex, float progress01, float resonance = 0.6f)
        {
            if (_tuningSource == null || !_tuningSource.isPlaying) return;

            float p = Mathf.Clamp01(progress01);
            // Rising perceived frequency via pitch (cheap, musical)
            _tuningSource.pitch = Mathf.Lerp(0.58f, 1.72f, p);
            // Volume + resonance boost for empowering "match" feel
            float targetVol = Mathf.Lerp(0.09f, 0.42f, p) * (0.75f + resonance * 0.35f);
            _tuningSource.volume = Mathf.Lerp(_tuningSource.volume, targetVol, Time.deltaTime * 6f);
        }

        /// <summary>Stop/fade the tuning tone. On success we layer the stinger on top via caller.</summary>
        public void StopPavilionTuning(int pavilionIndex, bool wasCanceled = false)
        {
            if (_tuningSource == null) return;

            if (wasCanceled)
            {
                _tuningSource.Stop();
                _tuningSource.volume = 0f;
            }
            else
            {
                StartCoroutine(FadeAndStop(_tuningSource, 0.35f));
            }
        }

        System.Collections.IEnumerator FadeAndStop(AudioSource src, float dur)
        {
            if (src == null) yield break;
            float start = src.volume;
            float t = 0f;
            while (t < dur && src != null)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / dur);
                yield return null;
            }
            if (src != null) src.Stop();
        }

        public void PlayThorneRadioStatic(Vector3 approxPos)
        {
            var clip = ProceduralSFXLibrary.Get("Moon5_ThorneRadioStatic");
            if (clip != null)
            {
                AudioManager.Instance?.PlaySFX(clip, approxPos, 0.6f);
            }
            else
            {
                // Fallback static-ish
                AudioManager.Instance?.PlaySFX3D("Moon2_PurgeCrackle", approxPos, 0.45f);
            }
            // Extra carrier crackle layer
            AudioManager.Instance?.PlayTone(210f, 0.6f, 0.11f);
        }

        // ─── Internal helpers ───
        AudioSource CreatePooled3DSource(string label)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform);
            var s = go.AddComponent<AudioSource>();
            s.spatialBlend = 1f;
            return s;
        }
    }
}