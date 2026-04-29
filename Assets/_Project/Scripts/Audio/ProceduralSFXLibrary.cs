using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Audio
{
    /// <summary>
    /// Generates all gameplay SFX procedurally at startup — no .wav assets required.
    /// Uses 432 Hz tuning, golden ratio harmonics, and shaped noise.
    /// Clips are cached by name for O(1) lookup.
    /// </summary>
    public static class ProceduralSFXLibrary
    {
        static readonly Dictionary<string, AudioClip> _clips = new();
        static bool _initialized;
        static int _sampleRate;

        // ─── Tartarian frequency palette ───
        const float F_TELLURIC  = 7.83f;
        const float F_HARMONIC  = 432f;
        const float F_HEALING   = 528f;
        const float F_CELESTIAL = 1296f;
        const float PHI = GoldenRatioValidator.PHI;

        public static void Initialize()
        {
            if (_initialized) return;
            _sampleRate = AudioSettings.outputSampleRate;

            // Movement
            Register("Footstep",       GenFootstep());
            Register("FootstepSprint", GenFootstepSprint());
            Register("Land",           GenLand());

            // Interaction
            Register("Interact",       GenInteract());
            Register("InteractFail",   GenInteractFail());

            // Discovery & Building
            Register("Discovery",      GenDiscovery());
            Register("BuildingReveal", GenBuildingReveal());
            Register("BuildingActive", GenBuildingActive());
            Register("Emergence",      GenEmergence());

            // Tuning
            Register("TuneLock",       GenTuneLock());
            Register("TuneSuccess",    GenTuneSuccess());
            Register("TuneFail",       GenTuneFail());

            // Combat
            Register("ResonancePulse", GenResonancePulse());
            Register("HarmonicStrike", GenHarmonicStrike());
            Register("ShieldActivate", GenShieldActivate());
            Register("CombatHit",      GenCombatHit());
            Register("EnemySpawn",     GenEnemySpawn());
            Register("EnemyDeath",     GenEnemyDeath());

            // UI
            Register("UIClick",        GenUIClick());
            Register("UIOpen",         GenUIOpen());
            Register("UIClose",        GenUIClose());
            Register("QuestAccept",    GenQuestAccept());
            Register("QuestComplete",  GenQuestComplete());
            Register("SaveConfirm",    GenSaveConfirm());
            Register("AchievementPop", GenAchievementPop());
            Register("ItemPickup",     GenItemPickup());      // Feature 2: shard pickup
            Register("InventoryFull",  GenInventoryFull());   // Feature 2: fail sound
            Register("ScanNoSignal",   GenScanNoSignal());    // Feature 2: scan fail
            Register("InsufficientAether", GenInsufficientAether()); // Feature 2: not enough aether

            // Tutorial
            Register("TutorialStep",   GenTutorialStep());
            Register("TutorialDone",   GenTutorialDone());

            // Ambient
            Register("AetherVisionOn",  GenAetherVisionOn());
            Register("AetherVisionOff", GenAetherVisionOff());

            _initialized = true;
            Debug.Log($"[ProceduralSFX] Generated {_clips.Count} SFX clips.");
        }

        public static AudioClip Get(string name)
        {
            if (!_initialized) Initialize();
            return _clips.TryGetValue(name, out var clip) ? clip : null;
        }

        public static bool Has(string name)
        {
            if (!_initialized) Initialize();
            return _clips.ContainsKey(name);
        }

        static void Register(string name, AudioClip clip)
        {
            if (clip != null) _clips[name] = clip;
        }

        // ═══════════════════════════════════════════════
        // Generator methods
        // ═══════════════════════════════════════════════

        // ─── Movement ───

        static AudioClip GenFootstep()
        {
            int len = Samples(0.06f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * (1f - t); // fast decay
                data[i] = env * 0.35f * FilteredNoise(i, 200f);
            }
            return MakeClip("SFX_Footstep", data);
        }

        static AudioClip GenFootstepSprint()
        {
            int len = Samples(0.05f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * (1f - t);
                data[i] = env * 0.45f * FilteredNoise(i, 280f);
            }
            return MakeClip("SFX_FootstepSprint", data);
        }

        static AudioClip GenLand()
        {
            int len = Samples(0.12f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Exp(-8f * t);
                data[i] = env * 0.5f * (FilteredNoise(i, 120f) + 0.3f * Sine(i, 60f));
            }
            return MakeClip("SFX_Land", data);
        }

        // ─── Interaction ───

        static AudioClip GenInteract()
        {
            int len = Samples(0.15f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(F_HARMONIC, F_HEALING, t);
                float env = Mathf.Sin(t * Mathf.PI); // bell curve
                data[i] = env * 0.4f * Sine(i, freq);
            }
            return MakeClip("SFX_Interact", data);
        }

        static AudioClip GenInteractFail()
        {
            int len = Samples(0.18f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(200f, 140f, t);
                float env = (1f - t);
                data[i] = env * 0.35f * (Sine(i, freq) + 0.3f * Sine(i, freq * 1.06f)); // slight dissonance
            }
            return MakeClip("SFX_InteractFail", data);
        }

        // ─── Discovery & Building ───

        static AudioClip GenDiscovery()
        {
            // Rising arpeggio: 432 → 432*φ → 528
            int len = Samples(0.5f);
            var data = new float[len];
            float[] notes = { F_HARMONIC, F_HARMONIC * PHI, F_HEALING };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                int noteIdx = Mathf.Min((int)(t * notes.Length), notes.Length - 1);
                float noteT = (t * notes.Length) - noteIdx;
                float env = Mathf.Sin(noteT * Mathf.PI) * (0.7f + 0.3f * t);
                float overall = 1f - Mathf.Pow(Mathf.Max(0f, t - 0.8f) / 0.2f, 2);
                data[i] = env * overall * 0.4f * (Sine(i, notes[noteIdx]) + 0.2f * Sine(i, notes[noteIdx] * 2f));
            }
            return MakeClip("SFX_Discovery", data);
        }

        static AudioClip GenBuildingReveal()
        {
            int len = Samples(0.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(80f, F_HARMONIC, t * t); // accelerating rise
                float env = Mathf.Sin(t * Mathf.PI) * 0.6f;
                float rumble = Mathf.Exp(-3f * t) * 0.3f * FilteredNoise(i, 60f);
                data[i] = env * Sine(i, freq) + rumble;
            }
            return MakeClip("SFX_BuildingReveal", data);
        }

        static AudioClip GenBuildingActive()
        {
            // Bright chord: 432 + 528 + 1296/2
            int len = Samples(1.2f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float attack = Mathf.Clamp01(t * 8f);
                float release = 1f - Mathf.Pow(Mathf.Max(0f, t - 0.6f) / 0.4f, 2);
                float env = attack * release;
                data[i] = env * 0.25f * (
                    Sine(i, F_HARMONIC) +
                    0.7f * Sine(i, F_HEALING) +
                    0.4f * Sine(i, F_CELESTIAL * 0.5f) +
                    0.15f * Sine(i, F_HARMONIC * 2f));
            }
            return MakeClip("SFX_BuildingActive", data);
        }

        static AudioClip GenEmergence()
        {
            int len = Samples(2.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float rumble = Mathf.Exp(-1.5f * t) * 0.4f * FilteredNoise(i, 40f);
                float rise = Mathf.Clamp01((t - 0.3f) * 3f) * 0.5f * Sine(i, Mathf.Lerp(60f, F_HARMONIC, t));
                float shimmer = Mathf.Clamp01((t - 0.6f) * 4f) * 0.2f * Sine(i, F_HEALING) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.85f) / 0.15f, 2));
                data[i] = rumble + rise + shimmer;
            }
            return MakeClip("SFX_Emergence", data);
        }

        // ─── Tuning ───

        static AudioClip GenTuneLock()
        {
            int len = Samples(0.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = env * 0.5f * Sine(i, F_HARMONIC);
            }
            return MakeClip("SFX_TuneLock", data);
        }

        static AudioClip GenTuneSuccess()
        {
            int len = Samples(0.6f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = env * 0.4f * (Sine(i, F_HARMONIC) + 0.5f * Sine(i, F_HEALING) + 0.25f * Sine(i, F_HARMONIC * 2f));
            }
            return MakeClip("SFX_TuneSuccess", data);
        }

        static AudioClip GenTuneFail()
        {
            int len = Samples(0.4f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t);
                data[i] = env * 0.35f * (Sine(i, 180f) + 0.5f * Sine(i, 180f * 1.059f)); // minor 2nd dissonance
            }
            return MakeClip("SFX_TuneFail", data);
        }

        // ─── Combat ───

        static AudioClip GenResonancePulse()
        {
            // Whoosh outward: high noise → resonant sine
            int len = Samples(0.35f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI);
                float noise = (1f - t) * 0.4f * FilteredNoise(i, 800f);
                float tone = t * 0.5f * Sine(i, F_HARMONIC * (1f + t * 0.5f));
                data[i] = env * (noise + tone);
            }
            return MakeClip("SFX_ResonancePulse", data);
        }

        static AudioClip GenHarmonicStrike()
        {
            int len = Samples(0.25f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Exp(-6f * t);
                float impact = FilteredNoise(i, 400f) * 0.5f;
                float ring = Sine(i, F_HEALING) * 0.4f + Sine(i, F_HEALING * PHI) * 0.2f;
                data[i] = env * (impact + ring * Mathf.Clamp01(t * 5f));
            }
            return MakeClip("SFX_HarmonicStrike", data);
        }

        static AudioClip GenShieldActivate()
        {
            int len = Samples(0.4f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Clamp01(t * 6f) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.7f) / 0.3f, 2));
                float shimmer = Sine(i, F_HEALING) + 0.5f * Sine(i, F_HEALING * PHI) + 0.15f * FilteredNoise(i, 2000f);
                data[i] = env * 0.35f * shimmer;
            }
            return MakeClip("SFX_ShieldActivate", data);
        }

        static AudioClip GenCombatHit()
        {
            int len = Samples(0.08f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * (1f - t);
                data[i] = env * 0.6f * (FilteredNoise(i, 500f) + 0.4f * Sine(i, 110f));
            }
            return MakeClip("SFX_CombatHit", data);
        }

        static AudioClip GenEnemySpawn()
        {
            int len = Samples(0.6f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(40f, 160f, t);
                float env = Mathf.Sin(t * Mathf.PI);
                float rumble = FilteredNoise(i, 80f) * 0.3f * (1f - t);
                data[i] = env * 0.4f * Sine(i, freq) + rumble;
            }
            return MakeClip("SFX_EnemySpawn", data);
        }

        static AudioClip GenEnemyDeath()
        {
            int len = Samples(0.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(300f, 50f, t);
                float env = (1f - t) * (1f - t);
                float dissolve = FilteredNoise(i, Mathf.Lerp(600f, 100f, t)) * 0.3f;
                data[i] = env * (0.4f * Sine(i, freq) + dissolve);
            }
            return MakeClip("SFX_EnemyDeath", data);
        }

        // ─── UI ───

        static AudioClip GenUIClick()
        {
            int len = Samples(0.03f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t);
                data[i] = env * 0.3f * Sine(i, 1200f);
            }
            return MakeClip("SFX_UIClick", data);
        }

        static AudioClip GenUIOpen()
        {
            int len = Samples(0.12f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(600f, 900f, t);
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = env * 0.25f * Sine(i, freq);
            }
            return MakeClip("SFX_UIOpen", data);
        }

        static AudioClip GenUIClose()
        {
            int len = Samples(0.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(900f, 500f, t);
                float env = (1f - t);
                data[i] = env * 0.2f * Sine(i, freq);
            }
            return MakeClip("SFX_UIClose", data);
        }

        static AudioClip GenQuestAccept()
        {
            int len = Samples(0.3f);
            var data = new float[len];
            float[] notes = { F_HARMONIC, F_HARMONIC * PHI };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                int n = t < 0.5f ? 0 : 1;
                float noteT = (t * 2f) - n;
                float env = Mathf.Sin(noteT * Mathf.PI);
                float fade = 1f - Mathf.Pow(Mathf.Max(0f, t - 0.85f) / 0.15f, 2);
                data[i] = env * fade * 0.35f * Sine(i, notes[n]);
            }
            return MakeClip("SFX_QuestAccept", data);
        }

        static AudioClip GenQuestComplete()
        {
            // Triumphant ascending triad: 432 → 528 → 648(=432*PHI÷φ^-1 ~864/PHI)
            int len = Samples(0.8f);
            var data = new float[len];
            float[] notes = { F_HARMONIC, F_HEALING, F_HARMONIC * 2f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                int n = Mathf.Min((int)(t * notes.Length), notes.Length - 1);
                float noteT = (t * notes.Length) - n;
                float env = Mathf.Sin(noteT * Mathf.PI) * (0.6f + 0.4f * t);
                float fade = 1f - Mathf.Pow(Mathf.Max(0f, t - 0.85f) / 0.15f, 2);
                data[i] = env * fade * 0.35f * (Sine(i, notes[n]) + 0.4f * Sine(i, notes[n] * 1.5f));
            }
            return MakeClip("SFX_QuestComplete", data);
        }

        static AudioClip GenSaveConfirm()
        {
            int len = Samples(0.15f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = env * 0.2f * (Sine(i, 800f) + 0.5f * Sine(i, 1200f));
            }
            return MakeClip("SFX_SaveConfirm", data);
        }

        static AudioClip GenAchievementPop()
        {
            int len = Samples(0.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Exp(-3f * t) * 0.5f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                data[i] = env * (Sine(i, F_HEALING) + 0.5f * Sine(i, F_CELESTIAL * 0.5f) + 0.2f * Sine(i, F_HARMONIC * 2f));
            }
            return MakeClip("SFX_AchievementPop", data);
        }

        static AudioClip GenItemPickup()
        {
            // Golden chime: 432 → 528 → 648 sparkle
            int len = Samples(0.4f);
            var data = new float[len];
            float[] notes = { F_HARMONIC, F_HEALING, F_HARMONIC * 1.5f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                int n = Mathf.Min((int)(t * notes.Length), notes.Length - 1);
                float noteT = (t * notes.Length) - n;
                float env = Mathf.Sin(noteT * Mathf.PI) * Mathf.Exp(-2f * t);
                data[i] = env * 0.4f * (Sine(i, notes[n]) + 0.3f * Sine(i, notes[n] * 2f));
            }
            return MakeClip("SFX_ItemPickup", data);
        }

        static AudioClip GenInventoryFull()
        {
            // Dull thud: low freq with slight dissonance
            int len = Samples(0.25f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * (1f - t);
                data[i] = env * 0.3f * (Sine(i, 180f) + 0.4f * Sine(i, 185f)); // slight beating
            }
            return MakeClip("SFX_InventoryFull", data);
        }

        static AudioClip GenScanNoSignal()
        {
            // Descending tone: disappointment
            int len = Samples(0.3f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(F_HARMONIC, F_HARMONIC * 0.7f, t);
                float env = Mathf.Exp(-4f * t);
                data[i] = env * 0.25f * Sine(i, freq);
            }
            return MakeClip("SFX_ScanNoSignal", data);
        }

        static AudioClip GenInsufficientAether()
        {
            // Same as ScanNoSignal but slightly different freq for variety
            int len = Samples(0.28f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(360f, 240f, t);
                float env = (1f - t) * 0.8f;
                data[i] = env * 0.3f * Sine(i, freq);
            }
            return MakeClip("SFX_InsufficientAether", data);
        }

        // ─── Tutorial ───

        static AudioClip GenTutorialStep()
        {
            int len = Samples(0.2f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = env * 0.3f * Sine(i, F_HARMONIC * PHI);
            }
            return MakeClip("SFX_TutorialStep", data);
        }

        static AudioClip GenTutorialDone()
        {
            int len = Samples(0.6f);
            var data = new float[len];
            float[] sweep = { F_HARMONIC, F_HEALING, F_HARMONIC * 2f, F_CELESTIAL * 0.5f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                int n = Mathf.Min((int)(t * sweep.Length), sweep.Length - 1);
                float noteT = (t * sweep.Length) - n;
                float env = Mathf.Sin(noteT * Mathf.PI);
                float fade = 1f - Mathf.Pow(Mathf.Max(0f, t - 0.9f) / 0.1f, 2);
                data[i] = env * fade * 0.35f * (Sine(i, sweep[n]) + 0.3f * Sine(i, sweep[n] * 2f));
            }
            return MakeClip("SFX_TutorialDone", data);
        }

        // ─── Ambient / Aether Vision ───

        static AudioClip GenAetherVisionOn()
        {
            int len = Samples(0.4f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Clamp01(t * 4f) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.7f) / 0.3f, 2));
                float shimmer = Sine(i, F_TELLURIC * 55f) * 0.4f + FilteredNoise(i, 3000f) * 0.15f + Sine(i, F_HEALING) * 0.3f;
                data[i] = env * shimmer;
            }
            return MakeClip("SFX_AetherVisionOn", data);
        }

        static AudioClip GenAetherVisionOff()
        {
            int len = Samples(0.3f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * (1f - t);
                float shimmer = Sine(i, F_TELLURIC * 55f) * 0.3f + FilteredNoise(i, 2000f) * 0.1f;
                data[i] = env * shimmer;
            }
            return MakeClip("SFX_AetherVisionOff", data);
        }

        // ═══════════════════════════════════════════════
        // DSP Primitives
        // ═══════════════════════════════════════════════

        static int Samples(float seconds) => Mathf.CeilToInt(_sampleRate * seconds);

        static float Sine(int sampleIndex, float freq)
        {
            return Mathf.Sin(2f * Mathf.PI * freq * sampleIndex / _sampleRate);
        }

        /// <summary>
        /// Deterministic pseudo-noise filtered to a rough cutoff frequency.
        /// Uses a simple one-pole low-pass approximation.
        /// </summary>
        static float FilteredNoise(int sampleIndex, float cutoffHz)
        {
            // Hash-based deterministic noise
            uint h = (uint)(sampleIndex * 196314165 + 907633515);
            h ^= h >> 13; h *= 1274126177u; h ^= h >> 16;
            float raw = (h / (float)uint.MaxValue) * 2f - 1f;

            // Simple smoothing factor to approximate LPF
            float rc = 1f / (2f * Mathf.PI * cutoffHz);
            float dt = 1f / _sampleRate;
            float alpha = dt / (rc + dt);

            // Apply one-pole filter (stateless approximation — blends with previous sample's noise)
            if (sampleIndex > 0)
            {
                uint hp = (uint)((sampleIndex - 1) * 196314165 + 907633515);
                hp ^= hp >> 13; hp *= 1274126177u; hp ^= hp >> 16;
                float prev = (hp / (float)uint.MaxValue) * 2f - 1f;
                raw = prev + alpha * (raw - prev);
            }
            return raw;
        }

        static AudioClip MakeClip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, _sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
