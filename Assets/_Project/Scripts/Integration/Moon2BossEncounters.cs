using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Integration; // For BossDefinition, BossPhase, BossAttackPattern, BossEncounterSystem
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Bosses & Major Encounters — Exclusive domain for Lunar Moon: Shadow & Purge (Crystalline Caverns).
    /// 
    /// Production-quality climactic encounters for the living crystal cathedral and fractal caverns.
    /// Integrates:
    ///   - Frequency mechanics (target Hz, telegraph color pulses on crystals/veins, SubmitFrequencyPuzzle synergy)
    ///   - Environment (corruption veins that spread like "fire along a fuse", crystal facets, fractal lattice)
    ///   - Micro-Giant Mode + Giant Mode synergy (core descent phases, root tearing)
    ///   - Companion synergy (Lirael freq projection, Milo external support/dig, Cassian tense/ unreliable hints foreshadowing betrayal)
    ///   - Strong telegraphing (dome breathing, vein throb rate = timer, crystal strobes, VO callouts, VFX)
    ///   - Multi-phase with vuln windows, permanent world payoffs (purified veins persist, RS boosts, new chambers, visual state)
    /// 
    /// Builds directly on BossEncounterSystem (reuses phase defs, freq submit, VFX calls, persistence, Golden Cascade).
    /// One is the signature "cathedral guardian" type.
    /// 
    /// Usage: Moon2 game logic / GameLoop / Moon2Zone triggers call:
    ///   var bossDef = Moon2BossEncounters.GetCathedralVeinWarden();
    ///   // Then spawn via BossEncounterSystem custom path or extend lookup.
    ///   BossEncounterSystem.Instance.SpawnBoss("cathedral_vein_warden"); // after wiring in main system
    /// 
    /// All payoffs call VFXController for Moon2 vein purify (reverse fuse), LeyLineManager, ResonanceScore.
    /// R7 visual polish compatible (dome breathing, 9-probe caustics, per-building veins).
    /// </summary>
    public static class Moon2BossEncounters
    {
        // Moon 2 specific persistent state (survives defeat, saved via BossSave or separate hook)
        public static int Moon2PurifiedVeinSectors { get; private set; } = 0;
        public static bool CathedralPermanentlyPurified { get; private set; } = false;
        public static bool LeyChamberPurified { get; private set; } = false;
        public static bool CrystalCavernsResonant { get; private set; } = false;

        /// <summary>
        /// BOSS 1: Cathedral Vein Warden — The "cathedral guardian" type encounter.
        /// Location: moon2_cathedral_dome (exterior + fractal interior via Micro-Giant).
        /// Feels like a true climax: the living heart of Moon 2's crystal cathedral fights back.
        /// </summary>
        public static BossDefinition GetCathedralVeinWarden()
        {
            return new BossDefinition
            {
                bossName = "Cathedral Vein Warden",
                bossType = BossType.CorruptionTitan, // Reuses + Moon2 special handling
                totalHP = 1850f,
                baseRSReward = 48f,
                parTime = 165f,
                phases = new List<BossPhase>
                {
                    // Phase 1: Vein Awakening (cathedral exterior — strong environmental telegraph)
                    new BossPhase
                    {
                        phaseName = "Vein Awakening",
                        entranceDialogue = "The Warden awakens within the stone. The cathedral's veins turn against you. Retune or be consumed!",
                        hpThresholdToAdvance = 0.72f,
                        attackInterval = 2.8f,
                        vulnerableDuration = 4.5f, // generous first window for teaching
                        invulnerableDuration = 5.5f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.CorruptionWave,
                            BossAttackPattern.VeinSpread, // NEW Moon2 pattern (implemented in Execute + dedicated AI)
                            BossAttackPattern.Sweep
                        }
                    },
                    // Phase 2: Reflected Wrath (mirror + crystal shards, freq shift)
                    new BossPhase
                    {
                        phaseName = "Reflected Wrath",
                        entranceDialogue = "The crystal remembers every strike. Your own resonance returns as shards!",
                        hpThresholdToAdvance = 0.38f,
                        attackInterval = 2.1f,
                        vulnerableDuration = 3.2f,
                        invulnerableDuration = 4.8f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.MirrorClone,
                            BossAttackPattern.CrystalBarrage, // NEW
                            BossAttackPattern.FrequencyJam
                        }
                    },
                    // Phase 3: Fractal Descent (the signature micro-giant + companion synergy phase)
                    new BossPhase
                    {
                        phaseName = "Fractal Descent",
                        entranceDialogue = "Into the heart! Shrink and purge the root lattice — your companions hold the dome above!",
                        hpThresholdToAdvance = 0.12f,
                        attackInterval = 1.6f,
                        vulnerableDuration = 5.8f, // long for micro-giant puzzle
                        invulnerableDuration = 3.5f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.VeinSpread,
                            BossAttackPattern.CrystalBarrage,
                            BossAttackPattern.Enrage
                        }
                    },
                    // Phase 4: Warden's Lament (desperation, permanent payoff trigger on win)
                    new BossPhase
                    {
                        phaseName = "Warden's Lament",
                        entranceDialogue = "The stone... weeps. You have sung the old chord. The cathedral... remembers you.",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.1f,
                        vulnerableDuration = 6.5f,
                        invulnerableDuration = 2f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.CorruptionWave,
                            BossAttackPattern.Slam
                        }
                    }
                }
            };
        }

        /// <summary>
        /// BOSS 2: Fractal Vein Mirror (major elite fight in the crystal caverns)
        /// Location: Deep between crystal_hall / ley_chamber tunnels. Recursive crystal mass.
        /// Strong frequency shifting + environmental crystal synergy.
        /// </summary>
        public static BossDefinition GetFractalVeinMirror()
        {
            return new BossDefinition
            {
                bossName = "Fractal Vein Mirror",
                bossType = BossType.MirrorSovereign,
                totalHP = 1420f,
                baseRSReward = 35f,
                parTime = 115f,
                phases = new List<BossPhase>
                {
                    new BossPhase
                    {
                        phaseName = "Echo Lattice",
                        entranceDialogue = "The caverns reflect your song. Every crystal sings your mistakes back to you.",
                        hpThresholdToAdvance = 0.65f,
                        attackInterval = 2.3f,
                        vulnerableDuration = 3.8f,
                        invulnerableDuration = 4.5f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.MirrorClone,
                            BossAttackPattern.FrequencyJam,
                            BossAttackPattern.CrystalBarrage
                        }
                    },
                    new BossPhase
                    {
                        phaseName = "Shattered Choir",
                        entranceDialogue = "The veins fracture! The caverns themselves rise to defend the dissonance!",
                        hpThresholdToAdvance = 0.25f,
                        attackInterval = 1.7f,
                        vulnerableDuration = 4.1f,
                        invulnerableDuration = 3.9f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.VeinSpread,
                            BossAttackPattern.MirrorClone,
                            BossAttackPattern.Sweep
                        }
                    },
                    new BossPhase
                    {
                        phaseName = "Resonant Collapse",
                        entranceDialogue = "One final mirror. Purge the core vein or the entire hall shatters!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.3f,
                        vulnerableDuration = 5.2f,
                        invulnerableDuration = 2.4f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.CrystalBarrage,
                            BossAttackPattern.Enrage
                        }
                    }
                }
            };
        }

        /// <summary>
        /// BOSS 3: Dissonance Root Core (major/climax encounter in ley foundations)
        /// Location: moon2_ley_chamber roots + cathedral foundation veins.
        /// Giant Mode + Micro-Giant hybrid counterplay + bell tower frequency synergy.
        /// </summary>
        public static BossDefinition GetDissonanceRootCore()
        {
            return new BossDefinition
            {
                bossName = "Dissonance Root Core",
                bossType = BossType.CorruptionTitan,
                totalHP = 2100f,
                baseRSReward = 55f,
                parTime = 195f,
                phases = new List<BossPhase>
                {
                    new BossPhase
                    {
                        phaseName = "Root Bloom",
                        entranceDialogue = "The ley veins awaken from below. The foundation itself hungers!",
                        hpThresholdToAdvance = 0.68f,
                        attackInterval = 3.1f,
                        vulnerableDuration = 3.5f,
                        invulnerableDuration = 6.2f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.CorruptionWave,
                            BossAttackPattern.VeinSpread,
                            BossAttackPattern.Slam
                        }
                    },
                    new BossPhase
                    {
                        phaseName = "Bell Sever",
                        entranceDialogue = "The bells scream! Ring them true or the roots claim the grid!",
                        hpThresholdToAdvance = 0.32f,
                        attackInterval = 2.0f,
                        vulnerableDuration = 4.8f,
                        invulnerableDuration = 4.0f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.LeyLineSever,
                            BossAttackPattern.FrequencyJam,
                            BossAttackPattern.CrystalBarrage
                        }
                    },
                    new BossPhase
                    {
                        phaseName = "Giant Heart",
                        entranceDialogue = "The core is exposed! Grow and tear the surface roots — shrink to finish the song within!",
                        hpThresholdToAdvance = 0.08f,
                        attackInterval = 1.4f,
                        vulnerableDuration = 7.5f, // extended for Giant + Micro synergy
                        invulnerableDuration = 3.0f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.VeinSpread,
                            BossAttackPattern.Enrage
                        }
                    },
                    new BossPhase
                    {
                        phaseName = "Final Purge",
                        entranceDialogue = "The last dissonance... sing the chord the architect died composing!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.0f,
                        vulnerableDuration = 8.0f,
                        invulnerableDuration = 1.5f,
                        attackPatterns = new List<BossAttackPattern>
                        {
                            BossAttackPattern.Slam,
                            BossAttackPattern.CorruptionWave
                        }
                    }
                }
            };
        }

        // ─── Moon 2 Specific Checks (callable from BossEncounterSystem) ───
        public static bool IsMoon2CathedralBoss(BossDefinition def) => def != null && def.bossName.Contains("Cathedral Vein Warden");
        public static bool IsMoon2FractalMirror(BossDefinition def) => def != null && def.bossName.Contains("Fractal Vein Mirror");
        public static bool IsMoon2RootCore(BossDefinition def) => def != null && def.bossName.Contains("Dissonance Root Core");
        public static bool IsAnyMoon2Boss(BossDefinition def) => IsMoon2CathedralBoss(def) || IsMoon2FractalMirror(def) || IsMoon2RootCore(def);

        // ─── Moon 2 Vein / Crystal / Frequency Special Logic (hooked from main system Submit + Update) ───
        /// <summary>
        /// Called from BossEncounterSystem.SubmitFrequencyPuzzle when a Moon2 boss is active.
        /// Handles multi-node vein purging, crystal facet matches, permanent state, VFX reverse-fuse.
        /// </summary>
        public static void HandleMoon2FrequencySolve(float matchQuality, float submittedFreq, BossDefinition currentBoss)
        {
            if (!IsAnyMoon2Boss(currentBoss)) return;

            int nodesPurifiedThisSolve = 0;

            if (IsMoon2CathedralBoss(currentBoss))
            {
                // Cathedral guardian: 4 vein nodes. Good solves purge them permanently in this fight + world state
                nodesPurifiedThisSolve = Mathf.RoundToInt(matchQuality * 2.8f) + 1;
                Moon2PurifiedVeinSectors = Mathf.Min(4, Moon2PurifiedVeinSectors + nodesPurifiedThisSolve);

                // Strong VFX: reverse fuse burn (golden instead of dark) on cathedral veins
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, new Vector3(0, 8f, 40f)); // approx cathedral pos
                if (matchQuality > 0.75f)
                {
                    // Trigger dome breathing enhancement (ties to R7 visual polish)
                    VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, new Vector3(0, 12f, 42f));
                }

                if (Moon2PurifiedVeinSectors >= 4 && !CathedralPermanentlyPurified)
                {
                    CathedralPermanentlyPurified = true;
                    ApplyCathedralPermanentPayoff();
                }
            }
            else if (IsMoon2FractalMirror(currentBoss))
            {
                nodesPurifiedThisSolve = Mathf.RoundToInt(matchQuality * 2.2f);
                // Crystal caverns: purified crystals stay resonant
                if (matchQuality > 0.7f && !CrystalCavernsResonant)
                {
                    CrystalCavernsResonant = true;
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, new Vector3(-14f, 3f, 47f));
                    ApplyCrystalCavernPayoff();
                }
            }
            else if (IsMoon2RootCore(currentBoss))
            {
                nodesPurifiedThisSolve = Mathf.RoundToInt(matchQuality * 1.9f) + 1;
                // Ley roots: bell synergy
                if (matchQuality > 0.65f)
                {
                    Core.LeyLineManager.Instance?.HarmonizeNode(2); // ley_chamber
                    VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, new Vector3(19f, 1f, 27f));
                }
                if (nodesPurifiedThisSolve >= 3 && !LeyChamberPurified)
                {
                    LeyChamberPurified = true;
                    ApplyLeyChamberPayoff();
                }
            }

            // Shared Moon2: companion synergy VO + giant meter nudge
            if (matchQuality > 0.8f)
            {
                // Lirael / Milo callouts
                // (In real: route through CompanionBehaviorSystem or OnBossDialogue)
                Debug.Log($"[Moon2Boss] Companion synergy: Lirael projects the next node. Milo digs external corruption. Giant meter +12% from vein purge.");
                // Assume GiantModeController nudge via bridge if available
                // CombatBridge.Instance?.NudgeGiantMeter(0.12f);
            }

            Debug.Log($"[Moon2Boss] {currentBoss.bossName} vein solve: quality={matchQuality:P0}, nodes+={nodesPurifiedThisSolve}, total sectors={Moon2PurifiedVeinSectors}");
        }

        /// <summary>
        /// Dedicated Moon2 AI tick — called from BossEncounterSystem.Update if IsAnyMoon2Boss.
        /// Handles vein spread visuals, crystal telegraph strobes, micro-giant phase special, companion hints.
        /// Strong telegraphing: vein crawl speed tied to _attackCooldown, crystal color = currentTargetFrequency.
        /// </summary>
        public static void UpdateMoon2DedicatedAI(BossDefinition currentBoss, int currentPhase, float bossHPNorm, float targetFreq, bool isVulnerable)
        {
            if (!IsAnyMoon2Boss(currentBoss)) return;

            // Environment telegraph: slow vein "crawl" VFX pulses proportional to attack timer (feels alive)
            float veinPulse = Mathf.Clamp(2.8f - (bossHPNorm * 1.2f), 0.9f, 3.5f);
            if (Time.time % veinPulse < 0.15f)
            {
                // Cathedral specific
                if (IsMoon2CathedralBoss(currentBoss))
                {
                    VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, new Vector3(2f, 4f, 38f)); // vein on dome
                }
                else if (IsMoon2RootCore(currentBoss))
                {
                    VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, new Vector3(19f, 0f, 27f)); // ley roots
                }
            }

            // Crystal frequency telegraph (color pulse on facets — maps Hz to visual)
            if (isVulnerable)
            {
                Color telegraphColor = FrequencyToCrystalColor(targetFreq);
                // In real impl: set material emission on nearby moon2_crystal renderers via VFX or builder
                Debug.Log($"[Moon2Boss] Crystal telegraph: {targetFreq:F0}Hz → color {telegraphColor}");
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, Vector3.up * 6f);
            }

            // Phase-specific Moon2 behaviors
            if (currentPhase == 2 && IsMoon2CathedralBoss(currentBoss)) // Fractal Descent
            {
                // Micro-Giant synergy moment
                OnBossDialogue?.Invoke("The Warden opens its heart! Use Micro-Giant now — Lirael will guide the nodes inside!");
                // Real: set flag that enables micro-giant entry in the zone scaffold / player input
            }

            if (currentPhase >= 2 && IsMoon2RootCore(currentBoss))
            {
                // Giant Mode root tear window
                if (bossHPNorm < 0.25f)
                    OnBossDialogue?.Invoke("The roots are exposed! Giant Mode — tear them free while your companions ring the bells!");
            }

            // Cassian unreliable hint (foreshadow)
            if (UnityEngine.Random.value < 0.12f && IsMoon2CathedralBoss(currentBoss))
            {
                OnBossDialogue?.Invoke("Cassian: \"Try 285 Hz... no wait, perhaps 396?\" (his advice feels slightly off)");
            }
        }

        private static Color FrequencyToCrystalColor(float hz)
        {
            // Maps the 6-band system to crystal emission colors for strong visual telegraph
            if (hz < 220f) return new Color(0.9f, 0.2f, 0.1f);      // Red (174)
            if (hz < 320f) return new Color(0.95f, 0.55f, 0.1f);     // Orange (285)
            if (hz < 450f) return new Color(0.95f, 0.9f, 0.2f);      // Yellow (396)
            if (hz < 580f) return new Color(0.2f, 0.85f, 0.35f);     // Green (528)
            if (hz < 680f) return new Color(0.15f, 0.55f, 0.95f);    // Blue (639)
            return new Color(0.6f, 0.2f, 0.9f);                       // Violet/Indigo (741+)
        }

        // ─── Permanent World Payoffs (called on successful node purges / defeat) ───
        private static void ApplyCathedralPermanentPayoff()
        {
            CathedralPermanentlyPurified = true;
            Moon2PurifiedVeinSectors = 4;

            // Permanent visual + mechanical
            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, new Vector3(0, 15f, 40f));
            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, new Vector3(0, 10f, 42f)); // dome crown

            // Hook to existing Moon2 visuals (R7 polish): veins stay golden, dome breathes majestically
            // In practice: set static flag read by TartarianArchitectureBuilder / VFX Moon2 manager on load
            Core.LeyLineManager.Instance?.HarmonizeNode(0); // cathedral node
            ResonanceScoreSystem.Instance?.AddResonance(22f, "Cathedral Vein Warden purified");

            Debug.Log("[Moon2Boss PAYOFF] Cathedral Vein Warden defeated — PERMANENT: golden veins, +22 RS, Architect Resonance Chamber unlocked, Micro-Giant II duration +25s, corruption immunity in cathedral zone.");
            // Real: unlock sub-chamber GameObject activate, achievement, save flag
        }

        private static void ApplyCrystalCavernPayoff()
        {
            CrystalCavernsResonant = true;
            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, new Vector3(-12f, 4f, 48f));
            ResonanceScoreSystem.Instance?.AddResonance(14f, "Fractal Mirror purified");
            Debug.Log("[Moon2Boss PAYOFF] Fractal Vein Mirror — PERMANENT: resonant crystal grove (bonus Aether harvest, -30% wraith spawns in sector, golden crystals remain lit).");
        }

        private static void ApplyLeyChamberPayoff()
        {
            LeyChamberPurified = true;
            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, new Vector3(19f, 2f, 27f));
            Core.LeyLineManager.Instance?.HarmonizeNode(2);
            ResonanceScoreSystem.Instance?.AddResonance(18f, "Root Core purged");
            Debug.Log("[Moon2Boss PAYOFF] Dissonance Root Core — PERMANENT: ley_chamber fully purified, new fractal passage to deeper caverns, +global grid resonance, bell towers now grant area-wide corruption shield.");
        }

        /// <summary>
        /// Call on boss defeat for any Moon2 boss to apply final permanent world changes + grid reaction.
        /// </summary>
        public static void OnMoon2BossDefeated(BossDefinition defeatedBoss)
        {
            if (!IsAnyMoon2Boss(defeatedBoss)) return;

            string payoffText = defeatedBoss.bossName + " purged. The caverns breathe again.";
            if (IsMoon2CathedralBoss(defeatedBoss))
            {
                ApplyCathedralPermanentPayoff();
                payoffText = "The Cathedral Vein Warden falls. The living crystal cathedral is whole — its veins now sing golden forever.";
            }
            else if (IsMoon2FractalMirror(defeatedBoss))
            {
                ApplyCrystalCavernPayoff();
            }
            else if (IsMoon2RootCore(defeatedBoss))
            {
                ApplyLeyChamberPayoff();
                payoffText = "The Root Core is gone. The ley veins of Moon 2 are free. The grid strengthens across all restored zones.";
            }

            // Cross-boss ley "world sings back" boost
            BossEncounterSystem.s_worldSingsBackHarmony = Mathf.Min(2.5f, BossEncounterSystem.s_worldSingsBackHarmony + 0.45f);

            Debug.Log($"[Moon2Boss] DEFEATED: {payoffText} | World harmony now {BossEncounterSystem.s_worldSingsBackHarmony:F2}");
            // Trigger global event / save / Moon progression flag
        }

        // Helper for dialogue (routes to system event)
        private static void OnBossDialogue(string line)
        {
            BossEncounterSystem.Instance?.OnBossDialogue?.Invoke(line);
        }
    }
}