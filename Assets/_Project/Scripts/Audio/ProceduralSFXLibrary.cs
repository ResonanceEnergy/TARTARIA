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

            // ═══ Moon 1 Echohaven (Magnetic Moon — The Awakening) — rich magical zone audio for scaffold + first 5-10 min FTUE ═══
            // Buried resonance hum (432 + PHI family overtones), first scan/tune stingers, Milo discovery reactions,
            // corruption drones (gentle dissonance), F310-synced success tones, ethereal motes wind layer.
            // All procedural, 432Hz core + golden ratio harmonics (PHI) for wondrous alive feel the moment Populate runs.
            // Used by Moon1EchohavenScaffold placements + Moon1FirstTuningTrigger + EchohavenContentSpawner ambient.
            Register("Moon1_BuriedResonanceHum", GenMoon1BuriedResonanceHum());
            Register("Moon1_ScanStinger", GenMoon1ScanStinger());
            Register("Moon1_TuneSuccessStinger", GenMoon1TuneSuccessStinger());
            Register("Moon1_CorruptionDrone", GenMoon1CorruptionDrone());
            Register("Moon1_MiloDiscovery", GenMoon1MiloDiscoveryReaction());
            Register("Moon1_F310SyncedTone", GenMoon1F310SyncedTone());
            Register("Moon1_EtherealMotes", GenMoon1EtherealMotes());

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

            // ═══ Moon 2 First Purge FTUE (Lunar Moon: Shadow & Purge) — immediate vertical slice audio (modeled on fresh Moon1 rich block) ═══
            // Dissonance vein hum at start site, cathartic purge stinger, Lirael first reaction chime, F310-synced success tone,
            // wraith teaser whisper. 324 Hz keynote + tritone corruption → pure 432/PHI crystal bloom on success.
            // Used by Moon2ZoneScaffold Populate + Moon2FirstPurgeTrigger.
            Register("Moon2_FirstVeinDissonanceHum", GenMoon2FirstVeinDissonanceHum());
            Register("Moon2_FirstPurgeStinger", GenMoon2FirstPurgeStinger());
            Register("Moon2_LiraelFirstPurgeReaction", GenMoon2LiraelFirstPurgeReaction());
            Register("Moon2_PurgeSuccessF310Tone", GenMoon2PurgeSuccessF310Tone());
            Register("Moon2_DissonanceWraithWhisper", GenMoon2DissonanceWraithWhisper());
            Register("Moon2_PurifiedCrystalHum", GenMoon2PurifiedCrystalHum());

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

            // ═══ Moon 4 (Resonance Moon — Water Temple + Golem Puzzle) — EXCLUSIVE ═══
            // Water flow restoration, bastion tuning snaps, golem awakening bass, bell tower harmony waves,
            // memory crystal chimes. All 432Hz + water overtones + earth rumbles.
            // TODO: Implement Moon 4 SFX generators
            /*
            Register("Moon4_BastionSnap", GenMoon4BastionSnap());
            Register("Moon4_WaterFlow", GenMoon4WaterFlowRestoration());
            Register("Moon4_GolemRoar", GenMoon4GolemRoar());
            Register("Moon4_BellTowerWaves", GenMoon4BellTowerHarmony());
            Register("Moon4_MemoryCrystal", GenMoon4MemoryCrystal());
            */

            // ═══ Moon 5 (Overtone Moon — White City Echo District Amplification) — EXCLUSIVE ═══
            // 432Hz + PHI harmonic overtones for tuning stingers, 6-band healing auras (528 family),
            // aurora fountain whooshes, bridge ignition motif (rising golden chord), Thorne radio crackle/static.
            // TuningRise for live hold-E frequency match feedback (rising pitch + partial bloom).
            // All procedural, 60fps friendly, pairs with Moon5WhiteCityAudioManager + hold interaction.
            Register("Moon5_AmplificationStinger", GenMoon5AmplificationStinger());
            Register("Moon5_HealingAuraTone", GenMoon5HealingAuraTone());
            Register("Moon5_FountainWhoosh", GenMoon5FountainWhoosh());
            Register("Moon5_BridgeIgnition", GenMoon5BridgeIgnition());
            Register("Moon5_ThorneRadioStatic", GenMoon5ThorneRadioStatic());
            Register("Moon5_TuningRise", GenMoon5TuningRise());

            // ═══ Moon 6 (Rhythmic Moon — Sunken Cathedral Organ Symphony) — EXCLUSIVE ═══
            // Full pipe organ tones (12 pipes = 12-note chromatic), hydraulic fountain flows,
            // Cymatic Requiem climax layers, Lirael choir harmonics, bell tolls, crystal chimes.
            // 432Hz + PHI + organ overtones (2nd/3rd/5th harmonics). Zero other moons touched.
            Register("Moon6_BrokenMelody", GenMoon6BrokenMelody());
            Register("Moon6_PipeRepair", GenMoon6PipeRepair());
            Register("Moon6_OrganTone", GenMoon6OrganTone());
            Register("Moon6_FountainFlow", GenMoon6FountainFlow());
            Register("Moon6_CymaticRequiem", GenMoon6CymaticRequiem());
            Register("Moon6_LiraelChoir", GenMoon6LiraelChoir());
            Register("Moon6_BellToll", GenMoon6BellToll());
            Register("Moon6_CrystalChime", GenMoon6CrystalChime());
            Register("Moon6_HydraulicBellows", GenMoon6HydraulicBellows());
            Register("Moon6_CathedralAmbience", GenMoon6CathedralAmbience());
            Register("Moon6_IonicMistRain", GenMoon6IonicMistRain());

            // ═══ Moon 7 (Resonant Moon — Korath Awakening + Giant Stasis Vault) — EXCLUSIVE ═══
            // Ice thaw crackles, 9-band aurora energy (violet 432*φ²), Korath voice rumbles,
            // golem siege bass impacts, Cassian confrontation tension, harmonic rock cutting SFX,
            // Korath sacrifice golden surge, stasis vault ambient (deep sub-bass + aurora hum).
            Register("Moon7_IceThaw", GenMoon7IceThaw());
            Register("Moon7_AuroraHum", GenMoon7AuroraHum());
            Register("Moon7_KorathVoice", GenMoon7KorathVoiceRumble());
            Register("Moon7_KorathAwakening", GenMoon7KorathAwakening());
            Register("Moon7_GolemSiege", GenMoon7GolemSiegeBass());
            Register("Moon7_CassianTension", GenMoon7CassianTension());
            Register("Moon7_HarmonicCutting", GenMoon7HarmonicRockCutting());
            Register("Moon7_KorathSacrifice", GenMoon7KorathSacrifice());
            Register("Moon7_StasisAmbience", GenMoon7StasisVaultAmbience());
            Register("Moon7_9BandUnlock", GenMoon79BandUnlock());
            Register("Moon7_VioletPulse", GenMoon7VioletPulse());

            // ═══ Moons 8-13 (Late Game — Continental Scale + Boss Encounters) — EXCLUSIVE ═══
            // Airship propulsion, rail network hum, aquifer purification, seismic tremors,
            // bell tower harmonies, aether tremors, fountain chains, leviathan death, final victory fanfare.
            // TODO: Implement Moons 8-13 SFX generators
            /*
            Register("Moon8_AirshipLaunch", GenMoon8AirshipLaunch());
            Register("Moon9_RailHum", GenMoon9RailNetworkHum());
            Register("Moon10_LeviathanRoar", GenMoon10LeviathanRoar());
            Register("Moon10_LeviathanDeath", GenMoon10LeviathanDeath());
            Register("Moon11_AquiferPurification", GenMoon11AquiferPurification());
            Register("Moon11_FountainChainActivation", GenMoon11FountainChainActivation());
            Register("Moon12_BellTowerSync", GenMoon12BellTowerSync());
            Register("Moon12_TowerHarmony", GenMoon12TowerHarmonyRing());
            Register("Moon13_AetherTremor", GenMoon13AetherTremor());
            Register("Moon13_SeismicTremor", GenMoon13SeismicTremor());
            */
            Register("BossPhaseTransition", GenBossPhaseTransition());

            // ═══ Global — Moon Clear + End Game fanfares ═══
            Register("moon_clear",                  GenMoonClearFanfare());
            Register("game_complete_credits_theme", GenGameCompleteCreditsTheme());

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

        // ═══ Moon 2 First Purge FTUE Generators (Shadow & Purge vertical slice) ═══
        // Directly modeled on the rich Moon1 block just delivered. Dissonance (fractured 324 + tritone) → cathartic pure 432/PHI crystal bloom on successful first vein purge.
        // Used by Moon2FirstPurgeTrigger + Moon2ZoneScaffold Populate.

        /// <summary>Initial dissonance vein hum at the first playable site — low fractured 324 Hz + tritone corruption, slow pulse, crystalline crackle. The caverns are sick.</summary>
        static AudioClip GenMoon2FirstVeinDissonanceHum()
        {
            int len = Samples(5.5f);
            var data = new float[len];
            float fBase = 324f * 0.333f; // deep sub
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = 0.75f + Mathf.Sin(t * 6.28f) * 0.12f;
                float core = Sine(i, fBase) * 0.55f + Sine(i, 324f) * 0.38f + Sine(i, 324f * 1.5f) * 0.22f; // tritone lean
                float crackle = FilteredNoise(i, 1850f) * 0.09f * env;
                float pulse = Mathf.Sin(t * 1.8f) * 0.08f;
                data[i] = (core + crackle + pulse) * env * 0.32f;
            }
            return MakeClip("SFX_Moon2_FirstVeinDissonanceHum", data);
        }

        /// <summary>Cathartic first purge success stinger — shadow fractures into pure 432 + PHI crystal bloom. Emotional release for the vertical slice.</summary>
        static AudioClip GenMoon2FirstPurgeStinger()
        {
            int len = Samples(2.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.35f);
                // Start fractured, bloom pure
                float diss = Sine(i, 324f) * 0.6f + Sine(i, 486f) * 0.4f;
                float pure = Sine(i, 432f) * 0.7f + Sine(i, 432f * 1.618f) * 0.55f + Sine(i, 528f) * 0.35f + Sine(i, 699f) * 0.28f;
                float blend = Mathf.Lerp(diss, pure, Mathf.SmoothStep(0.2f, 0.85f, t));
                float sparkle = FilteredNoise(i, 2100f) * 0.11f * env;
                data[i] = (blend + sparkle) * env * 0.48f;
            }
            return MakeClip("SFX_Moon2_FirstPurgeStinger", data);
        }

        /// <summary>Lirael first purge reaction chime — warm shadow-to-light shift, 432 core with soft PHI breath. Companion emotional payoff.</summary>
        static AudioClip GenMoon2LiraelFirstPurgeReaction()
        {
            int len = Samples(2.6f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t * 0.6f) * (0.6f + Mathf.Sin(t * 4.2f) * 0.25f);
                float core = Sine(i, 432f) * 0.48f + Sine(i, 432f * 1.25f) * 0.32f + Sine(i, 528f) * 0.27f;
                float breath = Sine(i, 864f) * 0.18f * env;
                data[i] = (core + breath) * env * 0.41f;
            }
            return MakeClip("SFX_Moon2_LiraelFirstPurgeReaction", data);
        }

        /// <summary>Tight bright F310-synced success tone for the exact moment the first vein purges. Pairs with rumble.</summary>
        static AudioClip GenMoon2PurgeSuccessF310Tone()
        {
            int len = Samples(0.72f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI);
                float tone = Sine(i, 699f) * 0.65f + Sine(i, 432f * 1.618f) * 0.48f + Sine(i, 1080f) * 0.22f;
                data[i] = tone * env * 0.62f;
            }
            return MakeClip("SFX_Moon2_PurgeSuccessF310Tone", data);
        }

        /// <summary>Subtle wraith whisper / fractal threat layer for the Conflict teaser spawn after first purge. Distant, unsettling, low volume.</summary>
        static AudioClip GenMoon2DissonanceWraithWhisper()
        {
            int len = Samples(3.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (0.4f + Mathf.Sin(t * 2.1f) * 0.3f) * (1f - t * 0.7f);
                float whisper = FilteredNoise(i, 920f) * 0.7f + Sine(i, 162f) * 0.55f + Sine(i, 243f) * 0.4f; // tritone cluster
                data[i] = whisper * env * 0.18f;
            }
            return MakeClip("SFX_Moon2_DissonanceWraithWhisper", data);
        }

        /// <summary>Soft pure singing hum from the newly purified crystal after successful first purge. Permanent world change audio.</summary>
        static AudioClip GenMoon2PurifiedCrystalHum()
        {
            int len = Samples(7.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = 0.82f + Mathf.Sin(t * 1.4f) * 0.11f;
                float pure = Sine(i, 432f) * 0.52f + Sine(i, 432f * 1.618f) * 0.38f + Sine(i, 648f) * 0.25f;
                float shimmer = FilteredNoise(i, 1650f) * 0.07f * env;
                data[i] = (pure + shimmer) * env * 0.29f;
            }
            return MakeClip("SFX_Moon2_PurifiedCrystalHum", data);
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
        // Moon 5 Overtone Generators (procedural, 432Hz family + PHI harmonics)
        // Lightweight sine + filtered noise, no assets. Used by audio manager for alive tuning feel.
        // ═══════════════════════════════════════════════

        static AudioClip GenMoon5AmplificationStinger()
        {
            int len = Samples(1.35f);
            var data = new float[len];
            float b = F_HARMONIC; // 432
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 0.92f) * (1f - t * 0.28f);
                float vib = Mathf.Sin(t * 11f) * 2.2f;
                float v = Sine(i, b) * 0.52f
                        + Sine(i, b * 1.5f + vib) * 0.31f
                        + Sine(i, b * 2.02f) * 0.19f
                        + Sine(i, F_HEALING) * 0.24f
                        + Sine(i, b * PHI * 0.5f) * 0.13f;
                float air = FilteredNoise(i, 1950f) * 0.07f * env;
                data[i] = (v + air) * env * 0.68f;
            }
            return MakeClip("SFX_Moon5_AmplificationStinger", data);
        }

        static AudioClip GenMoon5HealingAuraTone()
        {
            int len = Samples(3.8f); // soft looping-friendly aura
            var data = new float[len];
            float[] tones = { F_HEALING, F_HARMONIC * 1.22f, 648f, F_HEALING * 0.75f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float pulse = 0.82f + 0.18f * Mathf.Sin(2f * Mathf.PI * 0.19f * t);
                float v = 0f;
                for (int k = 0; k < tones.Length; k++)
                {
                    float f = tones[k] + Mathf.Sin(t * 0.8f + k) * 0.6f;
                    v += Sine(i, f) * (0.19f - k * 0.022f) * pulse;
                }
                float shimmer = FilteredNoise(i, 1420f) * 0.025f * pulse;
                data[i] = (v + shimmer) * 0.38f;
            }
            return MakeClip("SFX_Moon5_HealingAuraTone", data);
        }

        static AudioClip GenMoon5FountainWhoosh()
        {
            int len = Samples(2.1f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float gust = Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 2.7f + Mathf.Sin(t * 1.1f) * 1.6f)), 1.4f);
                float whoosh = FilteredNoise(i, 1250f) * gust * 0.38f;
                float water = Sine(i, 216f) * 0.21f + Sine(i, 324f) * 0.14f;
                float sparkle = FilteredNoise(i, 2650f) * 0.09f * gust * (0.6f + Mathf.Sin(t * 19f) * 0.4f);
                float env = Mathf.Sin(t * Mathf.PI * 0.85f) * (0.9f + 0.1f * gust);
                data[i] = (whoosh + water + sparkle) * env * 0.52f;
            }
            return MakeClip("SFX_Moon5_FountainWhoosh", data);
        }

        static AudioClip GenMoon5BridgeIgnition()
        {
            int len = Samples(4.2f);
            var data = new float[len];
            float b = F_HARMONIC;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Clamp01(t * 1.8f) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.65f) / 0.35f, 1.3f));
                float rise = Mathf.Clamp01((t - 0.1f) * 1.6f);
                float chord = Sine(i, b * 0.5f) * 0.42f * rise
                            + Sine(i, b) * 0.38f
                            + Sine(i, b * 1.5f + Mathf.Sin(t * 4f) * 3f) * 0.25f * rise
                            + Sine(i, 528f) * 0.19f;
                float surge = FilteredNoise(i, 820f) * 0.14f * env * rise;
                float high = FilteredNoise(i, 2100f) * 0.06f * env;
                data[i] = (chord + surge + high) * env * 0.74f;
            }
            return MakeClip("SFX_Moon5_BridgeIgnition", data);
        }

        static AudioClip GenMoon5ThorneRadioStatic()
        {
            int len = Samples(1.9f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = (1f - t * 0.6f);
                float crackle = FilteredNoise(i, 1350f) * (0.55f + Mathf.Sin(t * 47f) * 0.35f) * env;
                float carrier = Sine(i, 180f) * 0.12f + Sine(i, 390f) * 0.08f; // voice carrier hint
                float hiss = FilteredNoise(i, 4200f) * 0.04f * env;
                float pop = (Random.value < 0.018f ? 0.85f : 0f) * env;
                data[i] = (crackle + carrier + hiss + pop) * 0.48f;
            }
            return MakeClip("SFX_Moon5_ThorneRadioStatic", data);
        }

        static AudioClip GenMoon5TuningRise()
        {
            int len = Samples(0.9f);
            var data = new float[len];
            float b = F_HARMONIC;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI);
                float progress = t;
                float vib = Mathf.Sin(t * 9f) * (1.5f - progress);
                float v = Sine(i, b * (0.7f + progress * 1.1f) + vib) * 0.48f
                        + Sine(i, b * 1.5f * (0.85f + progress * 0.6f)) * 0.29f
                        + Sine(i, 528f + progress * 40f) * 0.18f;
                float bloom = FilteredNoise(i, 1650f) * 0.07f * (0.4f + progress);
                data[i] = (v + bloom) * env * 0.6f;
            }
            return MakeClip("SFX_Moon5_TuningRise", data);
        }

        // ═══════════════════════════════════════════════
        // Moon 1 Echohaven Generators — 432Hz + PHI family for magical first-zone wonder
        // Buried hums, stingers, Milo reactions, drones, F310 tones. Seamless loops where noted.
        // ═══════════════════════════════════════════════

        /// <summary>Deep buried resonance hum for first excavation site — low fundamentals + rich 432/PHI overtones, slow alive modulation. 6s loop.</summary>
        static AudioClip GenMoon1BuriedResonanceHum()
        {
            int len = Samples(6.0f);
            var data = new float[len];
            float fBase = 108f;
            float f432 = F_HARMONIC;
            float fPHI = F_HARMONIC * PHI * 0.5f; // ~349
            float fHigh = 648f;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float pulse = 0.88f + 0.12f * Mathf.Sin(2f * Mathf.PI * 0.09f * t);
                float vib = Mathf.Sin(t * 2.7f) * 1.2f;
                float s = Sine(i, fBase) * 0.32f * pulse
                        + Sine(i, fBase * 1.5f) * 0.18f * pulse
                        + Sine(i, f432 + vib) * 0.29f
                        + Sine(i, fPHI + vib * 0.6f) * 0.17f
                        + Sine(i, fHigh) * 0.11f * (0.7f + 0.3f * Mathf.Sin(t * 5.1f));
                float shimmer = FilteredNoise(i, 1850f) * 0.025f * pulse;
                float env = 0.95f + 0.05f * Mathf.Sin(2f * Mathf.PI * 0.045f * t);
                data[i] = (s + shimmer) * env * 0.38f;
            }
            return MakeClip("SFX_Moon1_BuriedResonanceHum", data);
        }

        /// <summary>First scan entry stinger — magical 432 + PHI family rising chord for "SCAN HERE" moment on FTUE trigger. Short bloom.</summary>
        static AudioClip GenMoon1ScanStinger()
        {
            int len = Samples(0.95f);
            var data = new float[len];
            float[] notes = { 216f, F_HARMONIC, F_HARMONIC * PHI, F_HEALING, 648f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 0.96f) * (1f - t * 0.22f);
                float v = 0f;
                for (int k = 0; k < notes.Length; k++)
                {
                    float phase = t * (2.2f + k * 0.6f);
                    v += Sine(i, notes[k] + Mathf.Sin(phase) * 1.8f) * (0.28f - k * 0.032f) * (0.6f + 0.4f * (1f - t));
                }
                float air = FilteredNoise(i, 2100f) * 0.06f * env;
                data[i] = (v + air) * env * 0.82f;
            }
            return MakeClip("SFX_Moon1_ScanStinger", data);
        }

        /// <summary>First tune success stinger — celebratory 432/PHI chord bloom + sparkle. F310-synced payoff feel for CompleteFirstTune.</summary>
        static AudioClip GenMoon1TuneSuccessStinger()
        {
            int len = Samples(1.85f);
            var data = new float[len];
            float b = F_HARMONIC;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Pow(1f - t * 0.48f, 0.7f) * (t < 0.15f ? t / 0.15f : 1f);
                float bloom = 0.7f + 0.3f * Mathf.Sin(t * 18f);
                float v = Sine(i, b) * 0.48f * bloom
                        + Sine(i, b * PHI) * 0.31f * bloom
                        + Sine(i, F_HEALING) * 0.26f * (0.85f + 0.15f * Mathf.Sin(t * 11f))
                        + Sine(i, 699f) * 0.19f
                        + Sine(i, 864f) * 0.14f * (0.6f + Mathf.Sin(t * 23f) * 0.4f);
                float sparkle = FilteredNoise(i, 2450f) * 0.09f * env * (0.5f + 0.5f * Mathf.Sin(t * 31f));
                data[i] = (v + sparkle) * env * 0.74f;
            }
            return MakeClip("SFX_Moon1_TuneSuccessStinger", data);
        }

        /// <summary>Gentle unsettling corruption drone for patches — low tritone + 432 bleed for contrast against pure resonance. 5.5s loop.</summary>
        static AudioClip GenMoon1CorruptionDrone()
        {
            int len = Samples(5.5f);
            var data = new float[len];
            float fLow = 47f;
            float fTrit = fLow * TRITONE;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float slow = 0.82f + 0.18f * Mathf.Sin(2f * Mathf.PI * 0.065f * t);
                float s = Sine(i, fLow) * 0.35f * slow
                        + Sine(i, fTrit) * 0.21f * slow
                        + Sine(i, 108f) * 0.12f
                        + FilteredNoise(i, 85f) * 0.24f * (0.6f + 0.4f * Mathf.Sin(t * 1.9f));
                float hiss = FilteredNoise(i, 1450f) * 0.06f * (0.4f + 0.6f * slow);
                float env = 0.9f + 0.1f * Mathf.Sin(2f * Mathf.PI * 0.03f * t);
                data[i] = (s + hiss) * env * 0.48f;
            }
            return MakeClip("SFX_Moon1_CorruptionDrone", data);
        }

        /// <summary>Milo discovery / first companion reaction — warm 432-family chime with soft breath and trust glow. Triggered on tune success + IntroduceMilo.</summary>
        static AudioClip GenMoon1MiloDiscoveryReaction()
        {
            int len = Samples(2.4f);
            var data = new float[len];
            float f1 = F_HARMONIC;
            float f2 = F_HARMONIC * 1.25f;
            float f3 = F_HEALING * 0.92f;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float breath = 0.65f + 0.35f * (Mathf.Sin(t * 2.8f) * 0.5f + 0.5f);
                float vib = Mathf.Sin(t * 3.6f) * 0.9f;
                float chime = Sine(i, f1 + vib) * 0.36f * breath
                            + Sine(i, f2 + vib * 0.5f) * 0.24f * breath
                            + Sine(i, f3) * 0.19f * breath
                            + Sine(i, 864f) * 0.11f * (0.5f + 0.5f * Mathf.Sin(t * 7f));
                float warmth = FilteredNoise(i, 520f) * 0.04f * breath;
                float env = Mathf.Sin(t * Mathf.PI * 0.92f) * (1f - t * 0.18f);
                data[i] = (chime + warmth) * env * 0.55f;
            }
            return MakeClip("SFX_Moon1_MiloDiscovery", data);
        }

        /// <summary>F310 haptic-synced pure tone burst — bright clean layer for first tune success rumble confirm. Short, punchy, wondrous.</summary>
        static AudioClip GenMoon1F310SyncedTone()
        {
            int len = Samples(0.65f);
            var data = new float[len];
            float fCore = 699f; // 432 * PHI approx
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI * 1.05f) * (1f - t * 0.35f);
                float v = Sine(i, fCore) * 0.72f * env
                        + Sine(i, fCore * 1.5f) * 0.31f * env
                        + Sine(i, 432f) * 0.18f * (0.4f + 0.6f * env);
                float ring = FilteredNoise(i, 3200f) * 0.05f * env;
                data[i] = (v + ring) * 0.68f;
            }
            return MakeClip("SFX_Moon1_F310SyncedTone", data);
        }

        /// <summary>High ethereal motes / zone wind layer — soft high harmonics + shimmer for floating aether feel over Echohaven. Long loop.</summary>
        static AudioClip GenMoon1EtherealMotes()
        {
            int len = Samples(7.2f);
            var data = new float[len];
            float f1 = 648f;
            float f2 = F_HARMONIC * 1.8f;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float gust = 0.78f + 0.22f * Mathf.Sin(2f * Mathf.PI * 0.14f * t + Mathf.Sin(t * 2.3f) * 0.6f);
                float s = Sine(i, f1) * 0.21f * gust
                        + Sine(i, f2) * 0.14f * gust
                        + Sine(i, 864f) * 0.09f * (0.5f + 0.5f * Mathf.Sin(t * 4.8f));
                float air = FilteredNoise(i, 1850f) * 0.07f * gust;
                float env = 0.92f + 0.08f * Mathf.Sin(2f * Mathf.PI * 0.07f * t);
                data[i] = (s + air) * env * 0.31f;
            }
            return MakeClip("SFX_Moon1_EtherealMotes", data);
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

        // ═══════════════════════════════════════════════
        // Global Fanfares — Moon Clear + Game Complete
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Moon-clear triumphant stinger — 3-second rising 432Hz chord bloom + shimmer tail.
        /// Plays once on each moon completion to give the player a memorable payoff moment.
        /// </summary>
        static AudioClip GenMoonClearFanfare()
        {
            int len = Samples(3.0f);
            var data = new float[len];
            // Root 432, PHI fifth, healing 528, octave 864 — golden chord
            float[] freqs = { F_HARMONIC, F_HARMONIC * PHI, F_HEALING, F_HARMONIC * 2f, 648f };
            float[] amps  = { 0.38f, 0.29f, 0.24f, 0.18f, 0.14f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                // Attack 0-0.1, sustain 0.1-0.6, decay 0.6-1.0
                float env = t < 0.1f  ? t / 0.1f :
                            t < 0.6f  ? 1f :
                            Mathf.Pow(1f - (t - 0.6f) / 0.4f, 0.6f);
                float v = 0f;
                for (int k = 0; k < freqs.Length; k++)
                {
                    // Stagger entry: each harmonic blooms in 60ms later
                    float tShifted = t - k * 0.06f;
                    if (tShifted < 0f) continue;
                    float kEnv = tShifted < 0.08f ? tShifted / 0.08f : env;
                    v += Sine(i, freqs[k]) * amps[k] * kEnv;
                }
                // Sparkle shimmer
                float shimmer = FilteredNoise(i, 3200f) * 0.04f * env * Mathf.Sin(t * 44f);
                // Bell-like body bob (8 Hz tremolo on tail)
                float vib = t > 0.3f ? 0.92f + 0.08f * Mathf.Sin(2f * Mathf.PI * 8f * t) : 1f;
                data[i] = (v + shimmer) * vib * 0.72f;
            }
            return MakeClip("SFX_MoonClear", data);
        }

        /// <summary>
        /// Game-complete credits theme — 12-second layered orchestral swell.
        /// Full golden-ratio harmonic stack: 432 root, PHI harmonics, healing 528, celestial 1296.
        /// Gentle pulse, warm choir pads, rising sparkle, never harsh.
        /// </summary>
        static AudioClip GenGameCompleteCreditsTheme()
        {
            int len = Samples(12.0f);
            var data = new float[len];
            float bRoot  = F_HARMONIC;          // 432
            float bPHI   = bRoot * PHI;         // ~699
            float bHeal  = F_HEALING;            // 528
            float bCel   = F_CELESTIAL * 0.5f;  // 648
            float bOct   = bRoot * 2f;          // 864
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                // Gentle fade-in (0-2s), full body (2-10s), fade-out (10-12s)
                float masterEnv = t < (2f/12f)  ? t / (2f/12f) :
                                  t < (10f/12f) ? 1f :
                                  Mathf.Pow(1f - (t - (10f/12f)) / (2f/12f), 1.4f);
                // Slow harmonic breathing (0.07 Hz pulse)
                float breath = 0.88f + 0.12f * Mathf.Sin(2f * Mathf.PI * 0.07f * t);
                // Root pad — warm sine
                float vRoot  = Sine(i, bRoot)  * 0.34f * breath;
                // PHI fifth — slightly detuned for warmth
                float detune = 1f + 0.0008f * Mathf.Sin(t * 1.3f);
                float vPHI   = Sine(i, bPHI * detune) * 0.26f * (0.8f + 0.2f * Mathf.Sin(t * 2.1f));
                // Healing harmonic — choir-like
                float choirMod = 0.6f + 0.4f * Mathf.Sin(t * 3.7f);
                float vHeal  = Sine(i, bHeal) * 0.22f * choirMod;
                // Celestial — bright shimmer, enters at t=0.2
                float vCel   = t > 0.2f ? Sine(i, bCel) * 0.16f * (t < 0.3f ? (t-0.2f)/0.1f : 1f) : 0f;
                // Octave bell — enters at t=0.35, slow tremolo
                float vOct   = t > 0.35f ? Sine(i, bOct) * 0.11f * (0.7f + 0.3f * Mathf.Sin(t * 9.3f)) : 0f;
                // Texture: filtered noise sparkle
                float sparkle = FilteredNoise(i, 4000f) * 0.018f * masterEnv * (0.5f + 0.5f * Mathf.Sin(t * 17f));
                data[i] = (vRoot + vPHI + vHeal + vCel + vOct + sparkle) * masterEnv * 0.68f;
            }
            return MakeClip("SFX_GameCompleteCredits", data);
        }

        // ═══════════════════════════════════════════════
        // Moon 6 Generators (Rhythmic Moon — Sunken Cathedral Organ Symphony)
        // Pipe organ tones, hydraulic fountains, Cymatic Requiem, Lirael choir, bell tolls
        // ═══════════════════════════════════════════════

        /// <summary>Broken melody — distorted organ playing backwards. Dissonant tritone + reversed 432 harmony.</summary>
        static AudioClip GenMoon6BrokenMelody()
        {
            int len = Samples(8.5f);
            var data = new float[len];
            float[] melody = { 432f, 486f, 432f * TRITONE, 324f, 648f }; // Broken intervals
            for (int i = 0; i < len; i++)
            {
                float t = 1f - ((float)i / len); // Reverse time
                int noteIdx = (int)(t * melody.Length) % melody.Length;
                float env = 0.4f + 0.3f * Mathf.Sin(t * 7f); // Erratic volume
                float organ = Sine(i, melody[noteIdx]) * 0.35f
                            + Sine(i, melody[noteIdx] * 2f) * 0.18f  // 2nd harmonic
                            + Sine(i, melody[noteIdx] * 3f) * 0.10f; // 3rd harmonic
                float distortion = FilteredNoise(i, 120f) * 0.12f;
                data[i] = (organ + distortion) * env * 0.55f;
            }
            return MakeClip("SFX_Moon6_BrokenMelody", data);
        }

        /// <summary>Pipe repair harmonic chime — bright 432Hz + PHI overtones, crystal ring.</summary>
        static AudioClip GenMoon6PipeRepair()
        {
            int len = Samples(1.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.3f);
                float tone = Sine(i, F_HARMONIC) * 0.42f
                           + Sine(i, F_HARMONIC * PHI) * 0.28f
                           + Sine(i, F_HEALING) * 0.19f
                           + Sine(i, 1296f * 0.5f) * 0.12f;
                float ring = FilteredNoise(i, 3500f) * 0.05f * env;
                data[i] = (tone + ring) * env * 0.65f;
            }
            return MakeClip("SFX_Moon6_PipeRepair", data);
        }

        /// <summary>Single organ pipe tone — procedural 12-note chromatic organ scale (C4 to B4). Call with pipe index 0-11.</summary>
        static AudioClip GenMoon6OrganTone()
        {
            // Generate middle C (261.63 Hz) with organ overtones
            int len = Samples(3.0f);
            var data = new float[len];
            float fundamental = 261.63f; // C4
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Clamp01(t * 10f) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.7f) / 0.3f, 1.5f));
                // Organ overtones: strong 2nd, 3rd, 5th harmonics
                float organ = Sine(i, fundamental) * 0.40f
                            + Sine(i, fundamental * 2f) * 0.30f
                            + Sine(i, fundamental * 3f) * 0.18f
                            + Sine(i, fundamental * 4f) * 0.10f
                            + Sine(i, fundamental * 5f) * 0.08f;
                data[i] = organ * env * 0.60f;
            }
            return MakeClip("SFX_Moon6_OrganTone", data);
        }

        /// <summary>Hydraulic fountain flow — water babble + mechanical bellows whoosh.</summary>
        static AudioClip GenMoon6FountainFlow()
        {
            int len = Samples(4.2f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float flow = Mathf.Abs(Mathf.Sin(t * 8f + Mathf.Sin(t * 2.1f) * 1.4f));
                float water = FilteredNoise(i, 850f) * flow * 0.32f;
                float bellows = Sine(i, 54f) * 0.18f + Sine(i, 108f) * 0.12f;
                float env = 0.85f + 0.15f * Mathf.Sin(t * 3.5f);
                data[i] = (water + bellows) * env * 0.48f;
            }
            return MakeClip("SFX_Moon6_FountainFlow", data);
        }

        /// <summary>Cymatic Requiem climax — full organ symphony with 432Hz golden cascade, 6-second swell.</summary>
        static AudioClip GenMoon6CymaticRequiem()
        {
            int len = Samples(8.5f);
            var data = new float[len];
            float[] chord = { 324f, F_HARMONIC, F_HEALING, 648f, 864f, 1296f }; // Cathedral chord
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float swell = Mathf.Clamp01(t * 2.5f) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.7f) / 0.3f, 1.2f));
                float v = 0f;
                for (int k = 0; k < chord.Length; k++)
                {
                    float delay = k * 0.08f;
                    float localT = Mathf.Max(0f, t - delay);
                    float kEnv = localT < 0.15f ? localT / 0.15f : 1f;
                    v += Sine(i, chord[k]) * (0.20f - k * 0.018f) * kEnv;
                }
                float thunder = FilteredNoise(i, 60f) * 0.15f * swell;
                data[i] = (v + thunder) * swell * 0.70f;
            }
            return MakeClip("SFX_Moon6_CymaticRequiem", data);
        }

        /// <summary>Lirael choir hum — spectral children's voices, 432Hz lullaby layer.</summary>
        static AudioClip GenMoon6LiraelChoir()
        {
            int len = Samples(5.5f);
            var data = new float[len];
            float[] voices = { 324f, F_HARMONIC * 0.9f, F_HARMONIC, F_HEALING * 0.85f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float breath = 0.75f + 0.25f * Mathf.Sin(t * 2.8f);
                float v = 0f;
                for (int k = 0; k < voices.Length; k++)
                {
                    float vibrato = Mathf.Sin(t * 5.2f + k) * 1.2f;
                    v += Sine(i, voices[k] + vibrato) * (0.18f - k * 0.02f) * breath;
                }
                float whisper = FilteredNoise(i, 1800f) * 0.04f * breath;
                float env = Mathf.Sin(t * Mathf.PI) * 0.95f;
                data[i] = (v + whisper) * env * 0.52f;
            }
            return MakeClip("SFX_Moon6_LiraelChoir", data);
        }

        /// <summary>Cathedral bell toll — deep 486Hz (Moon 2 bell family) with long decay, 8-second ring.</summary>
        static AudioClip GenMoon6BellToll()
        {
            int len = Samples(8.0f);
            var data = new float[len];
            float fundamental = 486f; // Bell root (Moon 2 family)
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float strike = t < 0.02f ? (1f - t / 0.02f) * 0.8f : 0f;
                float decay = Mathf.Exp(-t * 2.2f);
                float warble = 0.98f + 0.02f * Mathf.Sin(t * 7.3f);
                float bell = (Sine(i, fundamental) * 0.48f
                            + Sine(i, fundamental * 2.03f) * 0.26f
                            + Sine(i, fundamental * 3.1f) * 0.14f
                            + Sine(i, fundamental * 5.2f) * 0.08f) * warble;
                data[i] = (bell + strike * FilteredNoise(i, 400f)) * decay * 0.62f;
            }
            return MakeClip("SFX_Moon6_BellToll", data);
        }

        /// <summary>Crystal pipe chime — high 1296Hz celestial ping with shimmer tail.</summary>
        static AudioClip GenMoon6CrystalChime()
        {
            int len = Samples(2.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float decay = Mathf.Exp(-t * 3.5f);
                float chime = Sine(i, F_CELESTIAL) * 0.45f
                            + Sine(i, F_CELESTIAL * 1.5f) * 0.22f
                            + Sine(i, F_CELESTIAL * 2f) * 0.12f;
                float shimmer = FilteredNoise(i, 4200f) * 0.08f * decay;
                data[i] = (chime + shimmer) * decay * 0.58f;
            }
            return MakeClip("SFX_Moon6_CrystalChime", data);
        }

        /// <summary>Hydraulic bellows breathing — deep mechanical pulse, 54Hz sub-bass + air whoosh.</summary>
        static AudioClip GenMoon6HydraulicBellows()
        {
            int len = Samples(3.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float cycle = Mathf.Sin(t * Mathf.PI * 2f);
                float inhale = Mathf.Clamp01(cycle);
                float exhale = Mathf.Clamp01(-cycle);
                float sub = Sine(i, 54f) * 0.28f * inhale;
                float air = FilteredNoise(i, 650f) * 0.35f * exhale;
                float creak = FilteredNoise(i, 180f) * 0.12f * Mathf.Abs(cycle);
                data[i] = (sub + air + creak) * 0.65f;
            }
            return MakeClip("SFX_Moon6_HydraulicBellows", data);
        }

        /// <summary>Cathedral ambient loop — deep cave reverb, distant water drips, 432Hz undertone.</summary>
        static AudioClip GenMoon6CathedralAmbience()
        {
            int len = Samples(12.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float cave = FilteredNoise(i, 90f) * 0.22f * (0.85f + 0.15f * Mathf.Sin(t * 1.3f));
                float drone = Sine(i, F_HARMONIC * 0.25f) * 0.12f;
                float drip = (Random.value < 0.005f ? 0.4f : 0f) * FilteredNoise(i, 1800f);
                float wind = FilteredNoise(i, 420f) * 0.08f * (0.7f + 0.3f * Mathf.Sin(t * 2.1f));
                data[i] = (cave + drone + drip + wind) * 0.35f;
            }
            return MakeClip("SFX_Moon6_CathedralAmbience", data);
        }

        /// <summary>Ionized mist rain — cyan particles falling, soft electric crackle + water patter.</summary>
        static AudioClip GenMoon6IonicMistRain()
        {
            int len = Samples(10.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float rain = FilteredNoise(i, 1200f) * 0.28f * (0.8f + 0.2f * Mathf.Sin(t * 5.7f));
                float crackle = FilteredNoise(i, 3500f) * (Random.value < 0.12f ? 0.15f : 0.03f);
                float hum = Sine(i, F_HEALING * 0.6f) * 0.08f;
                data[i] = (rain + crackle + hum) * 0.42f;
            }
            return MakeClip("SFX_Moon6_IonicMistRain", data);
        }

        // ═══════════════════════════════════════════════
        // Moon 7 Generators (Resonant Moon — Korath Awakening + Giant Stasis Vault)
        // Ice thaw, 9-band aurora, Korath voice, golem siege, Cassian tension, harmonic cutting
        // ═══════════════════════════════════════════════

        /// <summary>Ice thaw crackle — sharp cracks + melt drips, violet energy dispersing.</summary>
        static AudioClip GenMoon7IceThaw()
        {
            int len = Samples(3.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float crack = (Random.value < 0.08f ? 0.65f : 0f) * FilteredNoise(i, 2800f);
                float melt = FilteredNoise(i, 950f) * 0.18f * (t + 0.3f);
                float energy = Sine(i, 699f) * 0.12f * Mathf.Exp(-t * 2f); // PHI freq
                data[i] = (crack + melt + energy) * 0.58f;
            }
            return MakeClip("SFX_Moon7_IceThaw", data);
        }

        /// <summary>9-band aurora hum — violet 432*φ² = ~1130Hz carrier with 7.83Hz modulation.</summary>
        static AudioClip GenMoon7AuroraHum()
        {
            int len = Samples(8.0f);
            var data = new float[len];
            float carrier = F_HARMONIC * PHI * PHI; // 9-band frequency
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float modulation = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * F_TELLURIC * t);
                float aurora = Sine(i, carrier) * 0.35f * modulation;
                float sub = Sine(i, carrier * 0.5f) * 0.18f;
                float shimmer = FilteredNoise(i, 2100f) * 0.06f * modulation;
                data[i] = (aurora + sub + shimmer) * 0.48f;
            }
            return MakeClip("SFX_Moon7_AuroraHum", data);
        }

        /// <summary>Korath voice rumble — deep 60Hz sub-bass with harmonic overtones, giant resonance.</summary>
        static AudioClip GenMoon7KorathVoiceRumble()
        {
            int len = Samples(4.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * 0.95f;
                float rumble = Sine(i, 60f) * 0.48f
                             + Sine(i, 120f) * 0.22f
                             + Sine(i, 180f) * 0.12f;
                float growl = FilteredNoise(i, 240f) * 0.18f;
                data[i] = (rumble + growl) * env * 0.72f;
            }
            return MakeClip("SFX_Moon7_KorathVoice", data);
        }

        /// <summary>Korath awakening — ice shattering + golden 432Hz surge, giant stands.</summary>
        static AudioClip GenMoon7KorathAwakening()
        {
            int len = Samples(6.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float shatter = t < 0.15f ? FilteredNoise(i, 3200f) * (0.15f - t) / 0.15f * 0.75f : 0f;
                float rise = Mathf.Clamp01((t - 0.15f) * 2f);
                float surge = Sine(i, F_HARMONIC * (0.5f + rise * 0.8f)) * 0.42f * rise
                            + Sine(i, F_HEALING) * 0.28f * rise
                            + Sine(i, 1296f * 0.5f) * 0.18f * rise;
                float giant = Sine(i, 54f) * 0.32f * rise;
                float env = 1f - Mathf.Pow(Mathf.Max(0f, t - 0.75f) / 0.25f, 1.5f);
                data[i] = (shatter + surge + giant) * env * 0.78f;
            }
            return MakeClip("SFX_Moon7_KorathAwakening", data);
        }

        /// <summary>Golem siege bass — massive 40Hz impacts + 80Hz rumble, war drums.</summary>
        static AudioClip GenMoon7GolemSiegeBass()
        {
            int len = Samples(12.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                // Impact every 1.5 seconds
                float impactCycle = (t * 8f) % 1f;
                float impact = impactCycle < 0.1f ? Mathf.Exp(-impactCycle * 30f) * 0.85f : 0f;
                float rumble = Sine(i, 40f) * 0.38f + Sine(i, 80f) * 0.22f;
                float war = FilteredNoise(i, 160f) * 0.28f * (0.7f + 0.3f * Mathf.Sin(t * 11f));
                data[i] = (impact * FilteredNoise(i, 600f) + rumble + war) * 0.68f;
            }
            return MakeClip("SFX_Moon7_GolemSiege", data);
        }

        /// <summary>Cassian confrontation tension — dissonant 432*tritone dread, minor seconds.</summary>
        static AudioClip GenMoon7CassianTension()
        {
            int len = Samples(5.2f);
            var data = new float[len];
            float root = F_HARMONIC;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float pulse = 0.8f + 0.2f * Mathf.Sin(t * 4.3f);
                float tension = Sine(i, root * TRITONE) * 0.32f * pulse
                              + Sine(i, root * TRITONE * 0.5f) * 0.18f
                              + Sine(i, root * 1.059f) * 0.22f; // Minor 2nd
                float dread = FilteredNoise(i, 110f) * 0.15f;
                data[i] = (tension + dread) * 0.55f;
            }
            return MakeClip("SFX_Moon7_CassianTension", data);
        }

        /// <summary>Harmonic rock cutting — 432Hz + PHI saw through stone, crystal precision.</summary>
        static AudioClip GenMoon7HarmonicRockCutting()
        {
            int len = Samples(2.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float cutting = FilteredNoise(i, 1800f) * 0.42f * (0.9f + 0.1f * Mathf.Sin(t * 23f));
                float harmonic = Sine(i, F_HARMONIC) * 0.28f + Sine(i, F_HARMONIC * PHI) * 0.18f;
                float grind = FilteredNoise(i, 350f) * 0.22f;
                data[i] = (cutting + harmonic + grind) * 0.65f;
            }
            return MakeClip("SFX_Moon7_HarmonicCutting", data);
        }

        /// <summary>Korath sacrifice — golden light surge, 1296Hz celestial bloom, giant fades.</summary>
        static AudioClip GenMoon7KorathSacrifice()
        {
            int len = Samples(9.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float swell = Mathf.Clamp01(t * 3f) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.6f) / 0.4f, 1.8f));
                float golden = Sine(i, F_CELESTIAL) * 0.45f * swell
                             + Sine(i, F_HEALING) * 0.32f * swell
                             + Sine(i, F_HARMONIC * 2f) * 0.22f * swell;
                float giant = Sine(i, 60f) * 0.28f * (1f - t * 0.8f);
                float celestial = FilteredNoise(i, 3800f) * 0.08f * swell;
                data[i] = (golden + giant + celestial) * 0.75f;
            }
            return MakeClip("SFX_Moon7_KorathSacrifice", data);
        }

        /// <summary>Stasis vault ambience — deep sub-bass 30Hz, violet aurora whisper, ice wind.</summary>
        static AudioClip GenMoon7StasisVaultAmbience()
        {
            int len = Samples(15.0f);
            var data = new float[len];
            float carrier = F_HARMONIC * PHI * PHI;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float sub = Sine(i, 30f) * 0.18f * (0.85f + 0.15f * Mathf.Sin(t * 0.8f));
                float aurora = Sine(i, carrier) * 0.12f * (0.7f + 0.3f * Mathf.Sin(t * 1.5f));
                float wind = FilteredNoise(i, 280f) * 0.15f * (0.75f + 0.25f * Mathf.Sin(t * 2.3f));
                float ice = FilteredNoise(i, 2400f) * 0.04f * (Random.value < 0.15f ? 1.5f : 0.6f);
                data[i] = (sub + aurora + wind + ice) * 0.38f;
            }
            return MakeClip("SFX_Moon7_StasisAmbience", data);
        }

        /// <summary>9-band unlock stinger — PHI² frequency cascade, anti-gravity surge.</summary>
        static AudioClip GenMoon79BandUnlock()
        {
            int len = Samples(3.2f);
            var data = new float[len];
            float[] cascade = { F_HARMONIC * PHI * PHI, F_HEALING * PHI, F_CELESTIAL * 0.8f, 1620f };
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.25f);
                float v = 0f;
                for (int k = 0; k < cascade.Length; k++)
                {
                    float delay = k * 0.15f;
                    float localT = Mathf.Max(0f, t - delay);
                    if (localT > 0.01f)
                        v += Sine(i, cascade[k]) * (0.22f - k * 0.03f) * (localT < 0.12f ? localT / 0.12f : 1f);
                }
                float sparkle = FilteredNoise(i, 4500f) * 0.06f * env;
                data[i] = (v + sparkle) * env * 0.68f;
            }
            return MakeClip("SFX_Moon7_9BandUnlock", data);
        }

        /// <summary>Violet pulse — 9-band energy throb, 7.83Hz Schumann modulation on PHI² carrier.</summary>
        static AudioClip GenMoon7VioletPulse()
        {
            int len = Samples(2.4f);
            var data = new float[len];
            float carrier = F_HARMONIC * PHI * PHI;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float modulation = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * F_TELLURIC * t);
                float pulse = Sine(i, carrier) * 0.42f * modulation;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = pulse * env * 0.58f;
            }
            return MakeClip("SFX_Moon7_VioletPulse", data);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // MOON 4 GENERATORS — Resonance Moon (Water Temple + Golem Puzzle)
        // ═══════════════════════════════════════════════════════════════════════════

        static AudioClip GenMoon4BastionSnap()
        {
            // Sharp resonance lock — tuning snap when frequency matches
            int len = Samples(0.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Exp(-6f * t);
                data[i] = env * 0.5f * (Sine(i, F_HARMONIC) + 0.4f * Sine(i, F_HEALING));
            }
            return MakeClip("SFX_Moon4_BastionSnap", data);
        }

        static AudioClip GenMoon4WaterFlowRestoration()
        {
            // Water channels reactivate — flowing bubbles + harmonic shimmer
            int len = Samples(2.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float bubbles = FilteredNoise(i, 800f + t * 400f) * 0.3f;
                float flow = Sine(i, 220f + t * 80f) * 0.2f;
                float env = Mathf.Clamp01(t * 2f) * (1f - Mathf.Pow(Mathf.Max(0f, t - 0.7f) / 0.3f, 2));
                data[i] = (bubbles + flow) * env * 0.45f;
            }
            return MakeClip("SFX_Moon4_WaterFlow", data);
        }

        static AudioClip GenMoon4GolemRoar()
        {
            // Stone giant awakens — deep bass rumble + earth cracking
            int len = Samples(2.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float bass = Sine(i, 40f + t * 30f) * 0.6f;
                float rumble = FilteredNoise(i, 120f) * 0.4f;
                float env = Mathf.Sin(t * Mathf.PI) * 0.9f;
                data[i] = (bass + rumble) * env;
            }
            return MakeClip("SFX_Moon4_GolemRoar", data);
        }

        static AudioClip GenMoon4BellTowerHarmony()
        {
            // Bell tower rings in harmony — golden waves spreading
            int len = Samples(3.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float bell = Sine(i, F_HARMONIC) * 0.4f + Sine(i, F_HARMONIC * PHI) * 0.25f;
                float waves = Sine(i, F_HEALING) * 0.2f;
                float env = Mathf.Exp(-1.2f * t);
                data[i] = (bell + waves) * env * 0.5f;
            }
            return MakeClip("SFX_Moon4_BellTowerWaves", data);
        }

        static AudioClip GenMoon4MemoryCrystal()
        {
            // Memory crystal activates — warm chime + shimmer
            int len = Samples(1.8f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float chime = Sine(i, F_HEALING) * 0.35f + Sine(i, F_HEALING * PHI) * 0.2f;
                float shimmer = Sine(i, F_CELESTIAL * 0.5f) * 0.15f;
                float env = Mathf.Exp(-2f * t);
                data[i] = (chime + shimmer) * env * 0.45f;
            }
            return MakeClip("SFX_Moon4_MemoryCrystal", data);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // MOONS 8-13 GENERATORS — Late Game (Continental Scale + Boss Encounters)
        // ═══════════════════════════════════════════════════════════════════════════

        static AudioClip GenMoon8AirshipLaunch()
        {
            // Airship propulsion — rising whoosh + harmonic thrust
            int len = Samples(3.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(80f, F_HARMONIC, t);
                float whoosh = FilteredNoise(i, 600f + t * 800f) * 0.5f;
                float thrust = Sine(i, freq) * 0.35f;
                float env = Mathf.Clamp01(t * 1.5f);
                data[i] = (whoosh + thrust) * env * 0.6f;
            }
            return MakeClip("SFX_Moon8_AirshipLaunch", data);
        }

        static AudioClip GenMoon9RailNetworkHum()
        {
            // Continental rail network ambient — deep harmonic hum
            int len = Samples(5.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float hum = Sine(i, F_HARMONIC * 0.5f) * 0.3f;
                float overtone = Sine(i, F_HARMONIC) * 0.15f;
                float modulation = 1f + 0.2f * Mathf.Sin(t * Mathf.PI * 4f);
                data[i] = (hum + overtone) * modulation * 0.4f;
            }
            return MakeClip("SFX_Moon9_RailHum", data);
        }

        static AudioClip GenMoon10LeviathanRoar()
        {
            // Massive creature roar — layered bass + screech
            int len = Samples(4.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float bass = Sine(i, 30f + t * 50f) * 0.7f;
                float mid = Sine(i, 200f + t * 300f) * 0.4f;
                float high = FilteredNoise(i, 2000f + t * 1000f) * 0.3f;
                float env = Mathf.Sin(t * Mathf.PI) * 0.95f;
                data[i] = (bass + mid + high) * env;
            }
            return MakeClip("SFX_Moon10_LeviathanRoar", data);
        }

        static AudioClip GenMoon10LeviathanDeath()
        {
            // Creature dissolves — falling pitch + dissipating energy
            int len = Samples(5.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(400f, 60f, t);
                float dissolve = Sine(i, freq) * 0.6f;
                float energy = FilteredNoise(i, Mathf.Lerp(1200f, 200f, t)) * 0.4f;
                float env = (1f - t) * (1f - t);
                data[i] = (dissolve + energy) * env * 0.7f;
            }
            return MakeClip("SFX_Moon10_LeviathanDeath", data);
        }

        static AudioClip GenMoon11AquiferPurification()
        {
            // Corrupt water purifies — dark red to crystal blue sonic shift
            int len = Samples(4.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float darkFreq = 180f * TRITONE; // dissonant start
                float pureFreq = F_HEALING; // healing end
                float freq = Mathf.Lerp(darkFreq, pureFreq, t);
                float purify = Sine(i, freq) * 0.45f;
                float bubble = FilteredNoise(i, 600f + t * 400f) * 0.25f;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = (purify + bubble) * env * 0.5f;
            }
            return MakeClip("SFX_Moon11_AquiferPurification", data);
        }

        static AudioClip GenMoon11FountainChainActivation()
        {
            // Fountain chain lights up — cascading chimes
            int len = Samples(3.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float chime1 = Sine(i, F_HARMONIC) * 0.3f;
                float chime2 = Sine(i, F_HEALING) * 0.25f;
                float cascade = Sine(i, F_CELESTIAL * 0.5f) * 0.2f * Mathf.Clamp01(t * 3f);
                float env = 1f - Mathf.Pow(t, 1.5f);
                data[i] = (chime1 + chime2 + cascade) * env * 0.45f;
            }
            return MakeClip("SFX_Moon11_FountainChainActivation", data);
        }

        static AudioClip GenMoon12BellTowerSync()
        {
            // Bell towers synchronize — harmonic convergence
            int len = Samples(2.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float bell1 = Sine(i, F_HARMONIC) * 0.35f;
                float bell2 = Sine(i, F_HARMONIC * PHI) * 0.3f;
                float sync = Sine(i, F_HEALING) * 0.25f * Mathf.Clamp01((t - 0.4f) * 3f);
                float env = Mathf.Exp(-1f * t);
                data[i] = (bell1 + bell2 + sync) * env * 0.5f;
            }
            return MakeClip("SFX_Moon12_BellTowerSync", data);
        }

        static AudioClip GenMoon12TowerHarmonyRing()
        {
            // Bell tower full harmony — sustained golden ring
            int len = Samples(6.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float fundamental = Sine(i, F_HARMONIC) * 0.4f;
                float overtone1 = Sine(i, F_HARMONIC * 2f) * 0.2f;
                float overtone2 = Sine(i, F_HARMONIC * PHI) * 0.25f;
                float env = Mathf.Exp(-0.8f * t);
                data[i] = (fundamental + overtone1 + overtone2) * env * 0.45f;
            }
            return MakeClip("SFX_Moon12_TowerHarmony", data);
        }

        static AudioClip GenMoon13AetherTremor()
        {
            // Reality trembles — unstable dimensional shift
            int len = Samples(3.5f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = F_HARMONIC + Mathf.Sin(t * Mathf.PI * 8f) * 120f; // warbling
                float tremor = Sine(i, freq) * 0.45f;
                float distortion = FilteredNoise(i, 400f + t * 600f) * 0.3f;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = (tremor + distortion) * env * 0.55f;
            }
            return MakeClip("SFX_Moon13_AetherTremor", data);
        }

        static AudioClip GenMoon13SeismicTremor()
        {
            // Ground quake — deep bass rumble + shockwave
            int len = Samples(4.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float bass = Sine(i, 25f + t * 35f) * 0.7f;
                float rumble = FilteredNoise(i, 80f + t * 120f) * 0.5f;
                float shock = Sine(i, F_TELLURIC) * 0.3f; // Schumann resonance
                float env = Mathf.Sin(t * Mathf.PI) * 0.9f;
                data[i] = (bass + rumble + shock) * env;
            }
            return MakeClip("SFX_Moon13_SeismicTremor", data);
        }

        static AudioClip GenBossPhaseTransition()
        {
            // Boss enters new phase — ominous surge
            int len = Samples(2.0f);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float surge = Sine(i, Mathf.Lerp(60f, 240f, t)) * 0.6f;
                float impact = FilteredNoise(i, 600f) * 0.4f * Mathf.Exp(-3f * t);
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = (surge + impact) * env * 0.65f;
            }
            return MakeClip("SFX_BossPhaseTransition", data);
        }
    }
}
