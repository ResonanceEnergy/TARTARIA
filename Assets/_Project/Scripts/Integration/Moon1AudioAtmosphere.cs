using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 audio atmosphere — three concentric ambient zones + restoration stinger
    /// + Aether-band layer switching (Telluric 7.83 / Harmonic 432 / Celestial 528).
    /// Per CLAUDE.md "no stubs" — all clips generated procedurally if no asset,
    /// real AudioSources with linear rolloff and proper spatial blend.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1AudioAtmosphere : MonoBehaviour
    {
        static Moon1AudioAtmosphere _instance;

        AudioSource _villageHum;
        AudioSource _perimeterWind;
        AudioSource _mudGurgle;
        AudioSource _bandTelluric;
        AudioSource _bandHarmonic;
        AudioSource _bandCelestial;

        const float HUM_HZ = 110f;
        const float TELLURIC_HZ = 7.83f;
        const float HARMONIC_HZ = 432f;
        const float CELESTIAL_HZ = 528f;

        int _buildingsRestored;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1AudioAtmosphere");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1AudioAtmosphere>();
        }

        void OnEnable()
        {
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        void Start()
        {
            var root = new GameObject("Moon1_Audio_Root");
            root.transform.SetParent(transform);

            // Zone 1: village center hum
            _villageHum = SpawnLoopingSource(root.transform, "VillageHum",
                Vector3.zero, GenerateBassHumClip(HUM_HZ), 0.22f, 2f, 22f);
            // Zone 2: perimeter wind
            _perimeterWind = SpawnLoopingSource(root.transform, "PerimeterWind",
                new Vector3(0f, 6f, 0f), GenerateWindClip(), 0.18f, 30f, 80f);
            // Zone 3: mud-pool gurgle
            _mudGurgle = SpawnLoopingSource(root.transform, "MudGurgle",
                new Vector3(-50f, 0.5f, 35f), GenerateGurgleClip(), 0.25f, 4f, 14f);

            // Aether bands — start silent, fade up when band is tuned
            _bandTelluric = SpawnLoopingSource(root.transform, "Band_Telluric",
                Vector3.zero, GenerateSineWaveClip(TELLURIC_HZ, 8f, includeSubBass: true), 0f, 5f, 100f);
            _bandHarmonic = SpawnLoopingSource(root.transform, "Band_Harmonic",
                Vector3.zero, GenerateSineWaveClip(HARMONIC_HZ, 6f), 0f, 5f, 100f);
            _bandCelestial = SpawnLoopingSource(root.transform, "Band_Celestial",
                Vector3.zero, GenerateSineWaveClip(CELESTIAL_HZ, 6f), 0f, 5f, 100f);

            Debug.Log("[Moon1AudioAtmosphere] 6 ambient sources online (village hum, wind, gurgle, 3 Aether bands silent).");
        }

        AudioSource SpawnLoopingSource(Transform parent, string name, Vector3 worldPos,
                                       AudioClip clip, float volume, float minDist, float maxDist)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = worldPos;
            var src = go.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = true;
            src.volume = volume;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = minDist;
            src.maxDistance = maxDist;
            src.clip = clip;
            if (clip != null) src.Play();
            return src;
        }

        void HandleBuildingRestored(string buildingId)
        {
            _buildingsRestored++;
            PlayRestorationStinger();
            // First building → fade in Harmonic band, third → Celestial, beat 'em all → Telluric anchor
            switch (_buildingsRestored)
            {
                case 1: StartCoroutine(FadeIn(_bandHarmonic, 0.30f, 4f)); break;
                case 2: StartCoroutine(FadeIn(_bandCelestial, 0.22f, 4f)); break;
                case 3:
                    StartCoroutine(FadeIn(_bandTelluric, 0.45f, 6f));
                    StartCoroutine(FadeIn(_bandHarmonic, 0.35f, 6f));
                    StartCoroutine(FadeIn(_bandCelestial, 0.30f, 6f));
                    break;
            }
        }

        void PlayRestorationStinger()
        {
            var stingerGO = new GameObject("RestorationStinger");
            stingerGO.transform.SetParent(transform);
            var src = stingerGO.AddComponent<AudioSource>();
            src.spatialBlend = 0f; // 2D — bypass spatial attenuation
            src.volume = 0.55f;
            src.clip = GenerateStingerClip();
            src.Play();
            // Self-clean
            Destroy(stingerGO, src.clip.length + 0.5f);
        }

        System.Collections.IEnumerator FadeIn(AudioSource src, float target, float dur)
        {
            if (src == null) yield break;
            float t = 0f;
            float start = src.volume;
            while (t < dur)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(start, target, t / dur);
                yield return null;
            }
            src.volume = target;
        }

        // ───────────── procedural clip generation ─────────────

        AudioClip GenerateSineWaveClip(float hz, float dur, bool includeSubBass = false)
        {
            const int sr = 44100;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("Sine_" + Mathf.RoundToInt(hz) + "Hz", samples, 1, sr, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float fundamental = Mathf.Sin(2f * Mathf.PI * hz * t) * 0.75f;
                float fifth = Mathf.Sin(2f * Mathf.PI * hz * 1.5f * t) * 0.20f;
                float val = (fundamental + fifth) * 0.30f;
                if (includeSubBass && hz < 50f)
                {
                    // Pulse a 60Hz carrier modulated by the very low tone
                    float carrier = Mathf.Sin(2f * Mathf.PI * 60f * t);
                    val += carrier * (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * hz * t)) * 0.20f;
                }
                data[i] = val;
            }
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateBassHumClip(float baseHz)
        {
            const int sr = 44100;
            const float dur = 8f;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("BassHum_" + Mathf.RoundToInt(baseHz) + "Hz", samples, 1, sr, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float f1 = Mathf.Sin(2f * Mathf.PI * baseHz * t) * 0.5f;
                float f2 = Mathf.Sin(2f * Mathf.PI * baseHz * 0.5f * t) * 0.3f;
                float f3 = Mathf.Sin(2f * Mathf.PI * baseHz * 2f * t) * 0.15f;
                float env = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.17f * t);
                data[i] = (f1 + f2 + f3) * 0.32f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateWindClip()
        {
            const int sr = 44100;
            const float dur = 12f;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("Wind", samples, 1, sr, false);
            var data = new float[samples];
            // Pseudo-random noise lowpassed via running average
            var rand = new System.Random(12345);
            float lp = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                lp = lp * 0.97f + noise * 0.03f; // low-pass
                float gust = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.07f * t + Mathf.Sin(t * 0.21f));
                data[i] = lp * gust * 0.45f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateGurgleClip()
        {
            const int sr = 44100;
            const float dur = 6f;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("MudGurgle", samples, 1, sr, false);
            var data = new float[samples];
            var rand = new System.Random(7777);
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                // Random "blip" events sparsely with low-frequency tonal body
                float body = Mathf.Sin(2f * Mathf.PI * (45f + 8f * Mathf.Sin(t * 0.4f)) * t) * 0.25f;
                float blip = 0f;
                if (rand.NextDouble() < 0.0003)
                {
                    // start a damped sine "burble"
                    blip = (float)(rand.NextDouble() * 0.4f);
                }
                data[i] = (body + blip) * 0.4f;
            }
            // Apply per-sample slow gain envelope
            for (int i = 1; i < samples; i++)
            {
                data[i] = data[i] * 0.7f + data[i - 1] * 0.3f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateStingerClip()
        {
            const int sr = 44100;
            const float dur = 6f;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("RestorationStinger", samples, 1, sr, false);
            var data = new float[samples];
            // Cascading major arpeggio: C4 (264) → E4 (330) → G4 (396) → C5 (528)
            float[] notes = { 264f, 330f, 396f, 528f };
            float noteDur = dur / notes.Length;
            for (int n = 0; n < notes.Length; n++)
            {
                int s0 = (int)(n * noteDur * sr);
                int s1 = (int)((n + 1) * noteDur * sr);
                if (s1 > samples) s1 = samples;
                for (int i = s0; i < s1; i++)
                {
                    float t = (float)(i - s0) / sr;
                    // ADSR-ish envelope
                    float a = 0.05f, d = 0.10f, s = 0.65f, r = 0.5f;
                    float env;
                    if (t < a) env = t / a;
                    else if (t < a + d) env = 1f - (1f - s) * ((t - a) / d);
                    else if (t < noteDur - r) env = s;
                    else env = s * (1f - (t - (noteDur - r)) / r);
                    if (env < 0f) env = 0f;
                    float val = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * 0.5f
                              + Mathf.Sin(2f * Mathf.PI * notes[n] * 2f * t) * 0.18f
                              + Mathf.Sin(2f * Mathf.PI * notes[n] * 3f * t) * 0.08f;
                    // Add reverbed prior notes for cascade
                    for (int pn = 0; pn < n; pn++)
                    {
                        float decay = Mathf.Exp(-1.2f * (t + (n - pn) * noteDur));
                        val += Mathf.Sin(2f * Mathf.PI * notes[pn] * (t + (n - pn) * noteDur)) * 0.18f * decay;
                    }
                    data[i] = val * env * 0.45f;
                }
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
