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

            // ═══ Moon 2 Expanded AVH — Crystal Resonance Tones, Bell Scalar Waves, Fountain Storm, Dissonance Corruption, 432Hz Lullaby Layers ═══
            // Wired for bosses (vein wardens freq puzzles), micro-giant tuning, ionized fountain storm climax (spawner beat 4), Lirael/Cassian 432Hz lullaby, giant synergy haptics
            // All 432 Hz / 324 Hz keynote family + PHI harmonics + tritone dissonance for corruption. Procedural, no assets.
            Register("Moon2_IonizedFountainStorm", GenMoon2IonizedFountainStorm());
            Register("Moon2_CrystalResonanceTone", GenMoon2CrystalResonanceTone());
            Register("Moon2_BellScalarWave", GenMoon2BellScalarWave());
            Register("Moon2_FountainStorm", GenMoon2FountainStorm());
            Register("Moon2_DissonanceCorruption", GenMoon2DissonanceCorruption());
            Register("LiraelLullabyHum", GenLiraelLullabyHum());
            Register("Moon2_432LullabyLayer", GenMoon2LullabyLayer());
            Register("Moon2_TuningResonance", GenMoon2TuningResonance());

            // ═══ Moon 3 (Compassion & Rails — Windswept Highlands / Orphan Train Escort / Leviathan) — EXCLUSIVE ═══
            // 432Hz base lullaby rhythm system, dynamic train (wheel clack / whistle / stress), reactive Highlands wind,
            // layered Leviathan roars/attacks, emotional "The Aether Remembers" motif, tension/warmth/triumph layers.
            // All integrated to RailEscortController + AdaptiveMusicController. Zero other moons touched.
            Register("Moon3_TrainDepart", GenMoon3TrainDepart());
            Register("Moon3_TrainWheelClack", GenMoon3WheelClack());
            Register("Moon3_TrainWhistle", GenMoon3TrainWhistle());
            Register("Moon3_TrainStress", GenMoon3TrainStress());
            Register("Moon3_LullabyPulse", GenMoon3LullabyPulse());
            Register("Moon3_LullabySuccess", GenMoon3LullabySuccess());
            Register("Moon3_LullabyWarmth", GenMoon3LullabyWarmth());
            Register("Moon3_HighlandsWind", GenMoon3HighlandsWind());
            Register("Moon3_WindCalm", GenMoon3WindCalm());
            Register("Moon3_LeviathanRoar", GenMoon3LeviathanRoar());
            Register("Moon3_LeviathanScream", GenMoon3LeviathanScream());
            Register("Moon3_LeviathanImpact", GenMoon3LeviathanImpact());
            Register("Moon3_SeventeenthHourChime", GenMoon3SeventeenthHourChime());
            Register("Moon3_AetherRemembers", GenMoon3AetherRemembersMotif());
            Register("Moon3_RailTuning", GenMoon3RailTuning());
            Register("Moon3_WraithShriek", GenMoon3WraithShriek());
            Register("Moon3_TrainRestored", GenMoon3TrainRestored());

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

        // ═══════════════════════════════════════════════════════════════
        // MOON 2 EXPANDED AVH GENERATORS — Crystal resonance tones, bell scalar waves,
        // fountain storm (ionized dome climax), dissonance corruption, 432Hz lullaby layers,
        // micro-giant tuning resonance. All tied to 432 Hz harmonic series + Moon2 324 Hz keynote.
        // Scalar waves = slow beating/phase interference for living bell feel.
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Ionized fountain storm for Moon2 climax dome purify (spawner beat 4, fountain storm). Intense sparkling + rumbling storm chimes.</summary>
        static AudioClip GenMoon2IonizedFountainStorm()
        {
            int len = Samples(4.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float stormMod = Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 4.1f + Mathf.Sin(t * 1.3f) * 2.2f)), 0.65f);
                float baseChime = Sine(i, 216f) * 0.19f + Sine(i, 324f) * 0.16f + Sine(i, 648f + Mathf.Sin(t * 5f) * 3f) * 0.11f;
                float ionSpark = FilteredNoise(i, 1620f) * stormMod * 0.31f;
                float highGlint = FilteredNoise(i, 2850f) * stormMod * 0.09f * (0.5f + Mathf.Sin(t * 19f) * 0.5f);
                float rumble = FilteredNoise(i, 48f) * 0.17f * (0.3f + 0.7f * stormMod);
                float env = 0.65f + 0.35f * Mathf.Sin(t * 6.8f);
                data[i] = (baseChime + ionSpark + highGlint + rumble) * env * stormMod * 0.58f;
            }
            return MakeClip("SFX_Moon2_IonizedFountainStorm", data);
        }

        /// <summary>Crystal resonance tone cluster — pulsing 324/432 family with shimmer overtones for tuning feedback and vein solves.</summary>
        static AudioClip GenMoon2CrystalResonanceTone()
        {
            int len = Samples(2.8f);
            var data = new float[len];
            float[] baseTones = { 324f, 405f, 432f, 486f, F_HEALING * 0.81f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float pulse = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.9f * t);
                float v = 0f;
                for (int k = 0; k < baseTones.Length; k++)
                {
                    float f = baseTones[k] + Mathf.Sin(t * 1.7f + k) * 1.2f;
                    v += Sine(i, f) * (0.22f - k * 0.028f) * pulse;
                }
                float shimmer = FilteredNoise(i, 2100f) * 0.04f * (0.6f + Mathf.Sin(t * 11f) * 0.4f);
                data[i] = (v + shimmer) * 0.48f;
            }
            return MakeClip("SFX_Moon2_CrystalResonanceTone", data);
        }

        /// <summary>Bell scalar wave — long ringing bell with slow phase-beat "scalar" interference (detuned partials) for Moon2 bell tower / root core phases.</summary>
        static AudioClip GenMoon2BellScalarWave()
        {
            int len = Samples(7.8f);
            var data = new float[len];
            float root = F_MOON2_KEY * 1.5f; // ~486 Hz bell root
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float decay = Mathf.Exp(-0.92f * t);
                float scalarBeat = Mathf.Sin(2f * Mathf.PI * 0.28f * t) * 0.5f + 0.5f; // slow scalar modulation
                float fundamental = Sine(i, root) * decay * 0.41f;
                float p2 = Sine(i, root * 2.0f + scalarBeat * 1.8f) * decay * 0.29f; // detuned for beat
                float p3 = Sine(i, root * 2.97f) * decay * 0.17f;
                float scalarTail = Sine(i, root * 4.05f + Mathf.Sin(t * 0.9f) * 2.1f) * decay * 0.11f * scalarBeat;
                float metalRing = FilteredNoise(i, 2650f) * decay * 0.05f * (0.4f + scalarBeat * 0.6f);
                data[i] = (fundamental + p2 + p3 + scalarTail + metalRing) * 0.53f;
            }
            return MakeClip("SFX_Moon2_BellScalarWave", data);
        }

        /// <summary>Fountain storm variant — even more intense storm dome for ionized fountain climax + giant synergy payoff.</summary>
        static AudioClip GenMoon2FountainStorm()
        {
            int len = Samples(3.6f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float intensity = Mathf.Pow(0.4f + 0.6f * Mathf.Abs(Mathf.Sin(t * 5.3f + Mathf.Sin(t * 0.8f) * 1.9f)), 1.1f);
                float chimes = Sine(i, 216f) * 0.17f * intensity + Sine(i, 324f) * 0.14f * intensity + Sine(i, 540f) * 0.09f * intensity;
                float stormNoise = FilteredNoise(i, 1780f) * 0.33f * intensity;
                float subRumble = FilteredNoise(i, 42f) * 0.19f * (0.5f + intensity * 0.5f);
                float sparkles = FilteredNoise(i, 3200f) * 0.07f * (0.3f + Mathf.Sin(t * 27f) * 0.7f) * intensity;
                data[i] = (chimes + stormNoise + subRumble + sparkles) * 0.61f;
            }
            return MakeClip("SFX_Moon2_FountainStorm", data);
        }

        /// <summary>Deep aggressive dissonance corruption — tritone + rapid chaos static for boss desperation / re-corruption phases.</summary>
        static AudioClip GenMoon2DissonanceCorruption()
        {
            int len = Samples(2.3f);
            var data = new float[len];
            float baseF = F_MOON2_KEY * 0.5f;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float chaos = FilteredNoise(i, 1100f) * (0.65f + Mathf.Sin(t * 61f) * 0.4f);
                float trit1 = Sine(i, baseF * TRITONE * 0.68f) * 0.31f;
                float trit2 = Sine(i, baseF * TRITONE * 1.27f) * 0.24f;
                float subBeat = FilteredNoise(i, 33f) * 0.21f * (0.5f + Mathf.Sin(t * 3.8f) * 0.5f);
                float pop = (Random.value < 0.04f ? 0.9f : 0f) * (1f - t * 0.6f);
                float env = Mathf.Sin(t * Mathf.PI) * 0.95f;
                data[i] = (chaos + trit1 + trit2 + subBeat + pop) * env * 0.51f;
            }
            return MakeClip("SFX_Moon2_DissonanceCorruption", data);
        }

        /// <summary>Soft 432Hz lullaby hum layers for Lirael relief / Cassian revelation / orphan memory moments. Gentle 3-voice canon with vibrato.</summary>
        static AudioClip GenLiraelLullabyHum()
        {
            int len = Samples(6.5f);
            var data = new float[len];
            float f1 = F_HARMONIC;           // 432
            float f2 = F_HARMONIC * 1.25f;   // ~540
            float f3 = F_HARMONIC * 1.5f;    // 648
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float breath = (Mathf.Sin(t * 0.7f) * 0.5f + 0.5f);
                float vib = Mathf.Sin(t * 4.2f) * 1.8f;
                float v1 = Sine(i, f1 + vib) * 0.28f * breath;
                float v2 = Sine(i, f2 + vib * 0.7f) * 0.21f * breath;
                float v3 = Sine(i, f3 + vib * 0.4f) * 0.16f * breath;
                float warmth = FilteredNoise(i, 420f) * 0.03f * breath;
                data[i] = (v1 + v2 + v3 + warmth) * 0.39f;
            }
            return MakeClip("SFX_LiraelLullabyHum", data);
        }

        /// <summary>Layered 432Hz lullaby variant for Moon2 revelation / Crystal Remembers banner — richer harmony for 5-beat FTUE payoff.</summary>
        static AudioClip GenMoon2LullabyLayer()
        {
            int len = Samples(5.9f);
            var data = new float[len];
            float[] notes = { F_HARMONIC, F_HARMONIC * 1.2f, F_HEALING * 0.82f, F_HARMONIC * 1.618f * 0.5f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.11f * t);
                float v = 0f;
                for (int k = 0; k < notes.Length; k++)
                {
                    float phase = t * (0.6f + k * 0.1f);
                    v += Sine(i, notes[k] + Mathf.Sin(phase) * 2.3f) * (0.19f - k * 0.022f) * env;
                }
                float softBreath = FilteredNoise(i, 280f) * 0.025f * env;
                data[i] = (v + softBreath) * 0.44f;
            }
            return MakeClip("SFX_Moon2_432LullabyLayer", data);
        }

        /// <summary>Short bright resonance tone burst for micro-giant crystal tuning successes and vein node solves.</summary>
        static AudioClip GenMoon2TuningResonance()
        {
            int len = Samples(1.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.4f);
                float core = Sine(i, 432f) * 0.42f + Sine(i, 324f * 1.25f) * 0.31f + Sine(i, 540f) * 0.18f;
                float ring = FilteredNoise(i, 1350f) * 0.08f * env;
                data[i] = (core + ring) * env * 0.55f;
            }
            return MakeClip("SFX_Moon2_TuningResonance", data);
        }

        // ═══════════════════════════════════════════════
        // Moon 3 (Rail Escort / Orphan Train / Leviathan) Generators — 432Hz Lullaby Rhythm, Dynamic Train, Reactive Ambience, Emotional Motifs
        // Moon 3 exclusive. Do not reference from other moons.
        // ═══════════════════════════════════════════════

        /// <summary>Train departure whoosh + initial wheel rumble + orphan hum undertone. 432 base.</summary>
        static AudioClip GenMoon3TrainDepart()
        {
            int len = Samples(3.2f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.3f);
                float rumble = FilteredNoise(i, 85f) * 0.6f * env;
                float whoosh = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.22f * env;
                float orphanHum = (Sine(i, 432f) + Sine(i, 540f)) * 0.08f * (0.7f + 0.3f * Mathf.Sin(t * 3.8f));
                data[i] = (rumble + whoosh + orphanHum) * 0.65f;
            }
            return MakeClip("SFX_Moon3_TrainDepart", data);
        }

        /// <summary>Short metallic wheel clack for rhythmic train movement. Speed modulates via source pitch/interval in manager.</summary>
        static AudioClip GenMoon3WheelClack()
        {
            int len = Samples(0.18f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * (1f - t * 0.6f);
                float click = FilteredNoise(i, 1200f) * 0.9f;
                float ring = Sine(i, 980f) * 0.45f * Mathf.Exp(-t * 18f);
                data[i] = (click * 0.7f + ring) * env * 0.8f;
            }
            return MakeClip("SFX_Moon3_TrainWheelClack", data);
        }

        /// <summary>Proud steam whistle — long tone with 432 harmonic overtones for station arrival / morale.</summary>
        static AudioClip GenMoon3TrainWhistle()
        {
            int len = Samples(2.4f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 0.9f) * (1f - t * 0.2f);
                float fundamental = Sine(i, 432f * 0.5f) * 0.55f; // low whistle fundamental
                float harm1 = Sine(i, 432f) * 0.35f;
                float harm2 = Sine(i, 432f * 1.5f) * 0.22f;
                float steam = FilteredNoise(i, 650f) * 0.12f * env;
                data[i] = (fundamental + harm1 + harm2 + steam) * env * 0.7f;
            }
            return MakeClip("SFX_Moon3_TrainWhistle", data);
        }

        /// <summary>Train stress groan — dissonant creak + low rumble when taking damage or shield low.</summary>
        static AudioClip GenMoon3TrainStress()
        {
            int len = Samples(1.6f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t * 0.7f) * 0.9f;
                float groan = FilteredNoise(i, 95f) * 0.75f * env;
                float creak = Mathf.Sin(2f * Mathf.PI * (140f + t * 40f) * i / _sampleRate) * 0.4f * env;
                float dissonance = Sine(i, 432f * 0.7f) * 0.18f + Sine(i, 432f * 1.05f) * 0.12f; // slight sour interval
                data[i] = (groan + creak + dissonance) * 0.6f;
            }
            return MakeClip("SFX_Moon3_TrainStress", data);
        }

        /// <summary>Core 432Hz lullaby pulse — soft heartbeat for rhythm system base. Gentle sine + breath.</summary>
        static AudioClip GenMoon3LullabyPulse()
        {
            int len = Samples(1.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * 0.85f;
                float vib = Mathf.Sin(2f * Mathf.PI * 3.2f * t) * 1.4f;
                float core = Sine(i, 432f + vib) * 0.48f;
                float sub = Sine(i, 216f) * 0.32f;
                float breath = FilteredNoise(i, 380f) * 0.06f * env;
                data[i] = (core + sub + breath) * env * 0.55f;
            }
            return MakeClip("SFX_Moon3_LullabyPulse", data);
        }

        /// <summary>Successful lullaby tap — bright warm harmonic bloom, strengthens shield emotionally.</summary>
        static AudioClip GenMoon3LullabySuccess()
        {
            int len = Samples(1.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 1.05f) * (1f - t * 0.15f);
                float[] notes = { 432f, 432f * 1.25f, 528f * 0.92f, 432f * 1.5f };
                float v = 0f;
                for (int k = 0; k < notes.Length; k++)
                {
                    float vib = Mathf.Sin(t * 2.8f + k) * (1.2f - k * 0.2f);
                    v += Sine(i, notes[k] + vib) * (0.22f - k * 0.03f);
                }
                float shimmer = FilteredNoise(i, 1650f) * 0.04f * env;
                data[i] = (v + shimmer) * env * 0.72f;
            }
            return MakeClip("SFX_Moon3_LullabySuccess", data);
        }

        /// <summary>Warm sustained lullaby layer for high shield success — emotional heart, reactive to player rhythm.</summary>
        static AudioClip GenMoon3LullabyWarmth()
        {
            int len = Samples(7.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 0.07f * t);
                float vib = Mathf.Sin(2f * Mathf.PI * 1.9f * t) * 0.9f;
                float v1 = Sine(i, 432f + vib) * 0.32f;
                float v2 = Sine(i, 540f + vib * 0.6f) * 0.24f;
                float v3 = Sine(i, 648f + vib * 0.3f) * 0.18f;
                float pad = (v1 + v2 + v3) * env;
                float soft = FilteredNoise(i, 310f) * 0.04f * env;
                data[i] = (pad + soft) * 0.48f;
            }
            return MakeClip("SFX_Moon3_LullabyWarmth", data);
        }

        /// <summary>Highlands wind ambience — gusty noise with subtle 432 harmonic ring. Reacts to lullaby (calmer = sweeter tone).</summary>
        static AudioClip GenMoon3HighlandsWind()
        {
            int len = Samples(9.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float gust = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.09f * t + Mathf.Sin(t * 1.3f));
                float low = FilteredNoise(i, 68f) * 0.55f * gust;
                float high = FilteredNoise(i, 1850f) * 0.18f * (0.4f + 0.6f * Mathf.Sin(t * 2.4f));
                float ring = Sine(i, 432f * 0.25f) * 0.07f * gust; // subtle harmonic tie to lullaby
                data[i] = (low + high + ring) * 0.42f;
            }
            return MakeClip("SFX_Moon3_HighlandsWind", data);
        }

        /// <summary>Calmed post-lullaby / post-victory wind — gentler, warmer, "the highlands remember".</summary>
        static AudioClip GenMoon3WindCalm()
        {
            int len = Samples(8.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float calm = 0.35f + 0.2f * Mathf.Sin(2f * Mathf.PI * 0.04f * t);
                float low = FilteredNoise(i, 55f) * 0.42f * calm;
                float sweet = Sine(i, 432f * 0.5f) * 0.11f * calm + Sine(i, 528f * 0.5f) * 0.07f;
                data[i] = (low + sweet) * 0.38f;
            }
            return MakeClip("SFX_Moon3_WindCalm", data);
        }

        /// <summary>Deep Leviathan roar — multi-layer low growl + chest resonance for approach / tail sweep.</summary>
        static AudioClip GenMoon3LeviathanRoar()
        {
            int len = Samples(3.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 0.95f) * (1f - t * 0.25f);
                float low1 = FilteredNoise(i, 42f) * 0.85f * env;
                float low2 = FilteredNoise(i, 78f) * 0.65f * env;
                float growl = Mathf.Sin(2f * Mathf.PI * (38f + Mathf.Sin(t * 1.8f) * 7f) * i / _sampleRate) * 0.55f * env;
                data[i] = (low1 + low2 + growl) * 0.7f;
            }
            return MakeClip("SFX_Moon3_LeviathanRoar", data);
        }

        /// <summary>High piercing Leviathan scream for sonic attack / phase 2.</summary>
        static AudioClip GenMoon3LeviathanScream()
        {
            int len = Samples(2.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * Mathf.Sin(t * 3.2f * Mathf.PI);
                float scream = Sine(i, 1650f + Mathf.Sin(t * 14f) * 220f) * 0.6f * env;
                float dissonant = Sine(i, 1650f * 1.07f) * 0.35f * env; // tritone-ish
                float air = FilteredNoise(i, 2100f) * 0.25f * env;
                data[i] = (scream + dissonant + air) * 0.68f;
            }
            return MakeClip("SFX_Moon3_LeviathanScream", data);
        }

        /// <summary>Leviathan attack impact / barrage hit — heavy low thud + crystal shatter layer.</summary>
        static AudioClip GenMoon3LeviathanImpact()
        {
            int len = Samples(1.4f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t * 0.8f) * Mathf.Exp(-t * 3.5f);
                float thud = FilteredNoise(i, 58f) * 0.9f * env;
                float shatter = FilteredNoise(i, 1450f) * 0.55f * env;
                data[i] = (thud + shatter) * 0.82f;
            }
            return MakeClip("SFX_Moon3_LeviathanImpact", data);
        }

        /// <summary>17th Hour alignment chime — celestial 432 + golden harmonics for calendar moment on the train.</summary>
        static AudioClip GenMoon3SeventeenthHourChime()
        {
            int len = Samples(4.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = 0.9f * Mathf.Sin(t * Mathf.PI * 0.65f) * (1f - t * 0.1f);
                float fundamental = Sine(i, 432f) * 0.42f;
                float golden = Sine(i, 432f * 1.618f) * 0.28f;
                float high = Sine(i, 1296f) * 0.15f;
                float shimmer = FilteredNoise(i, 920f) * 0.07f * env;
                data[i] = (fundamental + golden + high + shimmer) * env * 0.6f;
            }
            return MakeClip("SFX_Moon3_SeventeenthHourChime", data);
        }

        /// <summary>THE AETHER REMEMBERS — triumphant victory motif. Ascending 432 golden-ratio cascade for emotional payoff.</summary>
        static AudioClip GenMoon3AetherRemembersMotif()
        {
            int len = Samples(6.8f);
            var data = new float[len];
            float[] motif = { 432f, 432f * PHI, 528f, 432f * 2f, 648f, 432f * PHI * 1.5f, 1296f * 0.5f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 0.7f) * (1f - t * 0.18f);
                float v = 0f;
                for (int k = 0; k < motif.Length; k++)
                {
                    float delay = k * 0.28f;
                    float localT = Mathf.Max(0f, t - delay * 0.12f);
                    float localEnv = Mathf.Sin(localT * Mathf.PI * 1.6f) * (1f - localT * 0.3f);
                    if (localT > 0.01f)
                        v += Sine(i, motif[k] + Mathf.Sin(localT * 3.1f + k) * 1.8f) * (0.18f - k * 0.014f) * localEnv;
                }
                float pad = FilteredNoise(i, 260f) * 0.035f * env;
                data[i] = (v + pad) * env * 0.78f;
            }
            return MakeClip("SFX_Moon3_AetherRemembers", data);
        }

        /// <summary>Rail tuning success click — bright harmonic for station restores / lullaby synergy.</summary>
        static AudioClip GenMoon3RailTuning()
        {
            int len = Samples(0.9f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 1.4f) * (1f - t * 0.4f);
                float tone = Sine(i, 432f) * 0.6f + Sine(i, 540f) * 0.35f + Sine(i, 648f) * 0.22f;
                float click = FilteredNoise(i, 1650f) * 0.18f * env;
                data[i] = (tone + click) * env * 0.65f;
            }
            return MakeClip("SFX_Moon3_RailTuning", data);
        }

        /// <summary>Wraith shriek replacement / variant for Moon3 rail threats — dissonant but tied to 432 context.</summary>
        static AudioClip GenMoon3WraithShriek()
        {
            int len = Samples(0.7f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t) * Mathf.Exp(-t * 4f);
                float shriek = Sine(i, 920f + t * 180f) * 0.7f * env;
                float sub = FilteredNoise(i, 140f) * 0.4f * env;
                data[i] = (shriek + sub) * 0.75f;
            }
            return MakeClip("SFX_Moon3_WraithShriek", data);
        }

        /// <summary>Train restored / victory arrival — warm resolved chord with golden resonance.</summary>
        static AudioClip GenMoon3TrainRestored()
        {
            int len = Samples(3.6f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 0.85f) * (1f - t * 0.12f);
                float chord = Sine(i, 432f) * 0.38f + Sine(i, 540f) * 0.29f + Sine(i, 648f) * 0.21f + Sine(i, 864f) * 0.15f;
                float resolve = FilteredNoise(i, 420f) * 0.05f * env;
                data[i] = (chord + resolve) * env * 0.7f;
            }
            return MakeClip("SFX_Moon3_TrainRestored", data);
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
