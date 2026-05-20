using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Audio
{
    /// <summary>
    /// Generates all gameplay SFX procedurally at startup — no .wav assets required.
    /// Uses 432 Hz tuning, golden ratio harmonics, and shaped noise.
    /// Clips are cached by name for O(1) lookup.
    /// Moon 2 additions: rich corrupted crystal cathedral atmosphere (R8 audio/env polish).
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

        // Moon 2 Lunar Moon (from C_AUDIO_DESIGN.md) — keynote E4 for melancholy purification
        const float F_MOON2_KEY = 324f; // E4 solo-cello character
        const float TRITONE = 1.41421356f; // augmented 4th / devil's interval for corruption

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

            // ═══ Moon 2 Atmosphere, Audio & Environmental Polish (Crystal Cathedral) ═══
            // Unique per-area ambiences, reactive restore/purge, crystal resonance, wind, corruption, subtle shifts
            // Keynote 324 Hz + tritone corruption per C_AUDIO_DESIGN + 12_VIVID_VISUALS fractal purge
            Register("Moon2_CorruptionDrone", GenMoon2CorruptionDrone());
            Register("Moon2_CrystalHum", GenMoon2CrystalHum());
            Register("Moon2_WindCrystals", GenMoon2WindThroughCrystals());
            Register("Moon2_BellOvertone", GenMoon2BellOvertone());
            Register("Moon2_FountainChime", GenMoon2FountainChime());
            Register("Moon2_LeyPulse", GenMoon2LeyPulse());
            Register("Moon2_PurgeCrackle", GenMoon2PurgeCrackle());
            Register("Moon2_RestoreHarmonic", GenMoon2RestoreHarmonic());
            Register("Moon2_MuralWhisper", GenMoon2MuralWhisper());
            Register("Moon2_AreaCathedral", GenMoon2AreaAmbience(324f, 0.13f, true));
            Register("Moon2_AreaBell", GenMoon2AreaAmbience(486f, 0.10f, false));
            Register("Moon2_AreaFountain", GenMoon2AreaAmbience(216f, 0.15f, true));
            Register("Moon2_AreaHall", GenMoon2AreaAmbience(648f, 0.08f, false));
            Register("Moon2_AreaLey", GenMoon2AreaAmbience(162f, 0.12f, true));

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

        // ═══════════════════════════════════════════════════════════════
        // MOON 2 — LUNAR MOON CRYSTAL CATHEDRAL ATMOSPHERE (R8 Audio/Env Polish)
        // Per C_AUDIO_DESIGN.md Moon 2: E4 324Hz keynote, solo cello melancholy,
        // bell echoes, night wind. Corruption = tritone anti-harmonics + static.
        // Reactive to restoration/purge (visuals R6/R7 "burn like fire along fuse").
        // Unique ambiences for 5 areas + crystal resonance + wind + mural whispers.
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Deep pulsing corruption drone with tritone beats and sub-bass rumble. Starts heavy in corrupted state.</summary>
        static AudioClip GenMoon2CorruptionDrone()
        {
            int len = Samples(5.2f); // long looping ambience
            var data = new float[len];
            float baseF = F_MOON2_KEY * 0.5f;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float slowPulse = Mathf.Sin(2f * Mathf.PI * 0.18f * t) * 0.5f + 0.5f;
                float dissonant = Sine(i, baseF * TRITONE) * 0.22f;
                float fundamental = Sine(i, baseF) * 0.28f;
                float sub = FilteredNoise(i, 38f) * 0.19f * (0.6f + 0.4f * slowPulse);
                float staticHiss = FilteredNoise(i, 1200f) * 0.07f * (0.3f + Mathf.Sin(t * 19f) * 0.1f);
                float env = 0.82f + 0.18f * Mathf.Sin(2f * Mathf.PI * 0.07f * t);
                data[i] = (fundamental + dissonant + sub + staticHiss) * env * 0.55f;
            }
            return MakeClip("SFX_Moon2_CorruptionDrone", data);
        }

        /// <summary>Pure crystal hum cluster — warm amber tones, 3-Band/6-Band shimmer. Used for restored interiors.</summary>
        static AudioClip GenMoon2CrystalHum()
        {
            int len = Samples(4.8f);
            var data = new float[len];
            float[] tones = { F_MOON2_KEY, F_MOON2_KEY * 1.25f, F_MOON2_KEY * 1.5f, F_HEALING * 0.65f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float v = 0f;
                for (int k = 0; k < tones.Length; k++)
                {
                    float phase = t * 0.4f * (k + 1);
                    v += Sine(i, tones[k] + Mathf.Sin(phase) * 0.8f) * (0.18f - k * 0.025f);
                }
                float shimmer = FilteredNoise(i, 2400f) * 0.035f * (0.7f + Mathf.Sin(t * 7.3f) * 0.3f);
                float env = 0.9f + 0.1f * Mathf.Sin(2f * Mathf.PI * 0.11f * t);
                data[i] = (v + shimmer) * env * 0.42f;
            }
            return MakeClip("SFX_Moon2_CrystalHum", data);
        }

        /// <summary>Wind through crystal formations — high glassy whooshes, gusts with harmonic ring. Reactive intensity.</summary>
        static AudioClip GenMoon2WindThroughCrystals()
        {
            int len = Samples(3.9f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float gust = Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 1.7f + Mathf.Sin(t * 0.6f) * 1.4f)), 1.8f);
                float whoosh = FilteredNoise(i, 1650f) * gust * 0.32f;
                float ring = Sine(i, F_MOON2_KEY * 2.03f) * gust * 0.19f + Sine(i, F_MOON2_KEY * 3.1f) * gust * 0.11f;
                float lowWind = FilteredNoise(i, 95f) * 0.14f * (0.5f + gust * 0.5f);
                data[i] = (whoosh + ring + lowWind) * 0.48f;
            }
            return MakeClip("SFX_Moon2_WindCrystals", data);
        }

        /// <summary>Metallic bell tower overtones — long ringing decay with 324 Hz root + rich partials. Height wind layer.</summary>
        static AudioClip GenMoon2BellOvertone()
        {
            int len = Samples(6.1f);
            var data = new float[len];
            float root = F_MOON2_KEY * 1.5f;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float decay = Mathf.Exp(-1.15f * t);
                float strike = (t < 0.018f) ? 0.9f : 0f;
                float fundamental = Sine(i, root) * decay * 0.38f;
                float p2 = Sine(i, root * 2.0f) * decay * 0.27f;
                float p3 = Sine(i, root * 2.97f + 0.3f) * decay * 0.18f; // slight detune for living bell
                float p4 = Sine(i, root * 4.12f) * decay * 0.12f;
                float highRing = FilteredNoise(i, 3100f) * decay * 0.06f * (0.6f + Mathf.Sin(t * 23f) * 0.2f);
                data[i] = (fundamental + p2 + p3 + p4 + highRing + strike) * 0.51f;
            }
            return MakeClip("SFX_Moon2_BellOvertone", data);
        }

        /// <summary>Liquid crystal fountain chimes + soft bubbles. Watery 216 Hz base with sparkle overtones.</summary>
        static AudioClip GenMoon2FountainChime()
        {
            int len = Samples(4.4f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float bubble = Mathf.Sin(t * 11.4f) * 0.5f + 0.5f;
                float drip = FilteredNoise(i, 620f) * 0.11f * bubble;
                float baseTone = Sine(i, 216f) * 0.29f + Sine(i, 324f) * 0.17f;
                float sparkle = Sine(i, 648f + Mathf.Sin(t * 2.1f) * 4f) * 0.13f * (0.4f + 0.6f * bubble);
                float water = FilteredNoise(i, 180f) * 0.09f;
                data[i] = (drip + baseTone + sparkle + water) * 0.46f;
            }
            return MakeClip("SFX_Moon2_FountainChime", data);
        }

        /// <summary>Deep ley chamber pulse — low gold resonance with slow 3-6-9 modulation.</summary>
        static AudioClip GenMoon2LeyPulse()
        {
            int len = Samples(5.7f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float phase = t * 0.09f;
                float pulse = (Mathf.Sin(2f * Mathf.PI * phase) * 0.5f + 0.5f);
                float low = Sine(i, 162f) * 0.33f * pulse;
                float mid = Sine(i, 324f) * 0.21f * (0.7f + 0.3f * pulse);
                float gold = Sine(i, 486f) * 0.14f * pulse;
                float subRumble = FilteredNoise(i, 29f) * 0.22f * (0.4f + 0.6f * pulse);
                data[i] = (low + mid + gold + subRumble) * 0.49f;
            }
            return MakeClip("SFX_Moon2_LeyPulse", data);
        }

        /// <summary>Erratic purge crackle + violet static. Used during corruption re-ignition / purge events.</summary>
        static AudioClip GenMoon2PurgeCrackle()
        {
            int len = Samples(1.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float chaos = FilteredNoise(i, 950f) * (0.6f + Mathf.Sin(t * 47f) * 0.35f);
                float disson = Sine(i, F_MOON2_KEY * TRITONE * 0.7f) * 0.27f + Sine(i, F_MOON2_KEY * TRITONE * 1.3f) * 0.19f;
                float pop = (Random.value < 0.03f ? 0.8f : 0f); // micro bursts
                float env = Mathf.Sin(t * Mathf.PI) * 0.9f;
                data[i] = (chaos + disson + pop) * env * 0.47f;
            }
            return MakeClip("SFX_Moon2_PurgeCrackle", data);
        }

        /// <summary>Majestic restore swell — golden harmonic bloom, fuse-burn resolution into pure tone. Ties to R7 visuals.</summary>
        static AudioClip GenMoon2RestoreHarmonic()
        {
            int len = Samples(2.6f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float attack = Mathf.Clamp01(t * 3.8f);
                float release = 1f - Mathf.Pow(Mathf.Max(0f, t - 0.55f) / 0.45f, 1.6f);
                float env = attack * release;
                float core = Sine(i, F_MOON2_KEY) * 0.38f + Sine(i, F_HEALING * 0.62f) * 0.29f;
                float over = Sine(i, F_MOON2_KEY * 2.03f) * 0.22f + Sine(i, 1296f * 0.25f) * 0.15f;
                float bloom = FilteredNoise(i, 1850f) * 0.07f * (1f - t * 0.6f);
                data[i] = (core + over + bloom) * env * 0.52f;
            }
            return MakeClip("SFX_Moon2_RestoreHarmonic", data);
        }

        /// <summary>Subtle mural whisper / abandoned site sigh — faint Old Tartarian fragments + soft sorrow. Environmental storytelling layer.</summary>
        static AudioClip GenMoon2MuralWhisper()
        {
            int len = Samples(3.3f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float breath = (Mathf.Sin(t * 1.9f) * 0.5f + 0.5f) * 0.6f;
                float voiceLike = Sine(i, 187f) * 0.21f * breath + Sine(i, 291f) * 0.13f * breath; // soft vocal formants
                float reverbTail = FilteredNoise(i, 780f) * 0.09f * breath;
                float distant = Sine(i, F_MOON2_KEY * 0.5f) * 0.07f * (0.3f + 0.7f * breath);
                data[i] = (voiceLike + reverbTail + distant) * 0.33f;
            }
            return MakeClip("SFX_Moon2_MuralWhisper", data);
        }

        /// <summary>Generic per-area long ambience generator. Varies by building (cathedral, bell, fountain, hall, ley).</summary>
        static AudioClip GenMoon2AreaAmbience(float baseFreq, float amp, bool hasWateryMod)
        {
            int len = Samples(7.4f); // very long seamless loop
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float mod = 0.82f + 0.18f * Mathf.Sin(2f * Mathf.PI * 0.023f * t + baseFreq * 0.0007f);
                float v = Sine(i, baseFreq) * 0.31f + Sine(i, baseFreq * 1.5f) * 0.19f + Sine(i, baseFreq * 2.03f) * 0.12f;
                if (hasWateryMod)
                {
                    v += FilteredNoise(i, 310f) * 0.08f * (0.5f + Mathf.Sin(t * 2.7f) * 0.5f);
                }
                float windGhost = FilteredNoise(i, 920f) * 0.05f * mod;
                data[i] = (v + windGhost) * amp * mod;
            }
            return MakeClip($"SFX_Moon2_Area_{baseFreq:F0}", data);
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
